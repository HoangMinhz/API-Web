<template>
  <div class="container mx-auto px-4 py-8">
    <h1 class="text-3xl font-bold mb-8 mt-16">Shopping Cart</h1>
    
    <div v-if="$store.state.cart.loading" class="text-center">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-gray-900 mx-auto"></div>
      <p class="mt-4">Loading cart...</p>
    </div>

    <div v-else-if="$store.state.cart.error" class="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded relative" role="alert">
      <strong class="font-bold">Error!</strong>
      <span class="block sm:inline"> {{ $store.state.cart.error }}</span>
    </div>

    <div v-else-if="$store.getters['cart/items'].length === 0" class="flex flex-col items-center justify-center min-h-[60vh]">
      <div class="text-center">
        <svg class="w-16 h-16 text-gray-400 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z" />
        </svg>
        <h2 class="text-2xl font-semibold mb-4">Your cart is empty</h2>
        <p class="text-gray-600 mb-8">Looks like you haven't added any items to your cart yet.</p>
        <router-link to="/shop" class="inline-block bg-blue-500 hover:bg-blue-600 text-white font-bold py-2 px-4 rounded transition-colors duration-200">
          Continue Shopping
        </router-link>
      </div>
    </div>

    <div v-else class="flex flex-col md:flex-row gap-8">
      <!-- Cart Items -->
      <div class="md:w-2/3">
        <div class="bg-white rounded-lg shadow-md p-6">
          <div v-for="item in $store.getters['cart/items']" :key="item.id" class="flex items-center gap-4 py-4 border-b">
            <img :src="item.product?.imageUrl" :alt="item.product?.name" class="w-24 h-24 object-cover rounded">
            <div class="flex-1">
              <h3 class="font-semibold">{{ item.product?.name }}</h3>
              <p class="text-gray-600">{{ formatPrice(item.product?.price) }}</p>
              <div class="flex items-center mt-2">
                <button @click="updateQuantity(item, item.quantity - 1)" 
                        class="px-2 py-1 border rounded-l hover:bg-gray-100"
                        :disabled="item.quantity <= 1">
                  -
                </button>
                <span class="px-4 py-1 border-t border-b">{{ item.quantity }}</span>
                <button @click="updateQuantity(item, item.quantity + 1)" 
                        class="px-2 py-1 border rounded-r hover:bg-gray-100">
                  +
                </button>
              </div>
            </div>
            <div class="text-right">
              <p class="font-semibold">{{ formatPrice(item.product?.price * item.quantity) }}</p>
              <button @click="removeFromCart(item)" class="text-red-500 hover:text-red-700 mt-2">
                Remove
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Order Summary -->
      <div class="md:w-1/3">
        <div class="bg-white rounded-lg shadow-md p-6">
          <h2 class="text-xl font-semibold mb-4">Order Summary</h2>
          <div class="space-y-2">
            <div class="flex justify-between">
              <span>Subtotal</span>
              <span>{{ formatPrice($store.getters['cart/subtotal']) }}</span>
            </div>
            <div class="flex justify-between">
              <span>Tax (10%)</span>
              <span>{{ formatPrice($store.getters['cart/tax']) }}</span>
            </div>
            <div class="border-t pt-2 mt-2">
              <div class="flex justify-between font-semibold">
                <span>Total</span>
                <span>{{ formatPrice($store.getters['cart/total']) }}</span>
              </div>
            </div>
          </div>
          <button 
            @click="handleCheckout" 
            class="w-full bg-blue-500 hover:bg-blue-600 text-white font-bold py-2 px-4 rounded mt-6"
            :disabled="!$store.getters['user/isAuthenticated']">
            {{ $store.getters['user/isAuthenticated'] ? 'Proceed to Checkout' : 'Login to Checkout' }}
          </button>
          <div v-if="!$store.getters['user/isAuthenticated']" class="mt-4 text-center">
            <p class="text-sm text-gray-600">Please login to proceed with checkout</p>
            <router-link to="/login" class="text-blue-500 hover:text-blue-700">Login</router-link>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { useStore } from 'vuex';
import { onMounted } from 'vue';
import { useRouter } from 'vue-router';
import formatPrice from '../utils/formatPrice';

export default {
  name: 'CartPage',
  setup() {
    const store = useStore();
    const router = useRouter();

    const updateQuantity = async (item, newQuantity) => {
      if (newQuantity < 1) return;
      await store.dispatch('cart/updateCartItem', { productId: item.productId, quantity: newQuantity });
    };

    const removeFromCart = async (item) => {
      await store.dispatch('cart/removeFromCart', item.productId);
    };

    const handleCheckout = () => {
      if (!store.getters['user/isAuthenticated']) {
        router.push('/login');
      } else {
        // Handle checkout for authenticated users
        router.push('/checkout');
      }
    };

    onMounted(async () => {
      await store.dispatch('cart/fetchCart');
    });

    return {
      updateQuantity,
      removeFromCart,
      handleCheckout,
      formatPrice
    };
  }
};
</script>