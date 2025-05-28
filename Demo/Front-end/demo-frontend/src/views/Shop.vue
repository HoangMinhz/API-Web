<template>
  <div class="min-h-screen bg-gray-50">
    <!-- Hero Section -->
    <div class="bg-white shadow-sm">
      <div class="container mx-auto px-4 py-6 mt-16">
        <h1 class="text-4xl font-bold text-gray-900 mb-4">Shop</h1>
        <p class="text-gray-600">Browse our collection of products</p>
      </div>
    </div>

    <!-- Main Content -->
    <div class="container mx-auto px-4 py-8">
      <!-- Filters Section -->
      <div class="mb-8 bg-white rounded-lg shadow-sm p-6">
        <div class="flex flex-col md:flex-row gap-4">
          <!-- Search Bar -->
          <div class="flex-1">
            <div class="relative">
              <input
                v-model="searchQuery"
                type="text"
                placeholder="Search products..."
                class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
              <svg
                class="absolute right-3 top-1/2 transform -translate-y-1/2 h-5 w-5 text-gray-400"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            </div>
          </div>

          <!-- Category Filter -->
          <div class="w-full md:w-64">
            <select
              v-model="selectedCategory"
              class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            >
              <option value="">All Categories</option>
              <option v-for="category in categories" :key="category.id" :value="category.id">
                {{ category.name }}
              </option>
            </select>
          </div>
        </div>
      </div>

      <!-- Products Grid -->
      <div id="products-section" v-if="!loading && products.length > 0" class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
        <ProductCard
          v-for="product in paginatedProducts"
          :key="product.id"
          :product="product"
          :categories="categories"
        />
      </div>

      <!-- Pagination -->
      <div v-if="totalPages > 1" class="mt-8 flex justify-center">
        <nav class="flex items-center space-x-2">
          <!-- Previous Page Button -->
          <button
            @click="changePage(currentPage - 1)"
            :disabled="currentPage === 1"
            class="px-3 py-2 rounded-lg border border-gray-300 text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
            </svg>
          </button>
          
          <!-- Page Numbers -->
          <div class="flex items-center space-x-2">
            <button
              v-for="page in getPageNumbers()"
              :key="page"
              @click="typeof page === 'number' ? changePage(page) : null"
              :class="[
                'px-4 py-2 rounded-lg',
                typeof page === 'number' ? (
                  page === currentPage
                    ? 'bg-gray-800 text-white'
                    : 'border border-gray-300 text-gray-700 hover:bg-gray-50'
                ) : 'text-gray-400'
              ]"
              :disabled="typeof page !== 'number'"
            >
              {{ page }}
            </button>
          </div>
          
          <!-- Next Page Button -->
          <button
            @click="changePage(currentPage + 1)"
            :disabled="currentPage === totalPages"
            class="px-3 py-2 rounded-lg border border-gray-300 text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
            </svg>
          </button>
        </nav>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="text-center py-12">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500 mx-auto"></div>
        <p class="mt-4 text-gray-600">Loading products...</p>
      </div>

      <!-- No Results -->
      <div v-if="!loading && products.length === 0" class="text-center py-12">
        <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.172 16.172a4 4 0 015.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <h3 class="mt-2 text-lg font-medium text-gray-900">No products found</h3>
        <p class="mt-1 text-gray-500">Try adjusting your search or filter to find what you're looking for.</p>
      </div>
    </div>
  </div>
</template>

<script>
import api from '../services/api';
import formatPrice from '../utils/formatPrice';
import ProductCard from '../components/ProductCard.vue';

