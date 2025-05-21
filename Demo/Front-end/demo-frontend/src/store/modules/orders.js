import api from '../../services/api'

// Add reverse mapping for status
const StatusToNumber = {
  'Pending': 0,
  'Processing': 1,
  'Shipped': 2,
  'Delivered': 3,
  'Cancelled': 4
}

const OrderStatus = {
  0: 'Pending',
  1: 'Processing',
  2: 'Shipped',
  3: 'Delivered',
  4: 'Cancelled'
}

const state = {
  orders: [],
  totalPages: 1,
  currentPage: 1,
  statistics: null,
  loading: false,
  error: null
}

const mutations = {
  SET_ORDERS(state, { orders, totalPages, currentPage }) {
    state.orders = orders
    state.totalPages = totalPages
    state.currentPage = currentPage
  },
  SET_STATISTICS(state, statistics) {
    state.statistics = statistics
  },
  SET_LOADING(state, loading) {
    state.loading = loading
  },
  SET_ERROR(state, error) {
    state.error = error
  },
  UPDATE_ORDER_STATUS(state, { orderId, status }) {
    const order = state.orders.find(o => o.id === orderId)
    if (order) {
      order.status = status
    }
  }
}

const actions = {
  async getAdminOrders({ commit }, filters) {
    try {
      commit('SET_LOADING', true)
      commit('SET_ERROR', null)

      const queryParams = new URLSearchParams({
        page: filters.page || 1,
        pageSize: filters.pageSize || 10,
        sortBy: filters.sortBy || 'date_desc',
        searchTerm: filters.searchTerm || ''
      })

      // Only append status if it's a valid number
      if (filters.status !== undefined && filters.status !== '') {
        const statusValue = parseInt(filters.status)
        if (!isNaN(statusValue) && statusValue >= 0 && statusValue <= 4) {
          queryParams.append('status', statusValue)
        }
      }
      
      if (filters.fromDate) queryParams.append('fromDate', filters.fromDate)
      if (filters.toDate) queryParams.append('toDate', filters.toDate)

      const response = await api.get(`/Order/admin/orders?${queryParams}`)
      console.log('API Response:', response.data) // Debug log
      
      // Validate and process orders data
      const orders = response.data.orders.map(order => {
        console.log('Processing order:', order) // Debug log
        return {
          ...order,
          status: order.status
        }
      })
      
      commit('SET_ORDERS', {
        orders: orders,
        totalPages: response.data.totalPages || 1,
        currentPage: response.data.currentPage || 1
      })

      return {
        orders: orders,
        totalPages: response.data.totalPages || 1,
        currentPage: response.data.currentPage || 1
      }
    } catch (error) {
      console.error('Error in getAdminOrders:', error.response?.data || error)
      const errorMessage = error.response?.data?.message || 'Error fetching orders'
      commit('SET_ERROR', errorMessage)
      throw error
    } finally {
      commit('SET_LOADING', false)
    }
  },

  async getOrderStatistics({ commit }, { fromDate, toDate }) {
    try {
      commit('SET_LOADING', true)
      commit('SET_ERROR', null)

      const queryParams = new URLSearchParams()
      if (fromDate) queryParams.append('fromDate', fromDate)
      if (toDate) queryParams.append('toDate', toDate)

      const response = await api.get(`/Order/admin/orders/statistics?${queryParams}`)
      commit('SET_STATISTICS', response.data)
      return response.data
    } catch (error) {
      const errorMessage = error.response?.data?.message || 'Error fetching statistics'
      commit('SET_ERROR', errorMessage)
      throw error
    } finally {
      commit('SET_LOADING', false)
    }
  },

  async updateOrderStatus({ commit }, { orderId, status }) {
    try {
      commit('SET_LOADING', true)
      commit('SET_ERROR', null)

      // Validate status is a number
      const numericStatus = parseInt(status)
      if (isNaN(numericStatus) || numericStatus < 0 || numericStatus > 4) {
        throw new Error('Invalid status value')
      }

      console.log('Updating status with payload:', { orderId, status: numericStatus })

      // Send status as a raw number
      const response = await api.put(`/Order/${orderId}/status`, {
        Status: numericStatus // Match the C# model property name
      })
      
      console.log('Update status response:', response.data)
      
      if (response.data && response.data.status !== undefined) {
        // Update local state if needed
        commit('UPDATE_ORDER_STATUS', { 
          orderId, 
          status: response.data.status 
        })
      }
      
      return response.data
    } catch (error) {
      console.error('Error in updateOrderStatus:', error.response?.data?.message || error.message)
      
      let errorMessage = 'Error updating order status'
      if (error.response?.data?.message) {
        errorMessage = error.response.data.message
      } else if (error.message === 'Invalid status value') {
        errorMessage = 'Invalid status value provided'
      }
      
      commit('SET_ERROR', errorMessage)
      throw errorMessage
    } finally {
      commit('SET_LOADING', false)
    }
  }
}

const getters = {
  isLoading: state => state.loading,
  hasError: state => state.error !== null,
  getError: state => state.error,
  getOrders: state => state.orders,
  getStatistics: state => state.statistics
}

export default {
  namespaced: true,
  state,
  mutations,
  actions,
  getters
} 