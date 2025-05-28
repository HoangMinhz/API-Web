import api from '../../services/api';
import axios from 'axios';

export default {
  namespaced: true,
  state: () => {
    // Initialize state from localStorage
    const localCart = JSON.parse(localStorage.getItem('cart') || '[]');
    return {
      items: localCart,
      loading: false,
      error: null
    };
  },
  mutations: {
    setItems(state, items) {
      state.items = items;
      // Save to localStorage if not authenticated
      if (!localStorage.getItem('token')) {
        localStorage.setItem('cart', JSON.stringify(items));
      }
    },
    setLoading(state, loading) {
      state.loading = loading;
    },
    setError(state, error) {
      state.error = error;
    },
    clearCart(state) {
      state.items = [];
      state.error = null;
    }
  },
  actions: {
    async fetchCart({ commit, rootState }) {
      commit('setLoading', true);
      commit('setError', null);
      
      try {
        const token = rootState.user.token;
        if (!token) {
          // If not authenticated, get cart from localStorage
          const localCart = JSON.parse(localStorage.getItem('cart') || '[]');
          // Fetch product details for each item in the cart
          const itemsWithProducts = await Promise.all(localCart.map(async (item) => {
            try {
              const response = await api.get(`/Product/get/${item.productId}`);
              return {
                ...item,
                product: response.data
              };
            } catch (error) {
              console.error(`Failed to fetch product ${item.productId}:`, error);
              return item;
            }
          }));
          commit('setItems', itemsWithProducts);
          return;
        }

        const response = await api.get('/Cart', {
          headers: {
            Authorization: `Bearer ${token}`
          }
        });
        commit('setItems', response.data?.items || []);
      } catch (error) {
        console.error('Fetch cart failed:', error);
        commit('setError', error.message || 'Failed to fetch cart');
      } finally {
        commit('setLoading', false);
      }
    },
    async addToCart({ commit, dispatch, rootState }, { productId, quantity = 1 }) {
      try {
        const token = rootState.user.token;
        if (!token) {
          // If not authenticated, add to localStorage
          const localCart = JSON.parse(localStorage.getItem('cart') || '[]');
          const existingItem = localCart.find(item => item.productId === productId);
          
          if (existingItem) {
            existingItem.quantity += quantity;
          } else {
            localCart.push({ productId, quantity });
          }
          
          localStorage.setItem('cart', JSON.stringify(localCart));
          
          // Fetch product details for all items in cart
          const itemsWithProducts = await Promise.all(localCart.map(async (item) => {
            try {
              const response = await api.get(`/Product/get/${item.productId}`);
              return {
                ...item,
                product: response.data
              };
            } catch (error) {
              console.error(`Failed to fetch product ${item.productId}:`, error);
              return item;
            }
          }));
          
          commit('setItems', itemsWithProducts);
          dispatch('notification/showNotification', {
            type: 'success',
            message: 'Item added to cart!'
          }, { root: true });
          return;
        }

        await api.post('/Cart', { productId, quantity }, {
          headers: {
            Authorization: `Bearer ${token}`
          }
        });
        await dispatch('fetchCart');
        dispatch('notification/showNotification', {
          type: 'success',
          message: 'Item added to cart!'
        }, { root: true });
      } catch (error) {
        console.error('Add to cart failed:', error);
        commit('setError', error.message || 'Failed to add item to cart');
        dispatch('notification/showNotification', {
          type: 'error',
          message: 'Failed to add item to cart'
        }, { root: true });
      }
    },
    async updateCartItem({ commit, dispatch, rootState }, { productId, quantity }) {
      try {
        // Validate quantity
        if (quantity < 1) {
          throw new Error('Quantity must be at least 1');
        }

        const token = rootState.user.token;
        if (!token) {
          // If not authenticated, update localStorage
          const localCart = JSON.parse(localStorage.getItem('cart') || '[]');
          const item = localCart.find(item => item.productId === productId);
          
          if (item) {
            // Check product stock
            const productResponse = await api.get(`/Product/get/${productId}`);
            const product = productResponse.data;
            
            if (quantity > product.stock) {
              throw new Error(`Only ${product.stock} items available in stock`);
            }

            item.quantity = quantity;
            localStorage.setItem('cart', JSON.stringify(localCart));
            
            // Fetch updated product details
            const itemsWithProducts = await Promise.all(localCart.map(async (item) => {
              try {
                const response = await api.get(`/Product/get/${item.productId}`);
                return {
                  ...item,
                  product: response.data
                };
              } catch (error) {
                console.error(`Failed to fetch product ${item.productId}:`, error);
                return item;
              }
            }));
            
            commit('setItems', itemsWithProducts);
            dispatch('notification/showNotification', {
              type: 'success',
              message: 'Cart updated!'
            }, { root: true });
          }
          return;
        }

        // For authenticated users, check stock before updating
        const productResponse = await api.get(`/Product/get/${productId}`);
        const product = productResponse.data;
        
        if (quantity > product.stock) {
          throw new Error(`Only ${product.stock} items available in stock`);
        }

        await api.put(`/Cart/${productId}`, quantity, {
          headers: {
            Authorization: `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        });
        await dispatch('fetchCart');
        dispatch('notification/showNotification', {
          type: 'success',
          message: 'Cart updated!'
        }, { root: true });
      } catch (error) {
        console.error('Update cart failed:', error);
        commit('setError', error.message || 'Failed to update cart item');
        dispatch('notification/showNotification', {
          type: 'error',
          message: error.message || 'Failed to update cart item'
        }, { root: true });
      }
    },
    async removeFromCart({ commit, dispatch, rootState }, productId) {
      try {
        const token = rootState.user.token;
        if (!token) {
          // If not authenticated, remove from localStorage
          const localCart = JSON.parse(localStorage.getItem('cart') || '[]');
          const updatedCart = localCart.filter(item => item.productId !== productId);
          localStorage.setItem('cart', JSON.stringify(updatedCart));
          
          // Fetch updated product details
          const itemsWithProducts = await Promise.all(updatedCart.map(async (item) => {
            try {
              const response = await api.get(`/Product/get/${item.productId}`);
              return {
                ...item,
                product: response.data
              };
            } catch (error) {
              console.error(`Failed to fetch product ${item.productId}:`, error);
              return item;
            }
          }));
          
          commit('setItems', itemsWithProducts);
          dispatch('notification/showNotification', {
            type: 'success',
            message: 'Item removed from cart!'
          }, { root: true });
          return;
        }

        await api.delete(`/Cart/${productId}`, {
          headers: {
            Authorization: `Bearer ${token}`
          }
        });
        await dispatch('fetchCart');
        dispatch('notification/showNotification', {
          type: 'success',
          message: 'Item removed from cart!'
        }, { root: true });
      } catch (error) {
        console.error('Remove from cart failed:', error);
        commit('setError', error.message || 'Failed to remove item from cart');
        dispatch('notification/showNotification', {
          type: 'error',
          message: error.message || 'Failed to remove item from cart'
        }, { root: true });
      }
    },
    async clearCart({ commit, rootState }) {
      try {
        const token = rootState.user.token;
        if (token) {
          try {
            // If authenticated, clear cart on server
            await api.delete('/Cart/clear', {
              headers: {
                Authorization: `Bearer ${token}`
              }
            });
          } catch (error) {
            // If the cart doesn't exist (404) or other error, we can still proceed
            // as we want to clear the local cart anyway
            console.warn('Server cart clear failed:', error);
          }
        }
        // Clear local cart regardless of authentication status
        localStorage.removeItem('cart');
        commit('clearCart');
      } catch (error) {
        console.error('Clear cart failed:', error);
        commit('setError', error.message || 'Failed to clear cart');
      }
    },
    async checkout({ commit, state }, orderData) {
      try {
        commit('setLoading', true);
        commit('setError', null);

        // If user is authenticated, send order to backend
        if (state.isAuthenticated) {
          const response = await axios.post('http://localhost:5285/api/Order', orderData);
          if (response.data) {
            // Clear the cart after successful checkout
            commit('setItems', []);
            localStorage.removeItem('cart');
          }
        } else {
          // For guest users, just clear the cart
          commit('setItems', []);
          localStorage.removeItem('cart');
        }
      } catch (error) {
        console.error('Checkout error:', error);
        commit('setError', 'Failed to process your order. Please try again.');
        throw error;
      } finally {
        commit('setLoading', false);
      }
    }
  },
  getters: {
    items: state => state.items,
    loading: state => state.loading,
    error: state => state.error,
    totalItems: state => state.items.reduce((total, item) => total + item.quantity, 0),
    subtotal: state => {
        const total = state.items.reduce((total, item) => {
            const itemTotal = (item.product?.price || 0) * item.quantity;
            return total + Math.round(itemTotal * 100) / 100;
        }, 0);
        return Math.round(total * 100) / 100;
    },
    tax: state => {
        const tax = state.items.reduce((total, item) => {
            const itemTotal = (item.product?.price || 0) * item.quantity;
            return total + Math.round(itemTotal * 0.1 * 100) / 100;
        }, 0);
        return Math.round(tax * 100) / 100;
    },
    total: state => {
        const subtotal = state.items.reduce((total, item) => {
            const itemTotal = (item.product?.price || 0) * item.quantity;
            return total + Math.round(itemTotal * 100) / 100;
        }, 0);
        const tax = Math.round(subtotal * 0.1 * 100) / 100;
        return Math.round((subtotal + tax) * 100) / 100;
    }
  }
};