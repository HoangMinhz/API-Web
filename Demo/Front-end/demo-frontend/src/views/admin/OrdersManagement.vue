<template>
  <div class="min-h-screen bg-gray-50 py-8">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <!-- Header -->
      <div class="mb-8">
        <h1 class="text-3xl font-bold text-gray-900">Orders Management</h1>
        <p class="mt-2 text-sm text-gray-600">Manage and track all customer orders in one place</p>
      </div>

      <!-- Error Alert -->
      <div v-if="hasError" class="mb-8">
        <div class="bg-red-50 border-l-4 border-red-400 p-4">
          <div class="flex">
            <div class="flex-shrink-0">
              <svg class="h-5 w-5 text-red-400" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
              </svg>
            </div>
            <div class="ml-3">
              <p class="text-sm text-red-700">
                {{ error }}
              </p>
            </div>
          </div>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="isLoading" class="flex justify-center items-center h-64">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
      </div>

      <template v-else>
        <!-- Statistics Cards -->
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
          <div class="bg-white rounded-lg shadow p-6">
            <div class="flex items-center">
              <div class="p-3 rounded-full bg-blue-100">
                <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
                </svg>
              </div>
              <div class="ml-4">
                <p class="text-sm font-medium text-gray-500">Total Orders</p>
                <p class="text-2xl font-semibold text-gray-900">{{ statistics?.totalOrders || 0 }}</p>
              </div>
            </div>
          </div>

          <div class="bg-white rounded-lg shadow p-6">
            <div class="flex items-center">
              <div class="p-3 rounded-full bg-green-100">
                <svg class="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <div class="ml-4">
                <p class="text-sm font-medium text-gray-500">Total Revenue</p>
                <p class="text-2xl font-semibold text-gray-900">{{ formatPrice(statistics?.totalRevenue || 0) }}</p>
              </div>
            </div>
          </div>

          <div class="bg-white rounded-lg shadow p-6">
            <div class="flex items-center">
              <div class="p-3 rounded-full bg-purple-100">
                <svg class="w-6 h-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 7h6m0 10v-3m-3 3h.01M9 17h.01M9 14h.01M12 14h.01M15 11h.01M12 11h.01M9 11h.01M7 21h10a2 2 0 002-2V5a2 2 0 00-2-2H7a2 2 0 00-2 2v14a2 2 0 002 2z" />
                </svg>
              </div>
              <div class="ml-4">
                <p class="text-sm font-medium text-gray-500">Average Order</p>
                <p class="text-2xl font-semibold text-gray-900">{{ formatPrice(statistics?.averageOrderValue || 0) }}</p>
              </div>
            </div>
          </div>

          <div class="bg-white rounded-lg shadow p-6">
            <div class="flex items-center">
              <div class="p-3 rounded-full bg-yellow-100">
                <svg class="w-6 h-6 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <div class="ml-4">
                <p class="text-sm font-medium text-gray-500">Pending Orders</p>
                <p class="text-2xl font-semibold text-gray-900">
                  {{ statistics?.ordersByStatus?.find(s => s.status === 0)?.count || 0 }}
                </p>
              </div>
            </div>
          </div>
        </div>

        <!-- Filters -->
        <div class="bg-white rounded-lg shadow mb-8">
          <div class="p-6">
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-2">Search</label>
                <input 
                  v-model="filters.searchTerm" 
                  @input="debounceSearch"
                  type="text" 
                  placeholder="Search orders..."
                  class="w-full px-4 py-2 rounded-lg border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-2">Status</label>
                <select 
                  v-model="filters.status"
                  @change="fetchOrders"
                  class="w-full px-4 py-2 rounded-lg border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  <option value="">All Statuses</option>
                  <option v-for="(label, value) in orderStatuses" :key="value" :value="parseInt(value)">
                    {{ label }}
                  </option>
                </select>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-2">From Date</label>
                <input 
                  type="date" 
                  v-model="filters.fromDate"
                  @change="fetchOrders"
                  class="w-full px-4 py-2 rounded-lg border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-2">To Date</label>
                <input 
                  type="date" 
                  v-model="filters.toDate"
                  @change="fetchOrders"
                  class="w-full px-4 py-2 rounded-lg border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
              </div>
            </div>
          </div>
        </div>

        <!-- Orders Table -->
        <div class="bg-white rounded-lg shadow overflow-hidden">
          <div class="overflow-x-auto">
            <table class="min-w-full divide-y divide-gray-200">
              <thead class="bg-gray-50">
                <tr>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Order ID</th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Date</th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Customer</th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Total</th>
                  <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
                </tr>
              </thead>
              <tbody class="bg-white divide-y divide-gray-200">
                <tr v-for="order in orders" :key="order.id" class="hover:bg-gray-50">
                  <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                    #{{ order.id }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {{ formatDate(order.orderDate) }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <div class="text-sm font-medium text-gray-900">{{ order.fullName }}</div>
                    <div class="text-sm text-gray-500">{{ order.userInfo.email }}</div>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <span :class="getStatusClass(order.status)">
                      {{ formatStatus(order.status) }}
                    </span>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {{ formatPrice(order.totalAmount) }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <button 
                      @click="viewOrderDetails(order)"
                      class="text-blue-600 hover:text-blue-900 mr-3"
                    >
                      View
                    </button>
                    <button 
                      @click="openStatusUpdate(order)"
                      class="text-indigo-600 hover:text-indigo-900"
                    >
                      Update
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Pagination -->
          <div class="bg-white px-4 py-3 border-t border-gray-200 sm:px-6">
            <div class="flex items-center justify-between">
              <div class="flex-1 flex justify-between sm:hidden">
                <button
                  @click="changePage(currentPage - 1)"
                  :disabled="currentPage === 1"
                  class="relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
                >
                  Previous
                </button>
                <button
                  @click="changePage(currentPage + 1)"
                  :disabled="currentPage === totalPages"
                  class="ml-3 relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
                >
                  Next
                </button>
              </div>
              <div class="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
                <div>
                  <p class="text-sm text-gray-700">
                    Showing page <span class="font-medium">{{ currentPage }}</span> of
                    <span class="font-medium">{{ totalPages }}</span>
                  </p>
                </div>
                <div>
                  <nav class="relative z-0 inline-flex rounded-md shadow-sm -space-x-px">
                    <button
                      @click="changePage(currentPage - 1)"
                      :disabled="currentPage === 1"
                      class="relative inline-flex items-center px-2 py-2 rounded-l-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50"
                    >
                      <span class="sr-only">Previous</span>
                      <svg class="h-5 w-5" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                        <path fill-rule="evenodd" d="M12.707 5.293a1 1 0 010 1.414L9.414 10l3.293 3.293a1 1 0 01-1.414 1.414l-4-4a1 1 0 010-1.414l4-4a1 1 0 011.414 0z" clip-rule="evenodd" />
                      </svg>
                    </button>
                    <button
                      @click="changePage(currentPage + 1)"
                      :disabled="currentPage === totalPages"
                      class="relative inline-flex items-center px-2 py-2 rounded-r-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50"
                    >
                      <span class="sr-only">Next</span>
                      <svg class="h-5 w-5" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                        <path fill-rule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clip-rule="evenodd" />
                      </svg>
                    </button>
                  </nav>
                </div>
              </div>
            </div>
          </div>
        </div>
      </template>
    </div>

    <!-- Order Details Modal -->
    <div v-if="selectedOrder" class="fixed inset-0 overflow-y-auto z-50">
      <div class="flex items-center justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
        <div class="fixed inset-0 transition-opacity" aria-hidden="true">
          <div class="absolute inset-0 bg-gray-500 opacity-75"></div>
        </div>
        <div class="inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-4xl sm:w-full">
          <div class="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
            <div class="sm:flex sm:items-start">
              <div class="w-full">
                <div class="flex justify-between items-center mb-4">
                  <h3 class="text-lg leading-6 font-medium text-gray-900">Order #{{ selectedOrder.id }}</h3>
                  <button @click="closeModal" class="text-gray-400 hover:text-gray-500">
                    <span class="sr-only">Close</span>
                    <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </div>
                <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div>
                    <h4 class="text-sm font-medium text-gray-900 mb-2">Customer Information</h4>
                    <div class="bg-gray-50 rounded-lg p-4">
                      <p class="text-sm text-gray-600"><span class="font-medium">Name:</span> {{ selectedOrder.fullName }}</p>
                      <p class="text-sm text-gray-600 mt-2"><span class="font-medium">Email:</span> {{ selectedOrder.userInfo.email }}</p>
                      <p class="text-sm text-gray-600 mt-2"><span class="font-medium">Phone:</span> {{ selectedOrder.phoneNumber }}</p>
                      <p class="text-sm text-gray-600 mt-2"><span class="font-medium">Address:</span> {{ selectedOrder.shippingAddress }}</p>
                    </div>
                  </div>
                  <div>
                    <h4 class="text-sm font-medium text-gray-900 mb-2">Order Status</h4>
                    <div class="bg-gray-50 rounded-lg p-4">
                      <p class="text-sm text-gray-600"><span class="font-medium">Current Status:</span></p>
                      <span :class="getStatusClass(selectedOrder.status)">
                        {{ formatStatus(selectedOrder.status) }}
                      </span>
                      <p class="text-sm text-gray-600 mt-4"><span class="font-medium">Order Date:</span> {{ formatDate(selectedOrder.orderDate) }}</p>
                    </div>
                  </div>
                </div>
                <div class="mt-6">
                  <h4 class="text-sm font-medium text-gray-900 mb-2">Order Items</h4>
                  <div class="bg-gray-50 rounded-lg overflow-hidden">
                    <table class="min-w-full divide-y divide-gray-200">
                      <thead class="bg-gray-100">
                        <tr>
                          <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Product</th>
                          <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">Quantity</th>
                          <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">Price</th>
                          <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">Total</th>
                        </tr>
                      </thead>
                      <tbody class="divide-y divide-gray-200">
                        <tr v-for="item in selectedOrder.orderItems" :key="item.id">
                          <td class="px-6 py-4">
                            <div class="flex items-center">
                              <img :src="item.product.imageUrl" :alt="item.product.name" class="h-10 w-10 rounded-md object-cover">
                              <span class="ml-4 text-sm text-gray-900">{{ item.product.name }}</span>
                            </div>
                          </td>
                          <td class="px-6 py-4 text-right text-sm text-gray-500">{{ item.quantity }}</td>
                          <td class="px-6 py-4 text-right text-sm text-gray-500">{{ formatPrice(item.unitPrice) }}</td>
                          <td class="px-6 py-4 text-right text-sm text-gray-900">{{ formatPrice(item.totalPrice) }}</td>
                        </tr>
                      </tbody>
                      <tfoot>
                        <tr>
                          <td colspan="3" class="px-6 py-4 text-right text-sm font-medium text-gray-900">Total</td>
                          <td class="px-6 py-4 text-right text-sm font-medium text-gray-900">{{ formatPrice(selectedOrder.totalAmount) }}</td>
                        </tr>
                      </tfoot>
                    </table>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Status Update Modal -->
    <div v-if="showStatusModal" class="fixed inset-0 overflow-y-auto z-50">
      <div class="flex items-center justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
        <div class="fixed inset-0 transition-opacity" aria-hidden="true">
          <div class="absolute inset-0 bg-gray-500 opacity-75"></div>
        </div>
        <div class="inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full">
          <div class="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
            <div class="sm:flex sm:items-start">
              <div class="w-full">
                <h3 class="text-lg leading-6 font-medium text-gray-900 mb-4">Update Order Status</h3>
                <select 
                  v-model="newStatus" 
                  class="w-full px-4 py-2 rounded-lg border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  <option v-for="(label, value) in orderStatuses" :key="value" :value="parseInt(value)">
                    {{ label }}
                  </option>
                </select>
              </div>
            </div>
          </div>
          <div class="bg-gray-50 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse">
            <button 
              @click="updateOrderStatus" 
              class="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-blue-600 text-base font-medium text-white hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 sm:ml-3 sm:w-auto sm:text-sm"
            >
              Update
            </button>
            <button 
              @click="closeStatusModal" 
              class="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm"
            >
              Cancel
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, computed, watch } from 'vue'
import { useStore } from 'vuex'
import debounce from 'lodash/debounce'
import formatPrice from '@/utils/formatPrice';

