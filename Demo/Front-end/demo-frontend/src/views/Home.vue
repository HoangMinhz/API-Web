<template>
  <div class="pt-16">
    <!-- Hero Banner -->
    <div class="relative bg-gradient-to-br from-gray-900 via-gray-800 to-blue-900 text-white overflow-hidden">
      <!-- Background Pattern -->
      <div class="absolute inset-0 opacity-10">
        <div class="absolute inset-0 bg-[url('data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNjAiIGhlaWdodD0iNjAiIHZpZXdCb3g9IjAgMCA2MCA2MCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48ZyBmaWxsPSJub25lIiBmaWxsLXJ1bGU9ImV2ZW5vZGQiPjxwYXRoIGQ9Ik0zNiAzNGM0LjQxOCAwIDgtMy41ODIgOC04cy0zLjU4Mi04LTgtOC04IDMuNTgyLTggOCAzLjU4MiA4IDggOHoiIHN0cm9rZT0iI0ZGRiIgc3Ryb2tlLXdpZHRoPSIyIi8+PC9nPjwvc3ZnPg==')]"></div>
      </div>
      
      <div class="container mx-auto px-4 py-20 md:py-32">
        <div class="flex flex-col md:flex-row items-center justify-between gap-12">
          <!-- Left Content -->
          <div class="text-center md:text-left md:w-1/2 space-y-6">
            <h1 class="text-4xl md:text-6xl font-bold leading-tight">
              <span class="text-blue-400">Tech</span> Innovation
              <br />
              <span class="text-white">At Your</span> Fingertips
            </h1>
            <p class="text-lg md:text-xl text-gray-300 max-w-lg">
              Discover the latest in technology, from cutting-edge gadgets to premium accessories. Your one-stop shop for all things tech.
            </p>
            <div class="flex flex-col sm:flex-row gap-4 justify-center md:justify-start">
              <router-link
                to="/shop"
                class="inline-flex items-center justify-center px-8 py-3 bg-blue-600 text-white rounded-lg font-semibold hover:bg-blue-700 transition transform hover:scale-105"
              >
                Shop Now
                <svg class="w-5 h-5 ml-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 8l4 4m0 0l-4 4m4-4H3" />
                </svg>
              </router-link>
            </div>
          </div>

          <!-- Right Content - Hero Image -->
          <div class="md:w-1/2 relative">
            <div class="relative">
              <!-- Main Product Image -->
              <img
                src="https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=800&auto=format&fit=crop"
                alt="Premium Tech Products"
                class="w-full max-w-lg mx-auto rounded-2xl shadow-2xl transform hover:scale-105 transition duration-500"
              />
              
              <!-- Floating Elements -->
              <div class="absolute -top-4 -left-4 w-24 h-24 bg-blue-500/20 rounded-full blur-xl"></div>
              <div class="absolute -bottom-4 -right-4 w-24 h-24 bg-purple-500/20 rounded-full blur-xl"></div>
              
              <!-- Tech Icons -->
              <div class="absolute top-4 right-4 flex space-x-2">
                <div class="bg-white/10 backdrop-blur-sm p-2 rounded-lg">
                  <svg class="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 3v2m6-2v2M9 19v2m6-2v2M5 9H3m2 6H3m18-6h-2m2 6h-2M7 19h10a2 2 0 002-2V7a2 2 0 00-2-2H7a2 2 0 00-2 2v10a2 2 0 002 2zM9 9h6v6H9V9z" />
                  </svg>
                </div>
                <div class="bg-white/10 backdrop-blur-sm p-2 rounded-lg">
                  <svg class="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 18h.01M8 21h8a2 2 0 002-2V5a2 2 0 00-2-2H8a2 2 0 00-2 2v14a2 2 0 002 2z" />
                  </svg>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Section Danh mục (Carousel) -->
    <div class="container mx-auto px-4 py-12">
      <h2 class="text-3xl font-bold text-gray-800 mb-6 text-center">Explore Categories</h2>
      <div v-if="categoriesError" class="text-red-600 mb-4 text-center">{{ categoriesError }}</div>
      <div v-else-if="!categories.length" class="text-gray-600 mb-4 text-center">Loading categories...</div>
      <div v-else class="relative">
        <button
          @click="scrollLeft('categories')"
          class="absolute left-0 top-1/2 transform -translate-y-1/2 bg-white/80 backdrop-blur-sm text-gray-800 p-3 rounded-full hover:bg-white hover:shadow-lg transition-all duration-300 z-10"
        >
          <svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
          </svg>
        </button>
        <button
          @click="scrollRight('categories')"
          class="absolute right-0 top-1/2 transform -translate-y-1/2 bg-white/80 backdrop-blur-sm text-gray-800 p-3 rounded-full hover:bg-white hover:shadow-lg transition-all duration-300 z-10"
        >
          <svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
          </svg>
        </button>
        <div id="categories" class="flex overflow-x-auto space-x-6 pb-4 scrollbar-thin scrollbar-thumb-gray-500 scrollbar-track-gray-200">
          <router-link
            v-for="category in categories"
            :key="category.id"
            :to="`/category/${category.id}`"
            class="flex-shrink-0 w-72 group relative overflow-hidden rounded-lg shadow-lg hover:shadow-xl transition-shadow duration-300"
          >
            <div class="aspect-w-16 aspect-h-9">
              <img
                :src="category.imageUrl || 'https://via.placeholder.com/400x225?text=' + category.name"
                :alt="category.name"
                class="object-cover w-full h-full transform group-hover:scale-110 transition-transform duration-300"
              />
              <div class="absolute inset-0 bg-black bg-opacity-40 flex items-center justify-center">
                <span class="text-white text-xl font-semibold">{{ category.name }}</span>
              </div>
            </div>
          </router-link>
        </div>
      </div>
    </div>

    <!-- Modal Quick View -->
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

    <!-- Section Sản phẩm mới (New Arrivals) -->
    <div class="container mx-auto px-4 py-12">
      <div class="flex justify-between items-center mb-6">
        <h2 class="text-3xl font-bold text-gray-800">New Arrivals</h2>
        <div class="relative">
          <select
            v-model="newProductsFilter"
            @change="filterNewProducts"
            class="appearance-none px-4 py-2 pr-8 rounded-md bg-gray-100 text-gray-800 border border-gray-300 focus:outline-none focus:ring-2 focus:ring-gray-500"
          >
            <option value="all">All Categories</option>
            <option v-for="category in categories" :key="category.id" :value="category.id">
              {{ category.name }}
            </option>
          </select>
          <svg class="absolute right-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
          </svg>
        </div>
      </div>
      <div v-if="newProductsError" class="text-red-600 mb-4">{{ newProductsError }}</div>
      <div v-else-if="!filteredNewProducts.length" class="text-gray-600 mb-4">Loading new products...</div>
      <div v-else class="relative">
        <button
          @click="scrollLeft('new-products')"
          class="absolute left-0 top-1/2 transform -translate-y-1/2 bg-gray-800 text-white p-2 rounded-full hover:bg-gray-900 transition"
        >
          <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
          </svg>
        </button>
        <button
          @click="scrollRight('new-products')"
          class="absolute right-0 top-1/2 transform -translate-y-1/2 bg-gray-800 text-white p-2 rounded-full hover:bg-gray-900 transition"
        >
          <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
          </svg>
        </button>
        <div id="new-products" class="flex overflow-x-auto space-x-6 pb-4 scrollbar-thin scrollbar-thumb-gray-500 scrollbar-track-gray-200">
          <div
            v-for="product in filteredNewProducts"
            :key="product.id"
            class="flex-shrink-0 w-72 relative bg-white rounded-xl shadow-lg hover:shadow-2xl transition-all duration-300 transform hover:-translate-y-2 h-[500px] flex flex-col"
          >
            <router-link :to="`/products/${product.id}`" class="flex-1 flex flex-col">
              <div class="relative overflow-hidden rounded-t-xl flex-1">
                <img
                  :src="product.imageUrl || 'https://via.placeholder.com/150'"
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
                      :class="star <= product.rating ? 'text-yellow-400' : 'text-gray-300'"
                      fill="currentColor" 
                      viewBox="0 0 20 20"
                    >
                      <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                    </svg>
                  </div>
                  <span class="text-sm text-gray-500 ml-1">({{ product.rating }})</span>
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
                @click="openQuickView(product)"
                class="flex-1 bg-gray-200 text-gray-800 py-2 rounded-md hover:bg-gray-300 transition"
              >
                Quick View
              </button>
              <button
                @click="addToCart(product)"
                class="flex-1 bg-gray-800 text-white py-2 rounded-md hover:bg-gray-900 transition"
              >
                Add to Cart
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Section Sản phẩm nổi bật (Featured Products) -->
    <div class="container mx-auto px-4 py-12">
      <div class="flex justify-between items-center mb-6">
        <h2 class="text-3xl font-bold text-gray-800">Featured Products</h2>
        <div class="relative">
          <select
            v-model="featuredProductsFilter"
            @change="filterFeaturedProducts"
            class="appearance-none px-4 py-2 pr-8 rounded-md bg-gray-100 text-gray-800 border border-gray-300 focus:outline-none focus:ring-2 focus:ring-gray-500"
          >
            <option value="all">All Categories</option>
            <option v-for="category in categories" :key="category.id" :value="category.id">
              {{ category.name }}
            </option>
          </select>
          <svg class="absolute right-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
          </svg>
        </div>
      </div>
      <div v-if="productsError" class="text-red-600 mb-4">{{ productsError }}</div>
      <div v-else-if="!filteredFeaturedProducts.length && !productsError" class="flex space-x-6">
        <div v-for="n in 6" :key="n" class="flex-shrink-0 w-72 bg-white rounded-xl shadow-lg animate-pulse">
          <div class="w-full h-56 bg-gray-200 rounded-t-xl"></div>
          <div class="p-4 space-y-3">
            <div class="h-5 bg-gray-200 rounded w-3/4"></div>
            <div class="h-4 bg-gray-200 rounded w-full"></div>
            <div class="h-4 bg-gray-200 rounded w-5/6"></div>
            <div class="flex justify-between">
              <div class="h-5 bg-gray-200 rounded w-1/3"></div>
              <div class="h-4 bg-gray-200 rounded w-1/4"></div>
            </div>
            <div class="flex justify-between">
              <div class="h-4 bg-gray-200 rounded w-1/3"></div>
              <div class="h-4 bg-gray-200 rounded w-1/4"></div>
            </div>
            <div class="flex space-x-2">
              <div class="h-10 bg-gray-200 rounded w-1/2"></div>
              <div class="h-10 bg-gray-200 rounded w-1/2"></div>
            </div>
          </div>
        </div>
      </div>
      <div v-else class="relative">
        <button
          @click="scrollLeft('featured-products')"
          class="absolute left-0 top-1/2 transform -translate-y-1/2 bg-gray-800 text-white p-2 rounded-full hover:bg-gray-900 transition"
        >
          <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
          </svg>
        </button>
        <button
          @click="scrollRight('featured-products')"
          class="absolute right-0 top-1/2 transform -translate-y-1/2 bg-gray-800 text-white p-2 rounded-full hover:bg-gray-900 transition"
        >
          <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
          </svg>
        </button>
        <div id="featured-products" class="flex overflow-x-auto space-x-6 pb-4 scrollbar-thin scrollbar-thumb-gray-500 scrollbar-track-gray-200">
          <div
            v-for="product in filteredFeaturedProducts"
            :key="product.id"
            class="flex-shrink-0 w-72 relative bg-white rounded-xl shadow-lg hover:shadow-2xl transition-all duration-300 transform hover:-translate-y-2 h-[500px] flex flex-col"
          >
            <router-link :to="`/products/${product.id}`" class="flex-1 flex flex-col">
              <div class="relative overflow-hidden rounded-t-xl flex-1">
                <img
                  :src="product.imageUrl || 'https://via.placeholder.com/150'"
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
                      :class="star <= product.rating ? 'text-yellow-400' : 'text-gray-300'"
                      fill="currentColor" 
                      viewBox="0 0 20 20"
                    >
                      <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                    </svg>
                  </div>
                  <span class="text-sm text-gray-500 ml-1">({{ product.rating }})</span>
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
                @click="openQuickView(product)"
                class="flex-1 bg-gray-200 text-gray-800 py-2 rounded-md hover:bg-gray-300 transition"
              >
                Quick View
              </button>
              <button
                @click="addToCart(product)"
                class="flex-1 bg-gray-800 text-white py-2 rounded-md hover:bg-gray-900 transition"
              >
                Add to Cart
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Section Khuyến mãi -->
    <div class="bg-gray-50 py-12">
      <div class="container mx-auto px-4 text-center">
        <h2 class="text-3xl font-bold text-gray-800 mb-4">Special Offers</h2>
        <p class="text-lg text-gray-600 mb-6">Get up to 50% off on selected items this week!</p>
        <router-link to="/categories" class="inline-block bg-gray-800 text-white py-3 px-8 rounded-full font-semibold hover:bg-gray-900 transition transform hover:scale-105">
          View Deals
        </router-link>
      </div>
    </div>
  </div>
