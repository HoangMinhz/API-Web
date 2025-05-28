<template>
  <div v-if="product" class="max-w-4xl mx-auto mt-10">
    <!-- Top Section: Grid -->
    <div class="grid grid-cols-1 md:grid-cols-2 gap-10 bg-white rounded-lg shadow-lg p-8">
      <!-- Product Image -->
      <div class="flex justify-center items-start">
        <img
          :src="product.imageUrl || 'https://via.placeholder.com/400'"
          alt="Product image"
          class="w-full max-w-xs rounded-xl shadow-md border border-gray-200 object-contain"
        />
      </div>
      <!-- Product Info -->
      <div class="flex flex-col justify-between h-full">
        <div>
          <h1 class="text-3xl font-extrabold text-gray-900 mb-2">{{ product.name }}</h1>
          <span class="inline-block bg-blue-100 text-blue-800 text-xs font-semibold px-3 py-1 rounded-full mb-4">
            {{ product.category?.name }}
          </span>
          <div class="text-2xl font-bold text-blue-700 mb-4">{{ formatPrice(product.price) }}</div>
          
          <!-- Rating and Sold Count -->
          <div class="flex items-center space-x-4 mb-4">
            <div class="flex items-center">
              <div class="flex items-center">
                <template v-for="i in 5">
                  <svg v-if="i <= Math.floor(product.rating)" :key="'full-' + i" class="w-5 h-5 text-yellow-400" fill="currentColor" viewBox="0 0 20 20">
                    <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.286 3.97a1 1 0 00.95.69h4.162c.969 0 1.371 1.24.588 1.81l-3.357 2.44a1 1 0 00-.364 1.118l1.287 3.97c.3.921-.755 1.688-1.54 1.118l-3.357-2.44a1 1 0 00-1.175 0l-3.357 2.44c-.784.57-1.838-.197-1.54-1.118l1.287-3.97a1 1 0 00-.364-1.118L2.73 9.397c-.783-.57-.38-1.81.588-1.81h4.162a1 1 0 00.95-.69l1.286-3.97z" />
                  </svg>
                  <svg v-else-if="i === Math.ceil(product.rating) && product.rating % 1 >= 0.5" :key="'half-' + i" class="w-5 h-5 text-yellow-400" fill="currentColor" viewBox="0 0 20 20">
                    <defs>
                      <clipPath id="half-star">
                        <rect x="0" y="0" width="10" height="20" />
                      </clipPath>
                    </defs>
                    <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.286 3.97a1 1 0 00.95.69h4.162c.969 0 1.371 1.24.588 1.81l-3.357 2.44a1 1 0 00-.364 1.118l1.287 3.97c.3.921-.755 1.688-1.54 1.118l-3.357-2.44a1 1 0 00-1.175 0l-3.357 2.44c-.784.57-1.838-.197-1.54-1.118l1.287-3.97a1 1 0 00-.364-1.118L2.73 9.397c-.783-.57-.38-1.81.588-1.81h4.162a1 1 0 00.95-.69l1.286-3.97z" clip-path="url(#half-star)" />
                  </svg>
                  <svg v-else :key="'empty-' + i" class="w-5 h-5 text-gray-300" fill="currentColor" viewBox="0 0 20 20">
                    <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.286 3.97a1 1 0 00.95.69h4.162c.969 0 1.371 1.24.588 1.81l-3.357 2.44a1 1 0 00-.364 1.118l1.287 3.97c.3.921-.755 1.688-1.54 1.118l-3.357-2.44a1 1 0 00-1.175 0l-3.357 2.44c-.784.57-1.838-.197-1.54-1.118l1.287-3.97a1 1 0 00-.364-1.118L2.73 9.397c-.783-.57-.38-1.81.588-1.81h4.162a1 1 0 00.95-.69l1.286-3.97z" />
                  </svg>
                </template>
              </div>
              <span class="ml-2 text-sm text-gray-600">({{ product.reviewCount }} reviews)</span>
            </div>
            <div class="flex items-center text-sm text-gray-600">
              <svg class="w-5 h-5 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
              </svg>
              {{ product.soldCount }} sold
            </div>
          </div>
          
          <p class="text-gray-700 mb-6">{{ product.description }}</p>
        </div>
        <div>
          <!-- Quantity Selector -->
          <div class="flex items-center mb-4">
            <label class="text-sm font-medium text-gray-700 mr-4">Quantity:</label>
            <div class="flex items-center border border-gray-300 rounded-lg">
              <button
                @click="decreaseQuantity"
                :disabled="quantity <= 1"
                class="px-3 py-1 text-gray-600 hover:text-gray-700 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                -
              </button>
              <input
                v-model.number="quantity"
                type="number"
                min="1"
                :max="product.stock"
                class="w-16 text-center border-x border-gray-300 py-1 focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
              <button
                @click="increaseQuantity"
                :disabled="quantity >= product.stock"
                class="px-3 py-1 text-gray-600 hover:text-gray-700 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                +
              </button>
            </div>
            <span class="ml-2 text-sm text-gray-500">Available: {{ product.stock }}</span>
          </div>

          <button
            @click="addToCart"
            class="w-full bg-blue-600 text-white px-8 py-3 rounded-lg hover:bg-blue-700 transition-colors disabled:bg-gray-400 disabled:cursor-not-allowed flex items-center justify-center gap-2"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
              <path d="M3 3h2l.4 2M7 13h10l4-8H5.4" stroke-linecap="round" stroke-linejoin="round"/>
              <circle cx="9" cy="21" r="1" />
              <circle cx="20" cy="21" r="1" />
            </svg>
            Add to Cart
          </button>
        </div>
      </div>
    </div>

    <!-- Tabs Section -->
    <div class="mt-10 bg-white rounded-lg shadow p-6">
      <div class="flex border-b border-gray-200 mb-6">
        <button
          v-for="tab in tabs"
          :key="tab"
          @click="activeTab = tab"
          :class="['px-6 py-2 -mb-px font-semibold focus:outline-none', activeTab === tab ? 'border-b-2 border-blue-600 text-blue-700' : 'text-gray-500 hover:text-blue-600']"
        >
          {{ tab }}
        </button>
      </div>
      <!-- Tab Panels -->
      <div v-if="activeTab === 'Description'">
        <h2 class="text-xl font-bold mb-2">Product Description</h2>
        <p class="text-gray-700">{{ product.description }}</p>
      </div>
      <div v-else-if="activeTab === 'Related Products'">
        <h2 class="text-xl font-bold mb-4">Related Products</h2>
        <div v-if="relatedProducts.length" class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-6">
          <div
            v-for="relatedProduct in relatedProducts"
            :key="relatedProduct.id"
            class="bg-white rounded-lg shadow-md overflow-hidden hover:shadow-lg transition-shadow duration-300 flex flex-col h-full"
          >
            <router-link :to="`/products/${relatedProduct.id}`" class="block flex flex-col h-full">
              <div class="relative pb-[75%]">
                <img
                  :src="relatedProduct.imageUrl || 'https://via.placeholder.com/300'"
                  :alt="relatedProduct.name"
                  class="absolute top-0 left-0 w-full h-full object-cover"
                />
              </div>
              <div class="p-4 flex flex-col flex-grow">
                <h3 class="text-lg font-semibold text-gray-800 mb-2 line-clamp-2">{{ relatedProduct.name }}</h3>
                <p class="text-blue-600 font-bold mb-2">${{ relatedProduct.price }}</p>
                <div class="flex items-center text-sm text-gray-500 mt-auto">
                  <div class="flex items-center mr-2">
                    <svg v-for="i in 5" :key="i" class="w-4 h-4" :class="i <= relatedProduct.rating ? 'text-yellow-400' : 'text-gray-300'" fill="currentColor" viewBox="0 0 20 20">
                      <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.286 3.97a1 1 0 00.95.69h4.162c.969 0 1.371 1.24.588 1.81l-3.357 2.44a1 1 0 00-.364 1.118l1.287 3.97c.3.921-.755 1.688-1.54 1.118l-3.357-2.44a1 1 0 00-1.175 0l-3.357 2.44c-.784.57-1.838-.197-1.54-1.118l1.287-3.97a1 1 0 00-.364-1.118L2.73 9.397c-.783-.57-.38-1.81.588-1.81h4.162a1 1 0 00.95-.69l1.286-3.97z" />
                    </svg>
                  </div>
                  <span>({{ relatedProduct.reviewCount }})</span>
                </div>
              </div>
            </router-link>
          </div>
        </div>
        <div v-else class="text-gray-500">No related products found.</div>
      </div>
    </div>

    <!-- Add Rating Button -->
    <div v-if="isLoadingReviewStatus" class="mt-6 bg-white rounded-lg shadow p-6 text-sm text-gray-600">
      Loading review status...
    </div>
    <div v-else class="mt-6 bg-white rounded-lg shadow p-6">
      <div class="flex justify-between items-center">
        <h2 class="text-xl font-bold">Your Review</h2>
        <button 
          v-if="canReview"
          @click="openRatingModal"
          :disabled="!$store.getters['user/isAuthenticated']"
          class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed">
          {{ hasUserReview ? 'Edit Your Review' : 'Write a Review' }}
        </button>
      </div>
      <p v-if="!$store.getters['user/isAuthenticated']" class="mt-2 text-sm text-red-500">
        Please log in to leave a review.
      </p>
      <p v-else-if="!canReview" class="mt-2 text-sm text-amber-600">
        You can write a review after purchasing and receiving this product.
      </p>
      <div v-else-if="hasUserReview && userReview" class="mt-4 bg-gray-50 rounded-lg p-4">
        <div class="flex items-center mb-2">
          <div class="flex items-center">
            <template v-for="i in 5">
              <svg v-if="i <= userReview.rating" :key="'user-full-' + i" class="w-5 h-5 text-yellow-400" fill="currentColor" viewBox="0 0 20 20">
                <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.286 3.97a1 1 0 00.95.69h4.162c.969 0 1.371 1.24.588 1.81l-3.357 2.44a1 1 0 00-.364 1.118l1.287 3.97c.3.921-.755 1.688-1.54 1.118l-3.357-2.44a1 1 0 00-1.175 0l-3.357 2.44c-.784.57-1.838-.197-1.54-1.118l1.287-3.97a1 1 0 00-.364-1.118L2.73 9.397c-.783-.57-.38-1.81.588-1.81h4.162a1 1 0 00.95-.69l1.286-3.97z" />
              </svg>
              <svg v-else :key="'user-empty-' + i" class="w-5 h-5 text-gray-300" fill="currentColor" viewBox="0 0 20 20">
                <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.286 3.97a1 1 0 00.95.69h4.162c.969 0 1.371 1.24.588 1.81l-3.357 2.44a1 1 0 00-.364 1.118l1.287 3.97c.3.921-.755 1.688-1.54 1.118l-3.357-2.44a1 1 0 00-1.175 0l-3.357 2.44c-.784.57-1.838-.197-1.54-1.118l1.287-3.97a1 1 0 00-.364-1.118L2.73 9.397c-.783-.57-.38-1.81.588-1.81h4.162a1 1 0 00.95-.69l1.286-3.97z" />
              </svg>
            </template>
          </div>
          <span class="ml-2 text-xs text-gray-500">{{ new Date(userReview.createdAt).toLocaleDateString() }}</span>
        </div>
        <p class="text-gray-700">{{ userReview.comment }}</p>
      </div>
      <p v-else-if="canReview" class="mt-2 text-gray-600">You haven't reviewed this product yet.</p>
    </div>

    <!-- Rating Modal -->
    <div v-if="showRatingModal" class="fixed inset-0 z-50 overflow-y-auto">
      <div class="flex items-center justify-center min-h-screen px-4">
        <div class="fixed inset-0 bg-black bg-opacity-50 transition-opacity" @click="closeRatingModal"></div>
        
        <div class="relative bg-white rounded-lg max-w-md w-full mx-auto shadow-xl p-6 z-10">
          <div class="flex justify-between items-center mb-4">
            <h3 class="text-xl font-bold text-gray-900">Rate this product</h3>
            <button @click="closeRatingModal" class="text-gray-500 hover:text-gray-700">
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
          <div class="mb-6">
            <p class="text-sm text-gray-600 mb-2">Select your rating:</p>
            <div class="flex items-center justify-center space-x-1">
              <button 
                v-for="star in 5" 
                :key="star" 
                @click="userRating = star"
                class="focus:outline-none">
                <svg 
                  class="w-8 h-8" 
                  :class="star <= userRating ? 'text-yellow-400' : 'text-gray-300'" 
                  fill="currentColor" 
                  viewBox="0 0 20 20">
                  <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.286 3.97a1 1 0 00.95.69h4.162c.969 0 1.371 1.24.588 1.81l-3.357 2.44a1 1 0 00-.364 1.118l1.287 3.97c.3.921-.755 1.688-1.54 1.118l-3.357-2.44a1 1 0 00-1.175 0l-3.357 2.44c-.784.57-1.838-.197-1.54-1.118l1.287-3.97a1 1 0 00-.364-1.118L2.73 9.397c-.783-.57-.38-1.81.588-1.81h4.162a1 1 0 00.95-.69l1.286-3.97z" />
                </svg>
              </button>
            </div>
            <p class="text-sm mt-2 text-gray-600">
              {{ userRating ? ratingLabels[userRating - 1] : 'Select a rating' }}
            </p>
          </div>
          
          <div class="mb-6">
            <label for="review-comment" class="block text-sm font-medium text-gray-700 mb-2">
              Your review (optional)
            </label>
            <textarea
              id="review-comment"
              v-model="reviewComment"
              rows="4"
              class="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
              placeholder="Share your experience with this product"
            ></textarea>
          </div>
          
          <div class="flex justify-end">
            <button 
              @click="closeRatingModal" 
              class="mr-3 px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50">
              Cancel
            </button>
            <button 
              @click="submitRating" 
              :disabled="!userRating || isSubmitting"
              class="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed">
              {{ isSubmitting ? 'Submitting...' : 'Submit' }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Add ProductReview component -->
    <ProductReview :product-id="product.id" />
  </div>
  <div v-else class="flex justify-center items-center h-64">
    <span class="loader"></span>
  </div>
</template>

<script>
import api from '../services/api';
import formatPrice from '../utils/formatPrice';
import ProductReview from '@/components/ProductReview.vue';

export default {
  name: 'ProductDetailPage',
  components: {
    ProductReview
  },
  data() {
    return {
      product: null,
      relatedProducts: [],
      reviews: [],
      averageRating: 0,
      tabs: ['Description', 'Related Products'],
      activeTab: 'Description',
      quantity: 1,
      showRatingModal: false,
      userRating: 0,
      reviewComment: '',
      isSubmitting: false,
      ratingLabels: ['Poor', 'Fair', 'Good', 'Very Good', 'Excellent'],
      canReview: false,
      userReview: null,
      hasUserReview: false,
      isLoadingReviewStatus: true // Add loading state
    };
  },
  async created() {
    try {
      // Fetch product details
      const response = await api.get(`/Product/get/${this.$route.params.id}`);
      this.product = response.data;

      // Check if route has query parameter to open rating modal
      if (this.$route.query.openRatingModal) {
        this.openRatingModal();
      }

      // Fetch related products
      if (this.product?.categoryId) {
        try {
          const relatedRes = await api.get(`/Product/list`, {
            params: { categoryId: this.product.categoryId }
          });
          console.log('Related products response:', relatedRes.data);

          const products = Array.isArray(relatedRes.data) ? relatedRes.data : [];
          this.relatedProducts = products
            .filter(p => p.id !== this.product.id)
            .slice(0, 6);
          console.log('Filtered related products:', this.relatedProducts);
        } catch (error) {
          console.error('Error fetching related products:', error);
          this.relatedProducts = [];
        }
      }

      // Fetch reviews
      const reviewsRes = await api.get(`/Review/list/${this.$route.params.id}`);
      console.log('Reviews response:', reviewsRes.data);
      
      // Handle both array response and paginated response
      if (Array.isArray(reviewsRes.data)) {
        this.reviews = reviewsRes.data;
      } else if (reviewsRes.data && (reviewsRes.data.reviews || reviewsRes.data.Reviews)) {
        this.reviews = reviewsRes.data.reviews || reviewsRes.data.Reviews;
      } else {
        this.reviews = [];
      }
      
      this.averageRating = this.reviews.length
        ? this.reviews.reduce((sum, r) => sum + (r.rating || 0), 0) / this.reviews.length
        : 0;

      // Check if user can review this product
      await this.initReviewStatus();
    } catch (error) {
      console.error('Error fetching product details:', error);
    } finally {
      this.isLoadingReviewStatus = false;
    }
  },
  watch: {
    '$store.getters["user/isAuthenticated"]': async function(isAuthenticated) {
      if (isAuthenticated && this.product) {
        await this.initReviewStatus();
      } else {
        this.canReview = false;
        this.hasUserReview = false;
        this.userReview = null;
        this.isLoadingReviewStatus = false;
      }
    }
  },
  methods: {
    formatPrice,
    increaseQuantity() {
      if (this.quantity < this.product.stock) {
        this.quantity++;
      }
    },
    decreaseQuantity() {
      if (this.quantity > 1) {
        this.quantity--;
      }
    },
    async addToCart() {
      try {
        await this.$store.dispatch('cart/addToCart', { 
          productId: this.product.id, 
          quantity: this.quantity 
        });
      } catch (error) {
        console.error('Error adding to cart:', error);
        this.$store.dispatch('notification/showNotification', {
          type: 'error',
          message: 'Failed to add product to cart'
        }, { root: true });
      }
    },
    openRatingModal() {
      if (!this.$store.getters['user/isAuthenticated']) {
        this.$store.dispatch('notification/showNotification', {
          type: 'error',
          message: 'Please log in to leave a review'
        });
        return;
      }

      if (this.hasUserReview && this.userReview) {
        this.userRating = this.userReview.rating;
        this.reviewComment = this.userReview.comment;
      } else {
        this.userRating = 0;
        this.reviewComment = '';
      }

      this.showRatingModal = true;
    },
    closeRatingModal() {
      this.showRatingModal = false;
    },
    async checkCanReview(productId) {
      if (!productId) {
        console.log('No productId provided');
        return { canReview: false, hasReviewed: false, userReview: null };
      }

      try {
        const token = this.$store.state.user.token;
        console.log('Token:', token ? 'Present' : 'Missing');
        if (!token) {
          console.log('User not authenticated');
          return { canReview: false, hasReviewed: false, userReview: null };
        }

        // Kiểm tra xem user có thể đánh giá không
        const canReviewResponse = await api.get(`/Review/can-review/${productId}`, {
          headers: { Authorization: `Bearer ${token}` }
        });
        console.log('CanReview response:', canReviewResponse.data);
        const canReview = canReviewResponse.data.CanReview || canReviewResponse.data.canReview;

        let hasReviewed = false;
        let userReview = null;

        if (canReview) {
          try {
            // Kiểm tra xem user đã có review chưa
            const userReviewResponse = await api.get(`/Review/user-review/${productId}`, {
              headers: { Authorization: `Bearer ${token}` }
            });
            console.log('UserReview response:', userReviewResponse.data);
            userReview = userReviewResponse.data;
            hasReviewed = true;
          } catch {
            console.log('User has not reviewed this product yet');
            hasReviewed = false;
            userReview = null;
          }
        }

        return { canReview, hasReviewed, userReview };
      } catch (error) {
        console.error('Error checking review eligibility:', error);
        if (error.response) {
          console.error('Error response:', error.response.data, error.response.status);
        }
        return { canReview: false, hasReviewed: false, userReview: null };
      }
    },
    async initReviewStatus() {
      this.isLoadingReviewStatus = true;
      try {
        if (this.$store.getters['user/isAuthenticated'] && this.product) {
          const reviewStatus = await this.checkCanReview(this.product.id);
          console.log('Review status:', reviewStatus);
          this.canReview = reviewStatus.canReview;
          this.hasUserReview = reviewStatus.hasReviewed;
          this.userReview = reviewStatus.userReview;
        } else {
          this.canReview = false;
          this.hasUserReview = false;
          this.userReview = null;
        }
      } catch (error) {
        console.error('Error initializing review status:', error);
        this.canReview = false;
        this.hasUserReview = false;
        this.userReview = null;
      } finally {
        this.isLoadingReviewStatus = false;
      }
    },
    async submitRating() {
      if (!this.userRating) return;

      this.isSubmitting = true;

      try {
        const reviewData = {
          productId: this.product.id,
          rating: this.userRating,
          comment: this.reviewComment
        };

        if (this.hasUserReview && this.userReview) {
          await api.put(`/Review/update-review/${this.userReview.id}`, {
            rating: this.userRating,
            comment: this.reviewComment
          }, {
            headers: { Authorization: `Bearer ${this.$store.state.user.token}` }
          });
        } else {
          await api.post('/Review/submit-review', reviewData, {
            headers: { Authorization: `Bearer ${this.$store.state.user.token}` }
          });
        }

        // Refresh reviews
        const reviewsRes = await api.get(`/Review/list/${this.$route.params.id}`);
        
        // Handle both array response and paginated response
        if (Array.isArray(reviewsRes.data)) {
          this.reviews = reviewsRes.data;
        } else if (reviewsRes.data && (reviewsRes.data.reviews || reviewsRes.data.Reviews)) {
          this.reviews = reviewsRes.data.reviews || reviewsRes.data.Reviews;
        } else {
          this.reviews = [];
        }
        
        this.averageRating = this.reviews.length
          ? this.reviews.reduce((sum, r) => sum + (r.rating || 0), 0) / this.reviews.length
          : 0;

        // Refresh review status
        await this.initReviewStatus();

        this.$store.dispatch('notification/showNotification', {
          type: 'success',
          message: this.hasUserReview ? 'Review updated successfully' : 'Review submitted successfully'
        });

        this.closeRatingModal();
      } catch (error) {
        console.error('Error submitting review:', error);
        this.$store.dispatch('notification/showNotification', {
          type: 'error',
          message: error.response?.data?.message || 'Failed to submit review'
        });
      } finally {
        this.isSubmitting = false;
      }
    }
  }
};
</script>

<style>
.loader {
  border: 4px solid #f3f3f3;
  border-top: 4px solid #3490dc;
  border-radius: 50%;
  width: 36px;
  height: 36px;
  animation: spin 1s linear infinite;
}
@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}
</style>