import api from '../../services/api';

export default {
  namespaced: true,
  state: () => ({
    items: [],
    loading: false,
    error: null
  }),
  mutations: {
    setItems(state, items) {
      state.items = items;
    },
    addItem(state, item) {
      if (!state.items.some(i => i.id === item.id)) {
        state.items.push(item);
      }
    },
    removeItem(state, productId) {
      state.items = state.items.filter(item => item.id !== productId);
    },
    setLoading(state, loading) {
      state.loading = loading;
    },
    setError(state, error) {
      state.error = error;
    }
  },
  actions: {
    async fetchWishlist({ commit, rootState }) {
      commit('setLoading', true);
      commit('setError', null);
      
      try {
        const token = rootState.user.token;
        if (!token) {
          // If not authenticated, get wishlist from localStorage
          const localWishlist = JSON.parse(localStorage.getItem('wishlist') || '[]');
          commit('setItems', localWishlist);
          return;
        }

        const response = await api.get('/Wishlist', {
          headers: {
            Authorization: `Bearer ${token}`
          }
        });
        commit('setItems', response.data || []);
      } catch (error) {
        console.error('Fetch wishlist failed:', error);
        commit('setError', error.message || 'Failed to fetch wishlist');
      } finally {
        commit('setLoading', false);
      }
    },
    async addToWishlist({ commit, dispatch, rootState }, productId) {
      try {
        const token = rootState.user.token;
        if (!token) {
          // If not authenticated, add to localStorage
          const localWishlist = JSON.parse(localStorage.getItem('wishlist') || '[]');
          if (!localWishlist.includes(productId)) {
            localWishlist.push(productId);
            localStorage.setItem('wishlist', JSON.stringify(localWishlist));
            commit('addItem', { id: productId });
          }
          dispatch('notification/showNotification', {
            type: 'success',
            message: 'Added to wishlist!'
          }, { root: true });
          return;
        }

        await api.post('/Wishlist', { productId }, {
          headers: {
            Authorization: `Bearer ${token}`
          }
        });
        await dispatch('fetchWishlist');
        dispatch('notification/showNotification', {
          type: 'success',
          message: 'Added to wishlist!'
        }, { root: true });
      } catch (error) {
        console.error('Add to wishlist failed:', error);
        dispatch('notification/showNotification', {
          type: 'error',
          message: 'Failed to add to wishlist'
        }, { root: true });
      }
    },
    async removeFromWishlist({ commit, dispatch, rootState }, productId) {
      try {
        const token = rootState.user.token;
        if (!token) {
          // If not authenticated, remove from localStorage
          const localWishlist = JSON.parse(localStorage.getItem('wishlist') || '[]');
          const updatedWishlist = localWishlist.filter(id => id !== productId);
          localStorage.setItem('wishlist', JSON.stringify(updatedWishlist));
          commit('removeItem', productId);
          dispatch('notification/showNotification', {
            type: 'success',
            message: 'Removed from wishlist!'
          }, { root: true });
          return;
        }

        await api.delete(`/Wishlist/${productId}`, {
          headers: {
            Authorization: `Bearer ${token}`
          }
        });
        await dispatch('fetchWishlist');
        dispatch('notification/showNotification', {
          type: 'success',
          message: 'Removed from wishlist!'
        }, { root: true });
      } catch (error) {
        console.error('Remove from wishlist failed:', error);
        dispatch('notification/showNotification', {
          type: 'error',
          message: 'Failed to remove from wishlist'
        }, { root: true });
      }
    }
  },
  getters: {
    items: state => state.items,
    loading: state => state.loading,
    error: state => state.error,
    isInWishlist: state => productId => state.items.some(item => item.id === productId)
  }
}; 