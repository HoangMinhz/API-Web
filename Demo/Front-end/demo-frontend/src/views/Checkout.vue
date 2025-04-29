<template>
  <div class="min-h-screen bg-gray-50 py-12">
    <div class="container mx-auto px-4 max-w-6xl">
      <!-- Progress Steps -->
      <div class="mb-12">
        <div class="flex items-center justify-between">
          <div class="flex items-center">
            <div class="w-8 h-8 rounded-full bg-blue-600 text-white flex items-center justify-center">
              1
            </div>
            <div class="ml-2 text-sm font-medium text-blue-600">Shipping</div>
          </div>
          <div class="flex-1 h-1 mx-4 bg-blue-600"></div>
          <div class="flex items-center">
            <div class="w-8 h-8 rounded-full bg-gray-200 text-gray-600 flex items-center justify-center">
              2
            </div>
            <div class="ml-2 text-sm font-medium text-gray-600">Payment</div>
          </div>
          <div class="flex-1 h-1 mx-4 bg-gray-200"></div>
          <div class="flex items-center">
            <div class="w-8 h-8 rounded-full bg-gray-200 text-gray-600 flex items-center justify-center">
              3
            </div>
            <div class="ml-2 text-sm font-medium text-gray-600">Review</div>
          </div>
        </div>
      </div>

      <div v-if="cartError" class="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-lg mb-6">
        {{ cartError }}
      </div>

      <div v-else-if="cartLoading" class="flex justify-center items-center py-12">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
      </div>

      <div v-else-if="!cartItems.length" class="text-center py-12">
        <p class="text-gray-600 text-lg">Your cart is empty</p>
        <router-link to="/shop" class="text-blue-600 hover:text-blue-800 mt-4 inline-block">
          Continue Shopping
        </router-link>
      </div>

      <div v-else class="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <!-- Order Summary -->
        <div class="lg:col-span-1">
          <div class="bg-white rounded-xl shadow-sm p-6 sticky top-6">
            <h2 class="text-xl font-semibold text-gray-900 mb-6">Order Summary</h2>
            
            <div class="space-y-4 mb-6">
              <div v-for="item in cartItems" :key="item.productId" class="flex items-center space-x-4">
                <img
                  :src="item.product?.imageUrl || 'https://via.placeholder.com/100'"
                  :alt="item.product?.name"
                  class="w-16 h-16 object-cover rounded-lg"
                />
                <div class="flex-1">
                  <h3 class="text-sm font-medium text-gray-900">{{ item.product?.name }}</h3>
                  <div class="flex items-center justify-between mt-1">
                    <p class="text-sm text-gray-500">Quantity: {{ item.quantity }}</p>
                    <p class="text-sm text-gray-500">{{ formatPrice(item.product?.price) }} / unit</p>
                  </div>
                  <p class="text-sm font-medium text-gray-900 mt-1">Total: {{ formatPrice(item.product?.price * item.quantity) }}</p>
                </div>
              </div>
            </div>

            <div class="space-y-3 border-t border-gray-200 pt-6">
              <div class="flex justify-between">
                <span class="text-gray-600">Subtotal</span>
                <span class="text-gray-900">{{ formatPrice(subtotal) }}</span>
              </div>
              <div class="flex justify-between">
                <span class="text-gray-600">Tax (10%)</span>
                <span class="text-gray-900">{{ formatPrice(tax) }}</span>
              </div>
              <div class="flex justify-between text-lg font-semibold pt-3">
                <span class="text-gray-900">Total</span>
                <span class="text-blue-600">{{ formatPrice(total) }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Shipping Information -->
        <div class="lg:col-span-2">
          <div class="bg-white rounded-xl shadow-sm p-8">
            <h2 class="text-2xl font-semibold text-gray-900 mb-6">Shipping Information</h2>
            <form @submit.prevent="submitOrder" class="space-y-6">
              <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Full Name</label>
                  <input
                    v-model="shippingInfo.fullName"
                    type="text"
                    required
                    class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition"
                    placeholder="Enter your full name"
                  />
                </div>

                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Email</label>
                  <input
                    v-model="shippingInfo.email"
                    type="email"
                    required
                    class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition"
                    placeholder="Enter your email"
                  />
                </div>

                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Phone Number</label>
                  <input
                    v-model="shippingInfo.phone"
                    type="tel"
                    required
                    class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition"
                    placeholder="Enter your phone number"
                  />
                </div>

                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">City</label>
                  <input
                    v-model="shippingInfo.city"
                    type="text"
                    required
                    class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition"
                    placeholder="Enter your city"
                  />
                </div>
              </div>

              <div>
                <label class="block text-sm font-medium text-gray-700 mb-2">Address</label>
                <textarea
                  v-model="shippingInfo.address"
                  required
                  rows="3"
                  class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition"
                  placeholder="Enter your full address"
                ></textarea>
              </div>

              <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">State/Province</label>
                  <input
                    v-model="shippingInfo.state"
                    type="text"
                    required
                    class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition"
                    placeholder="Enter your state/province"
                  />
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Postal Code</label>
                  <input
                    v-model="shippingInfo.postalCode"
                    type="text"
                    required
                    class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition"
                    placeholder="Enter your postal code"
                  />
                </div>
              </div>

              <div>
                <label class="block text-sm font-medium text-gray-700 mb-2">Payment Method</label>
                <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <label class="flex items-center p-4 border border-gray-300 rounded-lg cursor-pointer hover:border-blue-500 transition">
                    <input
                      v-model="shippingInfo.paymentMethod"
                      type="radio"
                      value="cod"
                      class="h-4 w-4 text-blue-600 focus:ring-blue-500"
                    />
                    <span class="ml-3">
                      <span class="block text-sm font-medium text-gray-900">Cash on Delivery</span>
                      <span class="block text-xs text-gray-500">Pay when you receive the order</span>
                    </span>
                  </label>
                  <label class="flex items-center p-4 border border-gray-300 rounded-lg cursor-pointer hover:border-blue-500 transition">
                    <input
                      v-model="shippingInfo.paymentMethod"
                      type="radio"
                      value="bank"
                      class="h-4 w-4 text-blue-600 focus:ring-blue-500"
                    />
                    <span class="ml-3">
                      <span class="block text-sm font-medium text-gray-900">Bank Transfer</span>
                      <span class="block text-xs text-gray-500">Pay via bank transfer</span>
                    </span>
                  </label>
                </div>
              </div>

              <button
                type="submit"
                :disabled="isSubmitting"
                class="w-full bg-blue-600 text-white py-4 px-6 rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed transition"
              >
                {{ isSubmitting ? 'Processing...' : 'Place Order' }}
              </button>
            </form>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { mapState, mapActions } from 'vuex';
import formatPrice from '../utils/formatPrice';

export default {
  name: 'CheckoutPage',
  data() {
    return {
      shippingInfo: {
        fullName: '',
        email: '',
        phone: '',
        address: '',
        city: '',
        state: '',
        postalCode: '',
        paymentMethod: 'cod'
      },
      isSubmitting: false
    };
  },
  computed: {
    ...mapState('cart', ['items', 'loading', 'error']),
    cartItems() {
      return this.items;
    },
    cartLoading() {
      return this.loading;
    },
    cartError() {
      return this.error;
    },
    subtotal() {
      return this.cartItems.reduce((total, item) => {
        return total + (item.product?.price || 0) * item.quantity;
      }, 0);
    },
    tax() {
      return this.subtotal * 0.1; // 10% tax
    },
    total() {
      return this.subtotal + this.tax;
    }
  },
  methods: {
    ...mapActions('cart', ['checkout']),
    formatPrice,
    async submitOrder() {
      if (this.isSubmitting) return;
      
      this.isSubmitting = true;
      try {
        const orderData = {
          ...this.shippingInfo,
          items: this.cartItems.map(item => ({
            productId: item.productId,
            quantity: item.quantity,
            price: item.product.price
          })),
          subtotal: this.subtotal,
          tax: this.tax,
          total: this.total
        };

        await this.checkout(orderData);
        
        // Show success message and redirect
        this.$store.dispatch('notification/showNotification', {
          type: 'success',
          message: 'Order placed successfully!'
        }, { root: true });
        
        this.$router.push('/order-confirmation');
      } catch (error) {
        console.error('Checkout error:', error);
        this.$store.dispatch('notification/showNotification', {
          type: 'error',
          message: 'Failed to place order. Please try again.'
        }, { root: true });
      } finally {
        this.isSubmitting = false;
      }
    }
  },
  created() {
    this.$store.dispatch('cart/fetchCart');
  }
};
</script> 