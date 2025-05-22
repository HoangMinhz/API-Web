<template>
  <div class="min-h-screen bg-gray-50 py-12">
    <div class="container mx-auto px-4">
      <div class="max-w-6xl mx-auto">
        <div class="flex justify-between items-center mb-8">
          <h1 class="text-3xl font-bold text-gray-900">My Orders</h1>
          <router-link
            to="/shop"
            class="px-4 py-2 bg-white text-gray-700 rounded-lg border border-gray-300 hover:bg-gray-50"
          >
            Continue Shopping
          </router-link>
        </div>

        <!-- Loading State -->
        <div v-if="loading" class="flex justify-center items-center py-12">
          <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
        </div>

        <!-- Error State -->
        <div v-else-if="error" class="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-lg mb-6">
          {{ error }}
        </div>

        <!-- Empty State -->
        <div v-else-if="!orders.length" class="text-center py-12">
          <div class="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <svg class="w-8 h-8 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
            </svg>
          </div>
          <h2 class="text-xl font-semibold text-gray-900 mb-2">No Orders Yet</h2>
          <p class="text-gray-600 mb-4">You haven't placed any orders yet.</p>
          <router-link
            to="/shop"
            class="inline-flex items-center px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            Start Shopping
          </router-link>
        </div>

        <!-- Orders List -->
        <div v-else class="space-y-6">
          <div v-for="order in orders" :key="order.id" class="bg-white rounded-lg shadow-sm overflow-hidden">
            <!-- Order Header -->
            <div class="p-6 border-b border-gray-200">
              <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between">
                <div class="mb-4 sm:mb-0">
                  <h3 class="text-lg font-semibold text-gray-900">Order #{{ order.id }}</h3>
                  <p class="text-sm text-gray-500">{{ formatDate(order.orderDate) }}</p>
                </div>
                <div class="flex flex-col items-end">
                  <span
                    :class="{
                      'px-3 py-1 rounded-full text-sm font-medium': true,
                      'bg-yellow-100 text-yellow-800': getStatusText(order.status) === 'Pending',
                      'bg-blue-100 text-blue-800': getStatusText(order.status) === 'Processing',
                      'bg-green-100 text-green-800': getStatusText(order.status) === 'Shipped',
                      'bg-purple-100 text-purple-800': getStatusText(order.status) === 'Delivered',
                      'bg-red-100 text-red-800': getStatusText(order.status) === 'Cancelled'
                    }"
                  >
                    {{ getStatusText(order.status) }}
                  </span>
                  <p class="text-xs text-gray-500 mt-1">
                    {{ getStatusDescription(order.status) }}
                  </p>
                </div>
              </div>
            </div>

            <!-- Order Items -->
            <div class="p-6">
              <div class="space-y-4">
                <div v-for="item in order.orderItems" :key="item.id" class="flex items-center justify-between">
                  <div class="flex items-center space-x-4">
                    <img
                      :src="item.product?.imageUrl"
                      :alt="item.product?.name"
                      class="w-16 h-16 rounded-md object-cover"
                    />
                    <div>
                      <p class="text-sm font-medium text-gray-900">{{ item.product?.name }}</p>
                      <p class="text-sm text-gray-500">{{ item.quantity }} × {{ formatPrice(item.unitPrice) }}</p>
                    </div>
                  </div>
                  <p class="text-sm font-medium text-gray-900 ml-auto mr-4">{{ formatPrice(item.totalPrice) }}</p>
                  <button 
                    v-if="getStatusText(order.status) === 'Delivered'" 
                    @click="rateProduct(item.product?.id)"
                    class="px-3 py-1 text-xs bg-yellow-100 text-yellow-800 rounded-full hover:bg-yellow-200"
                  >
                    Rate Product
                  </button>
                </div>
              </div>
            </div>

            <!-- Order Footer -->
            <div class="px-6 py-4 bg-gray-50 border-t border-gray-200">
              <div class="flex items-center justify-between">
              <div class="text-sm text-gray-500">
                <p>{{ order.orderItems.length }} items</p>
              </div>
              <div class="flex items-center space-x-4">
                <p class="text-lg font-semibold text-gray-900">Total: {{ formatPrice(order.totalAmount) }}</p>
                <button
                @click="toggleOrderDetails(order.id)"
                class="px-4 py-2 bg-white text-gray-700 rounded-lg border border-gray-300 hover:bg-gray-50"
                >
                {{ expandedOrderId === order.id ? 'Hide Details' : 'View Details' }}
                </button>
                <button
                v-if="getStatusText(order.status) === 'Pending' || getStatusText(order.status) === 'Processing'"
                @click="cancelOrder(order.id)"
                class="px-4 py-2 bg-red-100 text-red-700 rounded-lg border border-red-300 hover:bg-red-200"
                >
                Cancel Order
                </button>
              </div>
              </div>
            </div>

            <!-- Order Details (Expanded) -->
            <div v-if="expandedOrderId === order.id" class="px-6 py-4 bg-gray-50 border-t border-gray-200">
              <div class="space-y-6">
                <!-- Status Timeline -->
                <div class="py-4">
                  <h4 class="text-sm font-medium text-gray-900 mb-4">Order Status</h4>
                  <div class="relative">
                    <!-- Status Line -->
                    <div class="absolute left-7 top-0 h-full w-0.5 bg-gray-200"></div>
                    
                    <!-- Status Steps -->
                    <div class="space-y-6 relative">
                      <!-- Pending -->
                      <div class="flex items-start">
                        <div :class="`rounded-full h-3.5 w-3.5 flex items-center justify-center p-3 mr-4 ${getStatusClass('Pending', order.status)}`">
                          <svg v-if="isStatusCompleted('Pending', order.status)" class="h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
                          </svg>
                        </div>
                        <div>
                          <h5 :class="`text-sm font-medium ${getStatusTextClass('Pending', order.status)}`">Order Placed</h5>
                          <p class="text-xs text-gray-500">We've received your order</p>
                        </div>
                      </div>
                      
                      <!-- Processing -->
                      <div class="flex items-start">
                        <div :class="`rounded-full h-3.5 w-3.5 flex items-center justify-center p-3 mr-4 ${getStatusClass('Processing', order.status)}`">
                          <svg v-if="isStatusCompleted('Processing', order.status)" class="h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
                          </svg>
                        </div>
                        <div>
                          <h5 :class="`text-sm font-medium ${getStatusTextClass('Processing', order.status)}`">Processing</h5>
                          <p class="text-xs text-gray-500">Preparing your order</p>
                        </div>
                      </div>
                      
                      <!-- Shipped -->
                      <div class="flex items-start">
                        <div :class="`rounded-full h-3.5 w-3.5 flex items-center justify-center p-3 mr-4 ${getStatusClass('Shipped', order.status)}`">
                          <svg v-if="isStatusCompleted('Shipped', order.status)" class="h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
                          </svg>
                        </div>
                        <div>
                          <h5 :class="`text-sm font-medium ${getStatusTextClass('Shipped', order.status)}`">Shipped</h5>
                          <p class="text-xs text-gray-500">Your order is on the way</p>
                        </div>
                      </div>
                      
                      <!-- Delivered -->
                      <div class="flex items-start">
                        <div :class="`rounded-full h-3.5 w-3.5 flex items-center justify-center p-3 mr-4 ${getStatusClass('Delivered', order.status)}`">
                          <svg v-if="isStatusCompleted('Delivered', order.status)" class="h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
                          </svg>
                        </div>
                        <div>
                          <h5 :class="`text-sm font-medium ${getStatusTextClass('Delivered', order.status)}`">Delivered</h5>
                          <p class="text-xs text-gray-500">Order completed successfully</p>
                        </div>
                      </div>
                      
                      <!-- Cancelled (only shown if order is cancelled) -->
                      <div v-if="getStatusText(order.status) === 'Cancelled'" class="flex items-start">
                        <div class="rounded-full h-3.5 w-3.5 flex items-center justify-center p-3 mr-4 bg-red-600">
                          <svg class="h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                          </svg>
                        </div>
                        <div>
                          <h5 class="text-sm font-medium text-red-800">Cancelled</h5>
                          <p class="text-xs text-gray-500">This order has been cancelled</p>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                
                <!-- Existing address information -->
                <div class="grid grid-cols-2 gap-4">
                  <div>
                    <p class="text-sm font-medium text-gray-500">Shipping Address</p>
                    <p class="text-sm text-gray-900">{{ order.shippingAddress }}</p>
                  </div>
                  <div>
                    <p class="text-sm font-medium text-gray-500">Phone Number</p>
                    <p class="text-sm text-gray-900">{{ order.phoneNumber }}</p>
                  </div>
                  <div>
                    <p class="text-sm font-medium text-gray-500">Full Name</p>
                    <p class="text-sm text-gray-900">{{ order.fullName }}</p>
                  </div>
                  <div v-if="order.notes">
                    <p class="text-sm font-medium text-gray-500">Notes</p>
                    <p class="text-sm text-gray-900">{{ order.notes }}</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import api from '../services/api';
