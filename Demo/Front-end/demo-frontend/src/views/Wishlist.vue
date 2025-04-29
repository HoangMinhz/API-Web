<template>
  <div class="container mx-auto px-4 py-8">
    <h1 class="text-3xl font-bold mb-8 mt-16">My Wishlist</h1>
    
    <div v-if="$store.state.wishlist.loading" class="text-center">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-gray-900 mx-auto"></div>
      <p class="mt-4">Loading wishlist...</p>
    </div>

    <div v-else-if="$store.state.wishlist.error" class="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded relative" role="alert">
      <strong class="font-bold">Error!</strong>
      <span class="block sm:inline"> {{ $store.state.wishlist.error }}</span>
    </div>

    <div v-else-if="$store.getters['wishlist/items'].length === 0" class="flex flex-col items-center justify-center min-h-[60vh]">
      <div class="text-center">
        <svg class="w-16 h-16 text-gray-400 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
        </svg>
        <h2 class="text-2xl font-semibold mb-4">Your wishlist is empty</h2>
        <p class="text-gray-600 mb-8">Looks like you haven't added any items to your wishlist yet.</p>
        <router-link to="/shop" class="inline-block bg-blue-500 hover:bg-blue-600 text-white font-bold py-2 px-4 rounded transition-colors duration-200">
          Browse Products
        </router-link>
      </div>
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
      <div v-for="item in $store.getters['wishlist/items']" :key="item.id" class="bg-white rounded-lg shadow-md overflow-hidden">
        <div class="relative">
          <img
            :src="getProductImage(item)"
            :alt="item.name"
            class="w-full h-48 object-cover"
          />
          <button
            @click="removeFromWishlist(item.id)"
            class="absolute top-2 right-2 p-2 bg-white bg-opacity-80 rounded-full hover:bg-opacity-100 transition-all duration-200"
          >
            <svg class="w-5 h-5 text-red-500" fill="currentColor" viewBox="0 0 20 20">
              <path fill-rule="evenodd" d="M3.172 5.172a4 4 0 015.656 0L10 6.343l1.172-1.171a4 4 0 115.656 5.656L10 17.657l-6.828-6.829a4 4 0 010-5.656z" clip-rule="evenodd" />
            </svg>
          </button>
        </div>
        <div class="p-4">
          <h3 class="text-lg font-semibold text-gray-900 mb-2">{{ item.name }}</h3>
          <p class="text-gray-600 text-sm mb-4 line-clamp-2">{{ item.description }}</p>
          <div class="flex items-center justify-between">
            <p class="text-lg font-bold text-gray-900">{{ formatPrice(item.price) }}</p>
            <button
              @click="addToCart(item)"
              class="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition-colors"
            >
              Add to Cart
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { onMounted } from 'vue';
import { useStore } from 'vuex';
import formatPrice from '../utils/formatPrice';

export default {
  name: 'WishlistPage',
  setup() {
    const store = useStore();

    const getProductImage = (product) => {
      if (!product?.imageUrl) {
        return 'https://via.placeholder.com/300x300?text=No+Image';
      }
      if (!product.imageUrl.startsWith('http')) {
        return `http://localhost:5285${product.imageUrl}`;
      }
      return product.imageUrl;
    };

    const removeFromWishlist = async (productId) => {
      await store.dispatch('wishlist/removeFromWishlist', productId);
    };

    const addToCart = async (product) => {
      try {
        await store.dispatch('cart/addToCart', {
          productId: product.id,
          quantity: 1
        });
        store.dispatch('notification/showNotification', {
          type: 'success',
          message: 'Added to cart!'
        }, { root: true });
      } catch (error) {
        console.error('Error adding to cart:', error);
        store.dispatch('notification/showNotification', {
          type: 'error',
          message: 'Failed to add to cart'
        }, { root: true });
      }
    };

    onMounted(async () => {
      await store.dispatch('wishlist/fetchWishlist');
    });

    return {
      getProductImage,
      removeFromWishlist,
      addToCart,
      formatPrice
    };
  }
};
</script> 