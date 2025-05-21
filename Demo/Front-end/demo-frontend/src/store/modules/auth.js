import axios from 'axios'

const state = {
  user: null,
  isAuthenticated: false,
  token: localStorage.getItem('token') || null
}

const getters = {
  isAuthenticated: state => state.isAuthenticated,
  currentUser: state => state.user,
  hasPurchasedProduct: state => productId => {
    return state.user?.hasPurchasedProducts?.includes(productId) || false
  }
}

const actions = {
  async login({ commit }, credentials) {
    try {
      const response = await axios.post('/api/auth/login', credentials)
      const { token, user } = response.data
      
      // Store user roles from the response
      if (user && user.roles) {
        user.roles = user.roles
      }
      
      // Fetch user's purchased products
      const userResponse = await axios.get(`/api/user/${user.id}/purchased-products`)
      user.hasPurchasedProducts = userResponse.data
      
      localStorage.setItem('token', token)
      axios.defaults.headers.common['Authorization'] = `Bearer ${token}`
      
      commit('setUser', user)
      commit('setToken', token)
      return response
    } catch (error) {
      throw error
    }
  },

  async register({ commit }, userData) {
    try {
      const response = await axios.post('/api/auth/register', userData)
      return response
    } catch (error) {
      throw error
    }
  },

  async logout({ commit }) {
    localStorage.removeItem('token')
    delete axios.defaults.headers.common['Authorization']
    commit('setUser', null)
    commit('setToken', null)
  },

  async fetchUser({ commit }) {
    try {
      console.log('Gọi fetchUser, token:', localStorage.getItem('token'));
      const response = await axios.get('/api/auth/me')
      console.log('Kết quả /api/auth/me:', response.data);
      const user = response.data
      // Đảm bảo roles luôn là mảng
      user.roles = user.roles || response.data.roles || []
      // Fetch user's purchased products
      const userResponse = await axios.get(`/api/user/${user.id}/purchased-products`)
      user.hasPurchasedProducts = userResponse.data
      commit('setUser', user)
      console.log('User sau khi set:', user);
      return response
    } catch (error) {
      console.error('Lỗi fetchUser:', error);
      throw error
    }
  },

  async updateUserProfile({ commit }, userData) {
    try {
      const response = await axios.put('/api/user/profile', userData)
      commit('setUser', response.data)
      return response
    } catch (error) {
      throw error
    }
  }
}

const mutations = {
  setUser(state, user) {
    state.user = user
    state.isAuthenticated = !!user
  },
  setToken(state, token) {
    state.token = token
  }
}

export default {
  namespaced: true,
  state,
  getters,
  actions,
  mutations
} 