<template>
  <div class="flex-shrink-0 w-72 relative bg-white rounded-xl shadow-lg hover:shadow-2xl transition-all duration-300 transform hover:-translate-y-2 h-[500px] flex flex-col">
    <router-link :to="`/products/${product.id}`" class="flex-1 flex flex-col">
      <div class="relative overflow-hidden rounded-t-xl flex-1">
        <img
          :src="getProductImage(product)"
          :alt="product.name"
          class="w-full h-full object-cover transition-transform duration-300 transform hover:scale-110"
          loading="lazy"
        />
        <div class="absolute inset-0 bg-gradient-to-t from-black/20 to-transparent"></div>
      </div>
      <div class="p-4 flex flex-col flex-1">
        <h3 class="text-lg font-semibold text-gray-800 mb-1 truncate">{{ product.name }}</h3>
        <p class="text-sm text-gray-600 line-clamp-2 mb-2">{{ product.description }}</p>
        
        <div class="flex items-center mb-2">
          <div class="flex items-center">
            <svg v-for="star in 5" :key="star" 
              class="w-4 h-4" 
              :class="star <= (product.rating || 4) ? 'text-yellow-400' : 'text-gray-300'"
              fill="currentColor" 
              viewBox="0 0 20 20"
            >
              <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
            </svg>
          </div>
          <span class="text-sm text-gray-500 ml-1">({{ product.rating || 4 }})</span>
          <span class="text-sm text-gray-500 ml-2">|</span>
          <span class="text-sm text-gray-500 ml-2">{{ product.soldCount || 0 }} sold</span>
        </div>

        <div class="flex justify-between items-center mt-auto">
          <p class="text-lg font-bold text-gray-800">{{ formatPrice(product.price) }}</p>
          <p class="text-xs text-gray-500">{{ getCategoryName(product.categoryId) }}</p>
        </div>
      </div>
    </router-link>
    <div class="px-4 pb-4 flex space-x-2">
      <button
        @click="openQuickView"
        class="flex-1 bg-gray-200 text-gray-800 py-2 rounded-md hover:bg-gray-300 transition"
      >
        Quick View
      </button>
      <button
        @click="addToCart"
        class="flex-1 bg-gray-800 text-white py-2 rounded-md hover:bg-gray-900 transition"
      >
        Add to Cart
      </button>
    </div>

    <!-- Quick View Modal -->
        <div v-if="showQuickView" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white rounded-xl p-6 max-w-lg w-full relative">
        <button @click="closeQuickView" class="absolute top-4 right-4 text-gray-600 hover:text-gray-800">
          <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
        <div class="flex flex-col md:flex-row gap-6">
          <img
            :src="selectedProduct.imageUrl || 'https://via.placeholder.com/150'"
            :alt="selectedProduct.name"
            class="w-full md:w-1/2 h-48 object-cover rounded-lg"
          />
          <div class="flex-1">
            <h3 class="text-2xl font-bold text-gray-800 mb-2">{{ selectedProduct.name }}</h3>
            <p class="text-sm text-gray-600 mb-4">{{ selectedProduct.description }}</p>
            <p class="text-lg font-bold text-gray-800 mb-4">{{ formatPrice(selectedProduct.price) }}</p>
            <button
              @click="addToCart(selectedProduct); closeQuickView()"
              class="w-full bg-gray-800 text-white py-2 rounded-md hover:bg-gray-900 transition"
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
import { ref, computed } from 'vue';
import { useStore } from 'vuex';
import formatPrice from '../utils/formatPrice';

export default {
  name: 'ProductCard',
  props: {
    product: {
      type: Object,
      required: true
    },
    categories: {
      type: Array,
      default: () => []
    }
  },
  setup(props) {
    const store = useStore();
    const showQuickView = ref(false);
    const quantity = ref(1);

    const isInWishlist = computed(() => {
      return store.getters['wishlist/isInWishlist'](props.product.id);
    });

    const getProductImage = (product) => {
      if (!product?.imageUrl) {
        return 'https://via.placeholder.com/300x300?text=No+Image';
      }
      if (!product.imageUrl.startsWith('http')) {
        return `http://localhost:5285${product.imageUrl}`;
      }
      return product.imageUrl;
    };

    const getCategoryName = (categoryId) => {
      const category = props.categories.find(c => c.id === categoryId);
      return category ? category.name : 'Unknown Category';
    };

    const openQuickView = () => {
      showQuickView.value = true;
    };

    const closeQuickView = () => {
      showQuickView.value = false;
    };

    const increaseQuantity = () => {
      if (quantity.value < props.product.stock) {
        quantity.value++;
      }
    };

    const decreaseQuantity = () => {
      if (quantity.value > 1) {
        quantity.value--;
      }
    };

    const addToCart = async () => {
      try {
        await store.dispatch('cart/addToCart', {
          productId: props.product.id,
          quantity: quantity.value
        });
        closeQuickView();
      } catch (error) {
        console.error('Error adding to cart:', error);
      }
    };

    const toggleWishlist = async () => {
      if (isInWishlist.value) {
        await store.dispatch('wishlist/removeFromWishlist', props.product.id);
      } else {
        await store.dispatch('wishlist/addToWishlist', props.product.id);
      }
    };

    return {
      showQuickView,
      quantity,
      isInWishlist,
      getProductImage,
      getCategoryName,
      openQuickView,
      closeQuickView,
      increaseQuantity,
      decreaseQuantity,
      addToCart,
      toggleWishlist,
      formatPrice
    };
  }
};
</script>

<style scoped>
/* Tùy chỉnh scrollbar */
.scrollbar-thin {
  scrollbar-width: thin;
}

.scrollbar-thin::-webkit-scrollbar {
  height: 8px;
}

.scrollbar-thumb-gray-500::-webkit-scrollbar-thumb {
  background-color: #6b7280;
  border-radius: 9999px;
}

.scrollbar-track-gray-200::-webkit-scrollbar-track {
  background-color: #e5e7eb;
}
</style>