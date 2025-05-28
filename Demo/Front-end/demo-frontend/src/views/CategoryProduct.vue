<template>
    <div class="pt-16">
      <!-- Section Sản phẩm theo danh mục -->
      <div class="container mx-auto px-4 py-12">
        <h2 class="text-3xl font-bold text-gray-800 mb-6">Products in {{ categoryName || 'Category' }}</h2>
        
        <!-- Loading State -->
        <div v-if="productsLoading" class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
          <div v-for="n in 8" :key="n" class="bg-white rounded-lg shadow-md overflow-hidden h-[500px] animate-pulse">
            <div class="bg-gray-200 w-full h-64"></div>
            <div class="p-4">
              <div class="bg-gray-200 h-6 w-3/4 rounded mb-2"></div>
              <div class="bg-gray-200 h-4 w-full rounded mb-2"></div>
              <div class="bg-gray-200 h-4 w-2/3 rounded mb-4"></div>
              <div class="bg-gray-200 h-6 w-1/2 rounded"></div>
            </div>
          </div>
        </div>

        <!-- Error State -->
        <div v-else-if="productsError" class="text-center py-12">
          <div class="text-red-600 mb-4">{{ productsError }}</div>
          <button 
            @click="fetchProducts" 
            class="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition-colors"
          >
            Try Again
          </button>
        </div>

        <!-- No Products -->
        <div v-else-if="!filteredProducts.length" class="text-center py-12">
          <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2 2v-5m16 0h-2M4 13h2m13-8V4a1 1 0 00-1-1H7a1 1 0 00-1 1v1m8 0V4.5" />
          </svg>
          <h3 class="mt-2 text-lg font-medium text-gray-900">No products found</h3>
          <p class="mt-1 text-gray-500">This category doesn't have any products yet.</p>
          <router-link 
            to="/shop" 
            class="mt-4 inline-block bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition-colors"
          >
            Browse All Products
          </router-link>
        </div>

        <!-- Products Grid -->
        <div v-else class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
          <ProductCard
            v-for="product in filteredProducts"
            :key="product.id"
            :product="product"
            :categories="categories"
          />
        </div>
      </div>
    </div>
  </template>
  
  <script>
  import api from '../services/api';
  import formatPrice from '../utils/formatPrice';
  import ProductCard from '../components/ProductCard.vue';
  
  export default {
    name: 'CategoryProduct',
    components: {
      ProductCard
    },
    data() {
      return {
        products: [],
        categories: [],
        productsError: null,
        categoryName: '',
        productsLoading: true,
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
    watch: {
      '$route.params.id': {
        handler() {
          this.setCategoryName();
        },
        immediate: false
      }
    },
    methods: {
      formatPrice,
      async fetchProducts() {
        try {
          this.productsLoading = true;
          const response = await api.get('/Product/list');
          this.products = response.data;
        } catch (err) {
          this.productsError = err.response?.data?.message || 'Failed to load products';
          console.error('Fetch products error:', err);
        } finally {
          this.productsLoading = false;
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
    },
  };
  </script>
  
  <style scoped>
  /* Responsive grid adjustments */
  @media (max-width: 640px) {
    .grid {
      grid-template-columns: repeat(1, minmax(0, 1fr));
    }
  }
  
  @media (min-width: 641px) and (max-width: 768px) {
    .grid {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }
  }
  
  @media (min-width: 769px) and (max-width: 1024px) {
    .grid {
      grid-template-columns: repeat(3, minmax(0, 1fr));
    }
  }
  
  @media (min-width: 1025px) {
    .grid {
      grid-template-columns: repeat(4, minmax(0, 1fr));
    }
  }
  </style>