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
                <div class="flex items-center">
                  <span
                    :class="{
                      'px-3 py-1 rounded-full text-sm font-medium': true,
                      'bg-yellow-100 text-yellow-800': order.status === 'Pending',
                      'bg-blue-100 text-blue-800': order.status === 'Processing',
                      'bg-green-100 text-green-800': order.status === 'Shipped',
                      'bg-purple-100 text-purple-800': order.status === 'Delivered',
                      'bg-red-100 text-red-800': order.status === 'Cancelled'
                    }"
                  >
                    {{ order.status }}
                  </span>
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
                  <p class="text-sm font-medium text-gray-900">{{ formatPrice(item.totalPrice) }}</p>
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
                </div>
              </div>
            </div>

            <!-- Order Details (Expanded) -->
            <div v-if="expandedOrderId === order.id" class="px-6 py-4 bg-gray-50 border-t border-gray-200">
              <div class="space-y-4">
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
    }
  }
};
</script> 