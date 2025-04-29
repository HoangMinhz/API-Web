<template>
  <div class="group relative bg-white rounded-lg shadow-md overflow-hidden h-[500px] flex flex-col">
    <!-- Product Image -->
    <div class="relative h-64 overflow-hidden">
      <img
        :src="getProductImage(product)"
        :alt="product.name"
        class="w-full h-full object-cover transform group-hover:scale-105 transition-transform duration-300"
      />
      <!-- Quick View Button -->
      <button
        @click="openQuickView"
        class="absolute inset-0 flex items-center justify-center bg-black bg-opacity-0 group-hover:bg-opacity-30 transition-all duration-300 opacity-0 group-hover:opacity-100"
      >
        <span class="text-white font-medium">Quick View</span>
      </button>
      <!-- Wishlist Button -->
      <button
        @click="toggleWishlist"
        class="absolute top-2 right-2 p-2 bg-white bg-opacity-80 rounded-full hover:bg-opacity-100 transition-all duration-200"
        :class="{ 'text-red-500': isInWishlist }"
      >
        <svg
          class="w-5 h-5"
          fill="currentColor"
          viewBox="0 0 20 20"
        >
          <path
            fill-rule="evenodd"
            d="M3.172 5.172a4 4 0 015.656 0L10 6.343l1.172-1.171a4 4 0 115.656 5.656L10 17.657l-6.828-6.829a4 4 0 010-5.656z"
            clip-rule="evenodd"
          />
        </svg>
      </button>
    </div>

    <!-- Product Info -->
    <div class="p-4 flex-1 flex flex-col">
      <h3 class="text-lg font-semibold text-gray-900 mb-2">{{ product.name }}</h3>
      <p class="text-gray-600 text-sm mb-4 line-clamp-2">{{ product.description }}</p>
      <div class="mt-auto">
        <p class="text-lg font-bold text-gray-900">{{ formatPrice(product.price) }}</p>
        <div class="mt-4 flex items-center justify-between">
          <span class="text-sm text-gray-500">In Stock: {{ product.stock }}</span>
          <button
            @click="addToCart"
            class="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition-colors"
          >
            Add to Cart
          </button>
        </div>
      </div>
    </div>

    <!-- Quick View Modal -->
    <div
      v-if="showQuickView"
      class="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center"
      @click.self="closeQuickView"
    >
      <div class="bg-white rounded-lg max-w-4xl w-full mx-4 max-h-[90vh] overflow-y-auto">
        <div class="p-6">
          <div class="flex justify-between items-start mb-4">
            <h2 class="text-2xl font-bold">{{ product.name }}</h2>
            <button
              @click="closeQuickView"
              class="text-gray-500 hover:text-gray-700"
            >
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <img
                :src="getProductImage(product)"
                :alt="product.name"
                class="w-full h-96 object-cover rounded-lg"
              />
            </div>
            <div>
              <p class="text-gray-600 mb-4">{{ product.description }}</p>
              <p class="text-2xl font-bold text-gray-900 mb-4">{{ formatPrice(product.price) }}</p>
              <div class="mb-4">
                <label class="block text-sm font-medium text-gray-700 mb-2">Quantity:</label>
                <div class="flex items-center">
                  <button
                    @click="decreaseQuantity"
                    :disabled="quantity <= 1"
                    class="px-3 py-1 border rounded-l-lg hover:bg-gray-100 disabled:opacity-50"
                  >
                    -
                  </button>
                  <input
                    v-model.number="quantity"
                    type="number"
                    min="1"
                    :max="product.stock"
                    class="w-16 text-center border-t border-b border-gray-300 py-1"
                  />
                  <button
                    @click="increaseQuantity"
                    :disabled="quantity >= product.stock"
                    class="px-3 py-1 border rounded-r-lg hover:bg-gray-100 disabled:opacity-50"
                  >
                    +
                  </button>
                </div>
              </div>
              <button
                @click="addToCart"
                class="w-full bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-700 transition-colors"
              >
                Add to Cart
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, computed } from 'vue';
import { useStore } from 'vuex';
import formatPrice from '../../utils/formatPrice';

export default {
  name: 'ProductCard',
  props: {
    product: {
      type: Object,
      required: true
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