import formatPrice from '../utils/formatPrice';
import store from '../store';
import router from '../router';

export default {
  name: 'OrdersPage',
  data() {
    return {
      orders: [],
      loading: true,
      error: null,
      expandedOrderId: null
    };
  },
  async created() {
    await this.fetchOrders();
  },
  methods: {
    formatPrice,
    formatDate(date) {
      if (!date) return '';
      return new Date(date).toLocaleDateString('vi-VN', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      });
    },
    async fetchOrders() {
      this.loading = true;
      this.error = null;
      try {
        console.log('Fetching orders...');
        const response = await api.get('/Order');
        console.log('Orders response:', response.data);
        this.orders = response.data;
      } catch (error) {
        console.error('Failed to fetch orders:', error);
        this.error = error.message || 'Failed to load orders. Please try again later.';
        
        // Check if user is logged in
        const token = store.state.user.token;
        if (!token) {
          this.error = 'Please login to view your orders.';
          router.push('/login');
          return;
        }
      } finally {
        this.loading = false;
      }
    },
    toggleOrderDetails(orderId) {
      this.expandedOrderId = this.expandedOrderId === orderId ? null : orderId;
    },
    getStatusText(statusCode) {
      // Map numeric status codes to text
      const statusMap = {
        0: 'Pending',
        1: 'Processing',
        2: 'Shipped',
        3: 'Delivered',
        4: 'Cancelled'
      };
      
      // If it's already a string or not found in the map, return as is
      return statusMap[statusCode] || statusCode;
    },
    getStatusDescription(status) {
      // Convert to text first if it's a number
      const statusText = this.getStatusText(status);
      
      switch (statusText) {
        case 'Pending':
          return 'Your order has been received and is awaiting processing';
        case 'Processing':
          return 'Your order is being prepared';
        case 'Shipped':
          return 'Your order is on the way';
        case 'Delivered':
          return 'Your order has been delivered';
        case 'Cancelled':
          return 'This order has been cancelled';
        default:
          return '';
      }
    },
    getStatusClass(step, currentStatus) {
      // Convert current status to text if it's a number
      const currentStatusText = this.getStatusText(currentStatus);
      
      if (currentStatusText === 'Cancelled') {
        return 'bg-gray-300'; // All steps are gray if order is cancelled
      }
      
      const statusOrder = ['Pending', 'Processing', 'Shipped', 'Delivered'];
      const currentIndex = statusOrder.indexOf(currentStatusText);
      const stepIndex = statusOrder.indexOf(step);
      
      if (currentIndex > stepIndex) return 'bg-green-600';
      if (currentIndex === stepIndex) return 'bg-blue-600';
      return 'bg-gray-300';
    },
    getStatusTextClass(step, currentStatus) {
      // Convert current status to text if it's a number
      const currentStatusText = this.getStatusText(currentStatus);
      
      if (currentStatusText === 'Cancelled') {
        return step === 'Cancelled' ? 'text-red-800' : 'text-gray-500';
      }
      
      const statusOrder = ['Pending', 'Processing', 'Shipped', 'Delivered'];
      const currentIndex = statusOrder.indexOf(currentStatusText);
      const stepIndex = statusOrder.indexOf(step);
      
      if (currentIndex >= stepIndex) return 'text-gray-900';
      return 'text-gray-500';
    },
    isStatusCompleted(step, currentStatus) {
      // Convert current status to text if it's a number
      const currentStatusText = this.getStatusText(currentStatus);
      
      if (currentStatusText === 'Cancelled') {
        return false; // No checkmarks for cancelled orders
      }
      
      const statusOrder = ['Pending', 'Processing', 'Shipped', 'Delivered'];
      const currentIndex = statusOrder.indexOf(currentStatusText);
      const stepIndex = statusOrder.indexOf(step);

      return currentIndex > stepIndex;
    },
    async cancelOrder(orderId) {
      if (!confirm('Are you sure you want to cancel this order?')) {
        return;
      }
      try {
        this.loading = true;
        await api.put(`/Order/${orderId}/cancel`);
       
        // Update the order status in the UI
        const orderIndex = this.orders.findIndex(order => order.id === orderId);
        if (orderIndex !== -1) {
          this.orders[orderIndex].status = 4; // Set to cancelled status
        }
        
        this.$toast.success('Order cancelled successfully');
      } catch (error) {
        console.error('Failed to cancel order:', error);
        this.$toast.error(error.response?.data?.message || 'Failed to cancel order. Please try again.');
      } finally {
        this.loading = false;
      }
    },
    async rateProduct(productId) {
      try {
        // Navigate to product details page with rating modal parameter
        this.$router.push({
          path: `/products/${productId}`,
          query: { openRatingModal: true }
        });
      } catch (error) {
        console.error('Navigation error:', error);
        this.$toast.error('Could not open rating page. Please try again.');
      }
    }
  }
};
</script>