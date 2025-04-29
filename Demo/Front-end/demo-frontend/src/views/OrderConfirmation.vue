<template>
  <div class="min-h-screen bg-gray-50 py-12">
    <div class="container mx-auto px-4">
      <div class="max-w-2xl mx-auto bg-white rounded-lg shadow-md p-8">
        <div class="text-center mb-8">
          <div class="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <svg class="w-8 h-8 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
            </svg>
          </div>
          <h1 class="text-2xl font-bold text-gray-900 mb-2">Order Confirmed!</h1>
          <p class="text-gray-600">Thank you for your purchase. Your order has been received.</p>
        </div>

        <div class="border-t border-gray-200 pt-6">
          <h2 class="text-lg font-semibold text-gray-900 mb-4">Order Details</h2>
          
          <div class="space-y-4">
            <div class="flex justify-between">
              <span class="text-gray-600">Order Number</span>
              <span class="text-gray-900 font-medium">{{ orderNumber }}</span>
            </div>
            <div class="flex justify-between">
              <span class="text-gray-600">Date</span>
              <span class="text-gray-900">{{ orderDate }}</span>
            </div>
            <div class="flex justify-between">
              <span class="text-gray-600">Total Amount</span>
              <span class="text-gray-900 font-medium">{{ formatPrice(orderTotal) }}</span>
            </div>
            <div class="flex justify-between">
              <span class="text-gray-600">Payment Method</span>
              <span class="text-gray-900">{{ paymentMethod }}</span>
            </div>
          </div>
        </div>

        <div class="border-t border-gray-200 pt-6 mt-6">
          <h2 class="text-lg font-semibold text-gray-900 mb-4">Shipping Information</h2>
          
          <div class="space-y-2">
            <p class="text-gray-900">{{ shippingInfo.fullName }}</p>
            <p class="text-gray-600">{{ shippingInfo.address }}</p>
            <p class="text-gray-600">{{ shippingInfo.city }}, {{ shippingInfo.state }} {{ shippingInfo.postalCode }}</p>
            <p class="text-gray-600">{{ shippingInfo.email }}</p>
            <p class="text-gray-600">{{ shippingInfo.phone }}</p>
          </div>
        </div>

        <div class="mt-8 flex flex-col sm:flex-row gap-4">
          <router-link
            to="/shop"
            class="flex-1 bg-gray-800 text-white py-3 px-4 rounded-md text-center hover:bg-gray-900 transition"
          >
            Continue Shopping
          </router-link>
          <router-link
            to="/orders"
            class="flex-1 bg-blue-600 text-white py-3 px-4 rounded-md text-center hover:bg-blue-700 transition"
          >
            View Orders
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import formatPrice from '../utils/formatPrice';

export default {
  name: 'OrderConfirmationPage',
  data() {
    return {
      orderNumber: '#' + Math.floor(Math.random() * 1000000).toString().padStart(6, '0'),
      orderDate: new Date().toLocaleDateString(),
      orderTotal: 0,
      paymentMethod: 'Cash on Delivery',
      shippingInfo: {
        fullName: '',
        email: '',
        phone: '',
        address: '',
        city: '',
        state: '',
        postalCode: ''
      }
    };
  },
  methods: {
    formatPrice
  },
  created() {
    // In a real application, you would fetch this data from your backend
    // For now, we'll use the data from the route query
    const orderData = this.$route.query.orderData;
    if (orderData) {
      try {
        const data = JSON.parse(decodeURIComponent(orderData));
        this.orderTotal = data.total;
        this.paymentMethod = data.paymentMethod === 'cod' ? 'Cash on Delivery' : 'Bank Transfer';
        this.shippingInfo = {
          fullName: data.fullName,
          email: data.email,
          phone: data.phone,
          address: data.address,
          city: data.city,
          state: data.state,
          postalCode: data.postalCode
        };
      } catch (error) {
        console.error('Error parsing order data:', error);
      }
    }
  }
};
</script> 