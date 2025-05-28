<template>
  <div class="min-h-screen bg-gray-50 flex flex-col justify-center py-12 sm:px-6 lg:px-8">
    <div class="sm:mx-auto sm:w-full sm:max-w-md">
      <div class="bg-white py-8 px-4 shadow sm:rounded-lg sm:px-10">
        <div class="text-center">
          <!-- Success Icon -->
          <div class="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-green-100 mb-4">
            <svg class="h-6 w-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
            </svg>
          </div>
          
          <h2 class="text-2xl font-bold text-gray-900 mb-2">Payment Successful!</h2>
          <p class="text-gray-600 mb-6">Your payment has been processed successfully.</p>
          
          <div class="bg-gray-50 rounded-lg p-4 mb-6">
            <div class="text-sm text-gray-600 space-y-2">
              <div v-if="orderId">
                <span class="font-medium">Order ID:</span> {{ orderId }}
              </div>
              <div v-if="transactionId">
                <span class="font-medium">Transaction ID:</span> {{ transactionId }}
              </div>
              <div>
                <span class="font-medium">Payment Time:</span> {{ new Date().toLocaleString() }}
              </div>
            </div>
          </div>
          
          <div class="space-y-3">
            <router-link 
              to="/orders" 
              class="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500">
              View My Orders
            </router-link>
            
            <router-link 
              to="/" 
              class="w-full flex justify-center py-2 px-4 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500">
              Continue Shopping
            </router-link>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'PaymentSuccess',
  data() {
    return {
      orderId: null,
      transactionId: null
    }
  },
  created() {
    // Get parameters from URL
    this.orderId = this.$route.query.orderId;
    this.transactionId = this.$route.query.transId;
    
    // Show success notification
    this.$store.dispatch('notification/showNotification', {
      type: 'success',
      message: 'Payment completed successfully!'
    });
  }
}
</script> 