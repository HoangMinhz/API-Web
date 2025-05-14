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
                    :class="{ 'border-red-500': validationErrors.fullName }"
                    placeholder="Enter your full name"
                  />
                  <p v-if="validationErrors.fullName" class="mt-1 text-sm text-red-600">
                    {{ validationErrors.fullName }}
                  </p>
                </div>

                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Email</label>
                  <input
                    v-model="shippingInfo.email"
                    type="email"
                    required
                    class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition"
                    :class="{ 'border-red-500': validationErrors.email }"
                    placeholder="Enter your email"
                  />
                  <p v-if="validationErrors.email" class="mt-1 text-sm text-red-600">
                    {{ validationErrors.email }}
                  </p>
                </div>

                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Phone Number</label>
                  <input
                    v-model="shippingInfo.phone"
                    type="tel"
                    required
                    class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition"
                    :class="{ 'border-red-500': validationErrors.phone }"
                    placeholder="Enter your phone number"
                  />
                  <p v-if="validationErrors.phone" class="mt-1 text-sm text-red-600">
                    {{ validationErrors.phone }}
                  </p>
                </div>

                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Address</label>
                  <input
                    v-model="shippingInfo.address"
                    type="text"
                    required
                    class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition"
                    :class="{ 'border-red-500': validationErrors.address }"
                    placeholder="Enter your address"
                  />
                  <p v-if="validationErrors.address" class="mt-1 text-sm text-red-600">
                    {{ validationErrors.address }}
                  </p>
                </div>

                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Province/City</label>
                  <select
                    v-model="shippingInfo.province"
                    required
                    class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition"
                    :class="{ 'border-red-500': validationErrors.province }"
                  >
                    <option value="">Select province/city</option>
                    <option v-for="province in provinces" :key="province.code" :value="province.code">
                      {{ province.name }}
                    </option>
                  </select>
                  <p v-if="validationErrors.province" class="mt-1 text-sm text-red-600">
                    {{ validationErrors.province }}
                  </p>
                </div>

                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">District</label>
                  <select
                    v-model="shippingInfo.district"
                    :required="districts.length > 0"
                    class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition"
                    :class="{ 'border-red-500': validationErrors.district }"
                    :disabled="!shippingInfo.province || districts.length === 0"
                  >
                    <option value="">Select district</option>
                    <option v-for="district in districts" :key="district.code" :value="district.code">
                      {{ district.name }}
                    </option>
                  </select>
                  <p v-if="validationErrors.district" class="mt-1 text-sm text-red-600">
                    {{ validationErrors.district }}
                  </p>
                  <p v-if="districts.length === 0" class="mt-1 text-sm text-gray-500">
                    No districts available for this province
                  </p>
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
                :disabled="isSubmitting || !isShippingValid"
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
import api from '../services/api';
import { provinces, districts } from '../data/vietnam-locations';

