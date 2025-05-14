<template>
  <div class="min-h-screen bg-gray-50 py-12">
    <div class="container mx-auto px-4 max-w-3xl">
      <div class="bg-white rounded-xl shadow-sm p-8">
        <div class="text-center mb-8">
          <div class="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <svg class="w-8 h-8 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
            </svg>
          </div>
          <h1 class="text-2xl font-semibold text-gray-900 mb-2">Order Confirmed!</h1>
          <p class="text-gray-600">Thank you for your purchase</p>
        </div>

        <div class="border-t border-gray-200 pt-6">
          <div class="space-y-4">
            <div class="flex justify-between">
              <span class="text-gray-600">Order Number</span>
              <span class="font-medium text-gray-900">{{ orderNumber }}</span>
            </div>
            <div class="flex justify-between">
              <span class="text-gray-600">Total Amount</span>
              <span class="font-medium text-gray-900">{{ formatPrice(total) }}</span>
            </div>
            <div class="flex justify-between">
              <span class="text-gray-600">Order Date</span>
              <span class="font-medium text-gray-900">{{ formatDate(orderDate) }}</span>
            </div>
          </div>
        </div>

        <div class="mt-8 space-y-4">
          <router-link
            to="/orders"
            class="block w-full text-center bg-blue-600 text-white py-3 px-4 rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition"
          >
            View Order Details
          </router-link>
          <router-link
            to="/shop"
            class="block w-full text-center bg-gray-100 text-gray-700 py-3 px-4 rounded-lg hover:bg-gray-200 focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 transition"
          >
            Continue Shopping
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import formatPrice from '../utils/formatPrice';
import api from '../services/api';

export default {
  name: 'OrderConfirmation',
  data() {
    return {
      orderNumber: this.$route.query.orderNumber || '',
      total: Number(this.$route.query.total) || 0,
      orderDate: new Date()
    };
  },
  created() {
    // Fetch order details if we have an order ID
    if (this.$route.params.id) {
      this.fetchOrderDetails();
    }
  },
  methods: {
    formatPrice,
    formatDate(date) {
      return new Date(date).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      });
    },
    async fetchOrderDetails() {
      try {
        const response = await api.get(`/Order/${this.$route.params.id}`);
        if (response.data) {
          this.total = response.data.totalAmount;
          this.orderDate = response.data.orderDate;
        }
      } catch (error) {
        console.error('Failed to fetch order details:', error);
      }
    }
  }
};
</script> 