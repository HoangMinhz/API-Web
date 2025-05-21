<template>
    <div class="pt-16">
      <!-- Section Sản phẩm theo danh mục -->
      <div class="container mx-auto px-4 py-12">
        <h2 class="text-3xl font-bold text-gray-800 mb-6">Products in {{ categoryName || 'Category' }}</h2>
        <div v-if="productsError" class="text-red-600 mb-4">{{ productsError }}</div>
        <div v-else-if="!filteredProducts.length && !productsError" class="flex space-x-6">
          <div v-for="n in 6" :key="n" class="flex-shrink-0 w-64 bg-white p-4 rounded-lg shadow-lg animate-pulse">
            <div class="bg-gray-200 w-full h-48 rounded-md mb-4"></div>
            <div class="bg-gray-200 h-6 w-3/4 rounded mb-2"></div>
            <div class="bg-gray-200 h-4 w-full rounded mb-2"></div>
            <div class="bg-gray-200 h-6 w-1/2 rounded"></div>
          </div>
        </div>
        <div v-else class="relative">
          <!-- Nút cuộn -->
          <button
            @click="scrollLeft('category-products')"
            class="absolute left-0 top-1/2 transform -translate-y-1/2 bg-gray-800 text-white p-2 rounded-full hover:bg-gray-900 transition"
          >
            <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
            </svg>
          </button>
          <button
            @click="scrollRight('category-products')"
            class="absolute right-0 top-1/2 transform -translate-y-1/2 bg-gray-800 text-white p-2 rounded-full hover:bg-gray-900 transition"
          >
            <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
            </svg>
          </button>
          <!-- Grid sản phẩm -->
          <div id="category-products" class="flex overflow-x-auto space-x-6 pb-4 scrollbar-thin scrollbar-thumb-gray-500 scrollbar-track-gray-200">
            <div
              v-for="product in filteredProducts"
              :key="product.id"
              class="flex-shrink-0 w-64 bg-white p-4 rounded-lg shadow-lg hover:shadow-xl transition transform hover:-translate-y-1"
            >
              <router-link :to="`/products/${product.id}`">
                <img
                  :src="product.imageUrl || 'https://via.placeholder.com/150'"
                  :alt="product.name"
                  class="w-full h-48 object-cover rounded-md mb-4 transition-opacity hover:opacity-90"
                  loading="lazy"
                />
                <h3 class="text-lg font-medium text-gray-800">{{ product.name }}</h3>
                <p class="text-sm text-gray-600 line-clamp-2">{{ product.description }}</p>
                <p class="text-lg font-bold text-gray-800 mt-2">{{ formatPrice(product.price) }}</p>
                <p class="text-sm text-gray-500">{{ product.categoryName }}</p>
              </router-link>
              <button
                @click="addToCart(product)"
                class="mt-4 w-full bg-gray-800 text-white py-2 rounded-md hover:bg-gray-900 transition transform hover:scale-105"
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
  import api from '../services/api';
  import formatPrice from '../utils/formatPrice';
  
  export default {
    data() {
      return {
        products: [],
        categories: [],
        productsError: null,
        categoryName: '',
      };
    },
    computed: {
      filteredProducts() {
        const categoryId = parseInt(this.$route.params.id);
        return this.products.filter(product => product.categoryId === categoryId);
      },
    },
    async created() {
      await Promise.all([this.fetchProducts(), this.fetchCategories()]);
      this.setCategoryName();
    },
    methods: {
      formatPrice,
      async fetchProducts() {
        try {
          const response = await api.get('/Product/list');
          this.products = response.data;
        } catch (err) {
          this.productsError = err.response?.data?.message || 'Failed to load products';
          console.error('Fetch products error:', err);
        }
      },
      async fetchCategories() {
        try {
          const response = await api.get('/Category/list');
          this.categories = response.data;
        } catch (err) {
          console.error('Fetch categories error:', err);
        }
      },
      setCategoryName() {
        const categoryId = parseInt(this.$route.params.id);
        const category = this.categories.find(cat => cat.id === categoryId);
        this.categoryName = category ? category.name : 'Category';
      },
      addToCart(product) {
        this.$store.dispatch('cart/addToCart', { productId: product.id, quantity: 1 });
      },
      scrollLeft(sectionId) {
        const container = document.getElementById(sectionId);
        if (container) {
          container.scrollBy({ left: -300, behavior: 'smooth' });
        }
      },
      scrollRight(sectionId) {
        const container = document.getElementById(sectionId);
        if (container) {
          container.scrollBy({ left: 300, behavior: 'smooth' });
        }
      },
    },
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