import { jwtDecode } from 'jwt-decode';
import api from '../../services/api';

export default {
  namespaced: true,
  state: () => ({
    token: localStorage.getItem('token') || null,
    user: JSON.parse(localStorage.getItem('user')) || null,
    roles: JSON.parse(localStorage.getItem('roles')) || [],
  }),
  mutations: {
    setToken(state, token) {
      state.token = token;
      localStorage.setItem('token', token);
    },
    setUser(state, user) {
      state.user = user;
      localStorage.setItem('user', JSON.stringify(user));
    },
    setRoles(state, roles) {
      state.roles = roles;
      localStorage.setItem('roles', JSON.stringify(roles));
    },
    clearUser(state) {
      state.token = null;
      state.user = null;
      state.roles = [];
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      localStorage.removeItem('roles');
    },
  },
  actions: {
    async login({ commit, dispatch }, response) {
      try {
        console.log('Processing login response:', response);
        
        if (!response.token) {
          console.error('No token in login response');
          return false;
        }

        const { token, user } = response;
        
        // Set token in axios defaults
        api.defaults.headers.common['Authorization'] = `Bearer ${token}`;
        
        // Process user data
        const userData = {
          id: user.id,
          username: user.userName,
          email: user.email,
        };

        console.log('Processed user data:', userData);

        // Get local cart items before clearing localStorage
        const localCart = JSON.parse(localStorage.getItem('cart') || '[]');

        // Commit changes
        commit('setToken', token);
        commit('setUser', userData);
        commit('setRoles', user.roles || ['User']);
        
        // Transfer cart items from localStorage to server
        if (localCart.length > 0) {
          try {
            // Add each item to the server cart
            for (const item of localCart) {
              await api.post('/Cart', { productId: item.productId, quantity: item.quantity }, {
                headers: {
                  Authorization: `Bearer ${token}`
                }
              });
            }
            // Clear local cart after successful transfer
            localStorage.removeItem('cart');
            // Fetch updated cart from server
            await dispatch('cart/fetchCart', null, { root: true });
          } catch (error) {
            console.error('Failed to transfer cart items:', error);
            // If transfer fails, keep the local cart
            dispatch('notification/showNotification', {
              type: 'warning',
              message: 'Failed to transfer cart items. Please try again.'
            }, { root: true });
          }
        }
        
        return true;
      } catch (error) {
        console.error('Login processing failed:', error);
        commit('clearUser');
        return false;
      }
    },
    logout({ commit }) {
      // Remove token from axios defaults
      delete api.defaults.headers.common['Authorization'];
      commit('clearUser');
    },
  },
  getters: {
    isAuthenticated: state => !!state.token,
    isAdmin: state => state.roles.includes('Admin'),
    currentUser: state => state.user,
  },
};