</template>

<script>
import api from '../services/api';
import formatPrice from '../utils/formatPrice';

export default {
  name: 'HomePage',
  data() {
    return {
      products: [],
      newProducts: [],
      categories: [],
      productsError: null,
      newProductsError: null,
      categoriesError: null,
      productsPerSection: 6,
      showQuickView: false,
      selectedProduct: null,
      newProductsFilter: 'all',
      featuredProductsFilter: 'all',
      originalNewProducts: [],
      originalFeaturedProducts: [],
    };
  },
  computed: {
    filteredNewProducts() {
      if (this.newProductsFilter === 'all') {
        return this.newProducts;
      }
      const categoryId = parseInt(this.newProductsFilter);
      return this.newProducts.filter(product => product.categoryId === categoryId);
    },
    filteredFeaturedProducts() {
      if (this.featuredProductsFilter === 'all') {
        return this.products.slice(0, this.productsPerSection);
      }
      const categoryId = parseInt(this.featuredProductsFilter);
      return this.products.slice(0, this.productsPerSection).filter(product => product.categoryId === categoryId);
    },
  },
  async created() {
    await Promise.all([this.fetchProducts(), this.fetchNewProducts(), this.fetchCategories()]);
    this.originalNewProducts = [...this.newProducts];
    this.originalFeaturedProducts = [...this.products];
  },
  methods: {
    formatPrice,
    async fetchProducts() {
      try {
        const response = await api.get('/Product');
        this.products = response.data.sort((a, b) => {
          const scoreA = (a.soldCount || 0) * 0.6 + (a.rating || 0) * 0.4;
          const scoreB = (b.soldCount || 0) * 0.6 + (b.rating || 0) * 0.4;
          return scoreB - scoreA;
        });
      } catch (err) {
        this.productsError = err.response?.data?.message || 'Failed to load products';
        console.error('Fetch products error:', err);
      }
    },
    async fetchNewProducts() {
      try {
        const response = await api.get('/Product?new=true');
        this.newProducts = response.data
          .slice(0, 8)
          .sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
      } catch (err) {
        this.newProductsError = err.response?.data?.message || 'Failed to load new products';
        console.error('Fetch new products error:', err);
      }
    },
    async fetchCategories() {
      try {
        const response = await api.get('/Category');
        this.categories = response.data;
        console.log('Categories data:', this.categories);
        // Debug each category's image URL
        this.categories.forEach(category => {
          console.log(`Category ${category.name}:`, {
            id: category.id,
            name: category.name,
            imageUrl: category.imageUrl,
            hasImage: !!category.imageUrl
          });
        });
      } catch (err) {
        this.categoriesError = err.response?.data?.message || 'Failed to load categories';
        console.error('Fetch categories error:', err);
      }
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
    openQuickView(product) {
      this.selectedProduct = product;
      this.showQuickView = true;
    },
    closeQuickView() {
      this.showQuickView = false;
      this.selectedProduct = null;
    },
    filterNewProducts() {
      if (this.newProductsFilter === 'all') {
        this.newProducts = [...this.originalNewProducts];
      } else {
        const categoryId = parseInt(this.newProductsFilter);
        this.newProducts = this.originalNewProducts.filter(product => product.categoryId === categoryId);
      }
    },
    filterFeaturedProducts() {
      if (this.featuredProductsFilter === 'all') {
        this.products = [...this.originalFeaturedProducts];
      } else {
        const categoryId = parseInt(this.featuredProductsFilter);
        this.products = this.originalFeaturedProducts.filter(product => product.categoryId === categoryId);
      }
    },
    getCategoryName(categoryId) {
      const category = this.categories.find(c => c.id === categoryId);
      return category ? category.name : 'Unknown Category';
    },
    getProductImage(product) {
      return product.imageUrl || 'https://via.placeholder.com/150';
    },
  },
};
</script>

<style scoped>
/* Hiệu ứng fade-in */
.animate-fade-in {
  animation: fadeIn 1s ease-in-out;
}

.animation-delay-200 {
  animation-delay: 200ms;
}

.animation-delay-400 {
  animation-delay: 400ms;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

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