export default {
  name: 'ShopPage',
  components: {
    ProductCard
  },
  data() {
    return {
      products: [],
      categories: [],
      loading: true,
      searchQuery: '',
      selectedCategory: '',
      currentPage: 1,
      itemsPerPage: 12
    };
  },
  computed: {
    filteredProducts() {
      if (!Array.isArray(this.products)) {
        return [];
      }
      
      return this.products.filter(product => {
        const matchesSearch = product.name.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
                            product.description.toLowerCase().includes(this.searchQuery.toLowerCase());
        const matchesCategory = !this.selectedCategory || product.categoryId === parseInt(this.selectedCategory);
        return matchesSearch && matchesCategory;
      });
    },
    paginatedProducts() {
      const startIndex = (this.currentPage - 1) * this.itemsPerPage;
      const endIndex = startIndex + this.itemsPerPage;
      return this.filteredProducts.slice(startIndex, endIndex);
    },
    totalPages() {
      return Math.ceil(this.filteredProducts.length / this.itemsPerPage);
    }
  },
  methods: {
    formatPrice,
    async fetchProducts() {
      try {
        console.log('Fetching products...');
        const response = await api.get('/Product/list');
        console.log('Products response:', response.data);
        
        // Process and log each product's image URL
        this.products = response.data.map(product => {
          console.log(`Product: ${product.name}`);
          console.log('Original Image URL:', product.imageUrl);
          
          // Add default rating and sold count if not present
          if (!product.rating) product.rating = Math.floor(Math.random() * 3) + 3; // Random rating between 3-5
          if (!product.soldCount) product.soldCount = Math.floor(Math.random() * 1000); // Random sold count
          
          // Fix image URL if it's relative
          if (product.imageUrl && !product.imageUrl.startsWith('http')) {
            product.imageUrl = `${api.defaults.baseURL}${product.imageUrl}`;
            console.log('Fixed Image URL:', product.imageUrl);
          }
          
          return product;
        });
      } catch (error) {
        console.error('Error fetching products:', error);
        this.products = [];
      } finally {
        this.loading = false;
      }
    },
    async fetchCategories() {
      try {
        console.log('Fetching categories...');
        const response = await api.get('/Category/list');
        console.log('Categories response:', response.data);
        this.categories = response.data || [];
      } catch (error) {
        console.error('Error fetching categories:', error);
        this.categories = [];
      }
    },
    changePage(page) {
      if (page >= 1 && page <= this.totalPages) {
        this.currentPage = page;
        // Scroll to top of the products section
        const productsSection = document.querySelector('#products-section');
        if (productsSection) {
          productsSection.scrollIntoView({ behavior: 'smooth' });
        }
      }
    },
    getPageNumbers() {
      const pages = [];
      const maxVisiblePages = 5;
      
      if (this.totalPages <= maxVisiblePages) {
        // Show all pages if total pages are less than max visible
        for (let i = 1; i <= this.totalPages; i++) {
          pages.push(i);
        }
      } else {
        // Always show first page
        pages.push(1);
        
        let startPage = Math.max(2, this.currentPage - 1);
        let endPage = Math.min(this.totalPages - 1, this.currentPage + 1);
        
        // Add ellipsis after first page if needed
        if (startPage > 2) {
          pages.push('...');
        }
        
        // Add pages around current page
        for (let i = startPage; i <= endPage; i++) {
          pages.push(i);
        }
        
        // Add ellipsis before last page if needed
        if (endPage < this.totalPages - 1) {
          pages.push('...');
        }
        
        // Always show last page
        pages.push(this.totalPages);
      }
      
      return pages;
    }
  },
  watch: {
    searchQuery() {
      this.currentPage = 1; // Reset to first page when search changes
    },
    selectedCategory() {
      this.currentPage = 1; // Reset to first page when category changes
    }
  },
  created() {
    this.fetchProducts();
    this.fetchCategories();
  }
};
</script>

<style scoped>
.aspect-w-1 {
  position: relative;
  padding-bottom: 100%;
}

.aspect-w-1 > * {
  position: absolute;
  height: 100%;
  width: 100%;
  top: 0;
  right: 0;
  bottom: 0;
  left: 0;
}

.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.min-h-\[3rem\] {
  min-height: 3rem;
}

/* Enhanced transition effects */
.transition-all {
  transition-property: all;
  transition-timing-function: cubic-bezier(0.4, 0, 0.2, 1);
  transition-duration: 500ms;
}

/* Card hover effects */
.group:hover {
  transform: translateY(-0.5rem);
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
}

/* Image hover effect */
.group:hover .group-hover\:scale-110 {
  transform: scale(1.1);
}

/* Background color transition */
.group-hover\:bg-gray-50 {
  background-color: rgb(249 250 251);
}
</style> 