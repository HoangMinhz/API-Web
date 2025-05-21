<template>
  <nav class="bg-white/80 backdrop-blur-md shadow-sm fixed top-0 left-0 w-full z-50 transition-all duration-300">
    <div class="container mx-auto px-4">
      <div class="flex items-center justify-between h-16">
        <!-- Logo -->
        <router-link to="/" class="flex items-center space-x-2 group">
          <span class="text-2xl font-bold bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent">
            MyShop
          </span>
        </router-link>

        <!-- Desktop Menu -->
        <div class="hidden md:flex items-center space-x-8">
          <router-link 
            v-for="link in mainLinks" 
            :key="link.to"
            :to="link.to"
            class="text-gray-600 hover:text-blue-600 transition-colors duration-200 relative group"
            active-class="text-blue-600"
          >
            {{ link.text }}
            <span class="absolute bottom-0 left-0 w-0 h-0.5 bg-blue-600 transition-all duration-200 group-hover:w-full"></span>
          </router-link>
        </div>

        <!-- Right Section -->
        <div class="flex items-center space-x-4">
          <!-- Search Bar -->
          <div class="relative hidden md:block">
            <input
              v-model="searchQuery"
              @input="handleSearch"
              type="text"
              placeholder="Search products..."
              class="w-64 px-4 py-2 rounded-full bg-gray-100 text-gray-700 border border-transparent focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all duration-200"
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

          <!-- Cart Icon -->
          <div class="relative group">
            <router-link to="/cart" class="p-2 rounded-full hover:bg-gray-100 transition-colors duration-200">
              <div class="relative">
                <svg class="w-6 h-6 text-gray-600" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                  <path d="M3.5 3.5h2.5l1.5 4h12l-2 6H7.5l-4-10z" />
                  <circle cx="7.5" cy="17.5" r="1.5" />
                  <circle cx="16.5" cy="17.5" r="1.5" />
                </svg>
                <span v-if="$store.state.cart.items.length" 
                  class="absolute -top-1 -right-1 bg-red-500 text-white text-xs rounded-full w-4 h-4 flex items-center justify-center">
                  {{ $store.state.cart.items.length }}
                </span>
              </div>
            </router-link>
            <!-- Cart Preview Dropdown -->
            <div class="absolute right-0 hidden group-hover:block bg-white shadow-lg rounded-lg mt-2 w-80 overflow-hidden">
              <div v-if="$store.state.cart.items.length > 0" class="max-h-96 overflow-y-auto">
                <!-- Cart Header -->
                <div class="p-4 border-b border-gray-100">
                  <div class="flex items-center justify-between">
                    <h3 class="text-lg font-semibold text-gray-900">Your Cart</h3>
                    <span class="text-sm text-gray-500">{{ $store.state.cart.items.length }} items</span>
                  </div>
                </div>

                <!-- Cart Items -->
                <div class="divide-y divide-gray-100">
                  <div v-for="item in $store.state.cart.items" :key="item.id" class="p-4 hover:bg-gray-50 transition-colors duration-150">
                    <div class="flex items-center space-x-4">
                      <div class="relative flex-shrink-0">
                        <img 
                          :src="item.product?.imageUrl || 'https://via.placeholder.com/50'" 
                          :alt="item.product?.name" 
                          class="w-14 h-14 object-cover rounded-md"
                        />
                        <span class="absolute -top-1 -right-1 bg-blue-500 text-white text-xs rounded-full w-5 h-5 flex items-center justify-center">
                          {{ item.quantity }}
                        </span>
                      </div>
                      <div class="flex-1 min-w-0">
                        <p class="text-sm font-medium text-gray-900 truncate">{{ item.product?.name }}</p>
                        <div class="mt-1 flex items-center justify-between">
                          <span class="text-sm text-gray-500">{{ formatPrice(item.product?.price) }}</span>
                          <span class="text-sm font-medium text-gray-900">{{ formatPrice(item.product?.price * item.quantity) }}</span>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>

                <!-- Cart Footer -->
                <div class="p-4 bg-gray-50 border-t border-gray-100">
                  <div class="flex justify-between items-center mb-4">
                    <span class="text-sm font-medium text-gray-900">Subtotal</span>
                    <span class="text-lg font-semibold text-gray-900">{{ formatPrice($store.getters['cart/subtotal']) }}</span>
                  </div>
                  <router-link 
                    to="/cart" 
                    class="block w-full px-4 py-2.5 bg-gray-900 text-white text-center rounded-md hover:bg-gray-800 transition-colors duration-150 text-sm font-medium"
                  >
                    View Cart
                  </router-link>
                </div>
              </div>

              <!-- Empty Cart State -->
              <div v-else class="p-6 text-center">
                <div class="w-12 h-12 mx-auto mb-3 rounded-full bg-gray-100 flex items-center justify-center">
                  <svg class="w-6 h-6 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4z" />
                  </svg>
                </div>
                <p class="text-sm text-gray-500 mb-4">Your cart is empty</p>
                <router-link 
                  to="/shop" 
                  class="inline-block px-4 py-2 bg-gray-900 text-white rounded-md hover:bg-gray-800 transition-colors duration-150 text-sm font-medium"
                >
                  Start Shopping
                </router-link>
              </div>
            </div>
          </div>

          <!-- User Profile -->
          <div v-if="$store.getters['user/isAuthenticated']" class="relative">
            <button @click="toggleUserMenu" class="flex items-center space-x-2 focus:outline-none">
              <div class="w-8 h-8 rounded-full bg-gradient-to-r from-blue-500 to-purple-500 flex items-center justify-center text-white font-semibold">
                {{ $store.state.user.user?.username?.charAt(0).toUpperCase() || 'U' }}
              </div>
            </button>
            <div v-if="userMenuOpen" class="absolute right-0 mt-2 w-48 bg-white rounded-lg shadow-lg py-1 z-50">
              <router-link
                to="/profile"
                class="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
              >
                My Profile
              </router-link>
              <router-link
                to="/orders"
                class="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
              >
                My Orders
              </router-link>
              <router-link
                v-if="$store.getters['user/isAdmin']"
                to="/admin/orders"
                class="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
              >
                Orders Management
              </router-link>
              <button
                @click="logout"
                class="block w-full text-sm px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
              >
                Sign Out
              </button>
            </div>
          </div>
          <router-link v-else to="/login" class="hidden md:block px-4 py-2 rounded-full bg-blue-600 text-white hover:bg-blue-700 transition-colors duration-200">
            Sign in
          </router-link>

          <!-- Mobile Menu Button -->
          <button @click="toggleMenu" class="md:hidden p-2 rounded-lg hover:bg-gray-100 transition-colors duration-200">
            <svg class="h-6 w-6 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path v-if="!menuOpen" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
              <path v-else stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
      </div>
    </div>

    <!-- Mobile Menu -->
    <div v-if="menuOpen" class="md:hidden bg-white shadow-lg">
      <div class="container mx-auto px-4 py-3 space-y-3">
        <router-link 
          v-for="link in mainLinks" 
          :key="link.to"
          :to="link.to"
          @click="toggleMenu"
          class="block py-2 text-gray-600 hover:text-blue-600 transition-colors duration-200"
          active-class="text-blue-600"
        >
          {{ link.text }}
        </router-link>
        <div class="pt-4 border-t border-gray-100">
          <router-link v-if="!$store.getters['user/isAuthenticated']" to="/login" @click="toggleMenu" 
            class="block w-full px-4 py-2 text-center rounded-full bg-blue-600 text-white hover:bg-blue-700 transition-colors duration-200">
            Sign in
          </router-link>
          <button v-else @click="logout" class="block w-full px-4 py-2 text-center rounded-full bg-gray-100 text-gray-700 hover:bg-gray-200 transition-colors duration-200">
            Sign out
          </button>
        </div>
      </div>
    </div>
  </nav>
