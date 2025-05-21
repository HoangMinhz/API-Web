import api from '../../services/api'

const state = {
  categories: [],
  loading: false,
  error: null
}

const mutations = {
  SET_CATEGORIES(state, categories) {
    state.categories = categories
  },
  SET_LOADING(state, loading) {
    state.loading = loading
  },
  SET_ERROR(state, error) {
    state.error = error
  }
}

const actions = {
  async getAllCategories({ commit }) {
    try {
      commit('SET_LOADING', true)
      commit('SET_ERROR', null)
      const response = await api.get('/Category/list')
      commit('SET_CATEGORIES', response.data)
      return response.data
    } catch (error) {
      const message = error.response?.data?.message || 'Error fetching categories'
      commit('SET_ERROR', message)
      throw error
    } finally {
      commit('SET_LOADING', false)
    }
  }
}

const getters = {
  getCategories: state => state.categories,
  isLoading: state => state.loading,
  getError: state => state.error
}

export default {
  namespaced: true,
  state,
  mutations,
  actions,
  getters
} 