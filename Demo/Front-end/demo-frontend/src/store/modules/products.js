import api from '../../services/api'

const state = {
  products: [],
  loading: false,
  error: null
}

const mutations = {
  SET_PRODUCTS(state, products) {
    state.products = products
  },
  SET_LOADING(state, loading) {
    state.loading = loading
  },
  SET_ERROR(state, error) {
    state.error = error
  },
  ADD_PRODUCT(state, product) {
    state.products.push(product)
  },
  UPDATE_PRODUCT(state, updatedProduct) {
    const index = state.products.findIndex(p => p.id === updatedProduct.id)
    if (index !== -1) {
      state.products.splice(index, 1, updatedProduct)
    }
  },
  DELETE_PRODUCT(state, productId) {
    state.products = state.products.filter(p => p.id !== productId)
  }
}

const actions = {
  async getAllProducts({ commit }) {
    try {
      commit('SET_LOADING', true)
      commit('SET_ERROR', null)
      const response = await api.get('/Product/list')
      commit('SET_PRODUCTS', response.data)
      return response.data
    } catch (error) {
      const message = error.response?.data?.message || 'Error fetching products'
      commit('SET_ERROR', message)
      throw error
    } finally {
      commit('SET_LOADING', false)
    }
  },

  async addProduct({ commit }, productData) {
    try {
      commit('SET_LOADING', true)
      commit('SET_ERROR', null)
      const response = await api.post('/Product', productData)
      commit('ADD_PRODUCT', response.data)
      return response.data
    } catch (error) {
      const message = error.response?.data?.message || 'Error adding product'
      commit('SET_ERROR', message)
      throw error
    } finally {
      commit('SET_LOADING', false)
    }
  },

  async updateProduct({ commit }, { id, productData }) {
    try {
      commit('SET_LOADING', true)
      commit('SET_ERROR', null)
      const response = await api.put(`/Product/${id}`, productData)
      commit('UPDATE_PRODUCT', response.data)
      return response.data
    } catch (error) {
      const message = error.response?.data?.message || 'Error updating product'
      commit('SET_ERROR', message)
      throw error
    } finally {
      commit('SET_LOADING', false)
    }
  },

  async deleteProduct({ commit }, productId) {
    try {
      commit('SET_LOADING', true)
      commit('SET_ERROR', null)
      await api.delete(`/Product/${productId}`)
      commit('DELETE_PRODUCT', productId)
    } catch (error) {
      const message = error.response?.data?.message || 'Error deleting product'
      commit('SET_ERROR', message)
      throw error
    } finally {
      commit('SET_LOADING', false)
    }
  }
}

const getters = {
  getProducts: state => state.products,
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