</template>

<script>
export default {
  data() {
    return {
      menuOpen: false,
      userMenuOpen: false,
      searchQuery: '',
      mainLinks: [
        { to: '/', text: 'Home' },
        { to: '/shop', text: 'Shop' },
        { to: '/admin', text: 'Admin', show: () => this.$store.getters['user/isAdmin'] }
      ].filter(link => !link.show || link.show())
    };
  },
  created() {
    // Initialize cart
    this.$store.dispatch('cart/fetchCart');
  },
  mounted() {
    window.addEventListener('scroll', this.handleScroll);
  },
  beforeUnmount() {
    window.removeEventListener('scroll', this.handleScroll);
  },
  methods: {
    toggleUserMenu() {
      this.userMenuOpen = !this.userMenuOpen;
    },
    toggleMenu() {
      this.menuOpen = !this.menuOpen;
      this.userMenuOpen = false;
    },
    handleSearch() {
      if (this.searchQuery.trim()) {
        this.$router.push(`/shop?search=${encodeURIComponent(this.searchQuery.trim())}`);
        this.searchQuery = '';
      }
    },
    handleScroll() {
      const navbar = document.querySelector('nav');
      if (window.scrollY > 50) {
        navbar.classList.add('shadow-md');
      } else {
        navbar.classList.remove('shadow-md');
      }
    },
    logout() {
      this.userMenuOpen = false;
      this.$store.dispatch('user/logout');
      this.$router.push('/login');
      this.menuOpen = false;
      this.$store.dispatch('notification/showNotification', {
        type: 'success',
        message: 'Logged out successfully!'
      });
    },
    formatPrice(price) {
      return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
      }).format(price);
    },
  },
};
</script>

<style scoped>
.router-link-active {
  @apply text-blue-600;
}
</style>