export default {
  name: 'OrdersManagement',
  setup() {
    const store = useStore()
    const orders = ref([])
    const statistics = ref(null)
    const totalPages = ref(1)
    const currentPage = ref(1)
    const selectedOrder = ref(null)
    const showModal = ref(false)
    const showStatusModal = ref(false)
    const newStatus = ref('')
    const orderToUpdate = ref(null)

    const filters = ref({
      searchTerm: '',
      status: '',
      fromDate: '',
      toDate: '',
      sortBy: 'date_desc',
      page: 1,
      pageSize: 10
    })

    // Watch for changes in filters
    watch(
      () => [filters.value.status, filters.value.fromDate, filters.value.toDate],
      () => {
        filters.value.page = 1 // Reset to first page when filters change
        fetchOrders()
        fetchStatistics() // Also update statistics when filters change
      }
    )

    const orderStatuses = {
      0: 'Pending',
      1: 'Processing',
      2: 'Shipped',
      3: 'Delivered',
      4: 'Cancelled'
    }

    // Computed properties
    const isLoading = computed(() => store.getters['orders/isLoading'])
    const hasError = computed(() => store.getters['orders/hasError'])
    const error = computed(() => store.getters['orders/getError'])

    const getStatusClass = (status) => {
      const statusMap = {
        0: 'bg-yellow-100 text-yellow-800', // Pending
        1: 'bg-blue-100 text-blue-800',     // Processing
        2: 'bg-purple-100 text-purple-800', // Shipped
        3: 'bg-green-100 text-green-800',   // Delivered
        4: 'bg-red-100 text-red-800'        // Cancelled
      }
      return `px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${statusMap[status] || 'bg-gray-100 text-gray-800'}`
    }

    const fetchOrders = async () => {
      try {
        const response = await store.dispatch('orders/getAdminOrders', filters.value)
        if (response && typeof response === 'object') {
          orders.value = Array.isArray(response.orders) ? response.orders : [];
          totalPages.value = response.totalPages || 1;
          currentPage.value = response.currentPage || 1;
        } else {
          console.error('Invalid response format:', response);
          orders.value = [];
          totalPages.value = 1;
          currentPage.value = 1;
        }
      } catch (error) {
        console.error('Error fetching orders:', error);
        orders.value = [];
        totalPages.value = 1;
        currentPage.value = 1;
      }
    }

    const fetchStatistics = async () => {
      try {
        statistics.value = await store.dispatch('orders/getOrderStatistics', {
          fromDate: filters.value.fromDate,
          toDate: filters.value.toDate
        })
      } catch (error) {
        console.error('Error fetching statistics:', error)
      }
    }

    const debounceSearch = debounce(() => {
      filters.value.page = 1
      fetchOrders()
    }, 300)

    const changePage = (page) => {
      filters.value.page = page
      fetchOrders()
    }

    const viewOrderDetails = (order) => {
      console.log('Order details:', order); // Debug log
      if (!order.status) {
        console.error('Order status is missing:', order);
      }
      selectedOrder.value = order
      showModal.value = true
    }

    const closeModal = () => {
      selectedOrder.value = null
      showModal.value = false
    }

    const openStatusUpdate = (order) => {
      console.log('Opening status update for order:', order)
      orderToUpdate.value = order
      newStatus.value = order.status
      showStatusModal.value = true
    }

    const closeStatusModal = () => {
      orderToUpdate.value = null
      showStatusModal.value = false
      // Clear any existing errors
      store.commit('orders/SET_ERROR', null)
    }

    const updateOrderStatus = async () => {
      try {
        console.log('Current order:', orderToUpdate.value)
        console.log('New status:', newStatus.value)
        
        if (typeof newStatus.value !== 'number') {
          console.error('Invalid status value:', newStatus.value)
          return
        }

        await store.dispatch('orders/updateOrderStatus', {
          orderId: orderToUpdate.value.id,
          status: newStatus.value
        })
        
        // Refresh data
        await fetchOrders()
        await fetchStatistics()
        
        closeStatusModal()
      } catch (error) {
        console.error('Error updating order status:', error)
        // Show error in UI
        const errorMessage = typeof error === 'string' ? error : 'An error occurred while updating the order status'
        store.commit('orders/SET_ERROR', errorMessage)
      }
    }

    const formatDate = (date) => {
      return new Date(date).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      })
    }

    const formatStatus = (status) => {
      return orderStatuses[status] || 'Unknown'
    }

    onMounted(() => {
      fetchOrders()
      fetchStatistics()
    })

    return {
      orders,
      statistics,
      filters,
      totalPages,
      currentPage,
      selectedOrder,
      showModal,
      showStatusModal,
      newStatus,
      orderToUpdate,
      orderStatuses,
      isLoading,
      hasError,
      error,
      getStatusClass,
      fetchOrders,
      fetchStatistics,
      debounceSearch,
      changePage,
      viewOrderDetails,
      closeModal,
      openStatusUpdate,
      closeStatusModal,
      updateOrderStatus,
      formatDate,
      formatStatus,
      formatPrice
    }
  }
}
</script> 