export default {
  name: 'CheckoutPage',
  data() {
    return {
      shippingInfo: {
        fullName: '',
        email: '',
        phone: '',
        address: '',
        province: '',
        district: '',
        paymentMethod: 'cod'
      },
      isSubmitting: false,
      userInfo: null,
      provinces,
      districts: [],
      validationErrors: {},
      currentStep: 1
    };
  },
  watch: {
    'shippingInfo.province': {
      handler(newProvince) {
        if (newProvince) {
          this.districts = districts[newProvince] || [];
          if (this.districts.length === 0) {
            this.shippingInfo.district = '';
          }
        } else {
          this.districts = [];
          this.shippingInfo.district = '';
        }
      },
      immediate: true
    }
  },
  computed: {
    ...mapState('cart', ['items', 'loading', 'error']),
    ...mapState('auth', ['user']),
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
      return this.$store.getters['cart/subtotal'];
    },
    tax() {
      return this.$store.getters['cart/tax'];
    },
    total() {
      return this.$store.getters['cart/total'];
    },
    isShippingValid() {
      return (
        this.shippingInfo.fullName &&
        this.shippingInfo.email &&
        this.shippingInfo.phone &&
        this.shippingInfo.address &&
        this.shippingInfo.province &&
        (this.districts.length === 0 || this.shippingInfo.district)
      );
    }
  },
  async created() {
    await this.fetchUserInfo();
  },
  methods: {
    ...mapActions('cart', ['clearCart']),
    ...mapActions('notification', ['showNotification']),
    formatPrice,
    validatePhoneNumber(phone) {
      const phoneRegex = /^[0-9]{10,11}$/;
      return phoneRegex.test(phone);
    },
    validateEmail(email) {
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      return emailRegex.test(email);
    },
    validateShippingInfo() {
      this.validationErrors = {};

      if (!this.shippingInfo.fullName?.trim()) {
        this.validationErrors.fullName = 'Full name is required';
      }

      if (!this.shippingInfo.email?.trim()) {
        this.validationErrors.email = 'Email is required';
      } else if (!this.validateEmail(this.shippingInfo.email)) {
        this.validationErrors.email = 'Invalid email format';
      }

      if (!this.shippingInfo.phone?.trim()) {
        this.validationErrors.phone = 'Phone number is required';
      } else if (!this.validatePhoneNumber(this.shippingInfo.phone)) {
        this.validationErrors.phone = 'Phone number must be 10-11 digits';
      }

      if (!this.shippingInfo.address?.trim()) {
        this.validationErrors.address = 'Address is required';
      }

      if (!this.shippingInfo.province) {
        this.validationErrors.province = 'Province is required';
      }

      if (this.districts.length > 0 && !this.shippingInfo.district) {
        this.validationErrors.district = 'District is required';
      }

      return Object.keys(this.validationErrors).length === 0;
    },
    async fetchUserInfo() {
      try {
        const response = await api.get('/User/current');
        this.userInfo = response.data;
        // Auto-fill shipping information
        this.shippingInfo = {
          ...this.shippingInfo,
          fullName: this.userInfo.fullName || '',
          email: this.userInfo.email || '',
          phone: this.userInfo.phoneNumber || '',
          address: this.userInfo.address || '',
          province: this.userInfo.province || '',
          district: this.userInfo.district || ''
        };
      } catch (error) {
        console.error('Failed to fetch user info:', error);
        this.showNotification({
          type: 'error',
          message: 'Failed to load user information'
        });
      }
    },
    async submitOrder() {
      if (this.isSubmitting) return;

      // Validate shipping information
      if (!this.validateShippingInfo()) {
        this.showNotification({
          type: 'error',
          message: 'Please fill in all required fields correctly'
        });
        return;
      }

      this.isSubmitting = true;
      try {
        const selectedProvince = this.provinces.find(p => p.code === this.shippingInfo.province);
        const selectedDistrict = this.districts.find(d => d.code === this.shippingInfo.district);
        
        // Build address string based on available data
        let addressParts = [this.shippingInfo.address];
        if (selectedDistrict?.name) {
          addressParts.push(selectedDistrict.name);
        }
        if (selectedProvince?.name) {
          addressParts.push(selectedProvince.name);
        }
        
        const orderData = {
          shippingAddress: addressParts.join(', '),
          phoneNumber: this.shippingInfo.phone,
          fullName: this.shippingInfo.fullName,
          notes: `Payment Method: ${this.shippingInfo.paymentMethod}`,
          items: this.cartItems.map(item => ({
            productId: item.productId,
            quantity: item.quantity
          }))
        };

        const response = await api.post('/Order', orderData);
        
        // Show success notification
        this.showNotification({
          type: 'success',
          message: 'Order placed successfully!'
        });

        // Clear cart and redirect
        await this.clearCart();
        
        // Redirect to order confirmation page
        this.$router.push({ 
          name: 'order-confirmation', 
          params: { id: response.data.id },
          query: { 
            orderNumber: response.data.orderNumber,
            total: response.data.totalAmount || this.total
          }
        });
      } catch (error) {
        console.error('Failed to submit order:', error);
        const errorMessage = error.response?.data?.message || 'Failed to place order. Please try again.';
        this.showNotification({
          type: 'error',
          message: errorMessage
        });
      } finally {
        this.isSubmitting = false;
      }
    }
  }
};
</script> 