<template>
  <div class="product-review">
    <div class="review-summary">
      <div class="rating-overview">
        <div class="average-rating">
          <span class="rating-number">{{ averageRating.toFixed(1) }}</span>
          <div class="stars">
            <i v-for="n in 5" :key="n" class="fas fa-star" :class="{ 'active': n <= Math.round(averageRating) }"></i>
          </div>
          <span class="review-count">{{ totalReviews }} reviews</span>
        </div>
        <div class="rating-distribution">
          <div v-for="n in 5" :key="n" class="rating-bar">
            <span class="star-label">{{ n }} stars</span>
            <div class="progress">
              <div class="progress-bar" :style="{ width: getRatingPercentage(n) + '%' }"></div>
            </div>
            <span class="count">{{ getRatingCount(n) }}</span>
          </div>
        </div>
      </div>
    </div>

    <div v-if="isAuthenticated && hasPurchased && !userReview" class="add-review">
      <h3>Write a Review</h3>
      <form @submit.prevent="submitReview">
        <div class="rating-input">
          <span>Your Rating:</span>
          <div class="stars-input">
            <i v-for="n in 5" :key="n" 
               class="fas fa-star" 
               :class="{ 'active': n <= newReview.rating }"
               @click="newReview.rating = n"
               @mouseover="hoverRating = n"
               @mouseleave="hoverRating = 0"></i>
          </div>
        </div>
        <div class="form-group">
          <label for="comment">Your Review:</label>
          <textarea id="comment" 
                    v-model="newReview.comment" 
                    :class="{ 'is-invalid': errors.comment }"
                    placeholder="Write your review here (minimum 10 characters)"
                    rows="4"></textarea>
          <div class="invalid-feedback" v-if="errors.comment">{{ errors.comment }}</div>
        </div>
        <button type="submit" class="btn btn-primary" :disabled="isSubmitting">
          {{ isSubmitting ? 'Submitting...' : 'Submit Review' }}
        </button>
      </form>
    </div>

    <div v-if="userReview" class="user-review">
      <h3>Your Review</h3>
      <div class="review-card">
        <div class="review-header">
          <div class="stars">
            <i v-for="n in 5" :key="n" class="fas fa-star" :class="{ 'active': n <= userReview.rating }"></i>
          </div>
          <span class="review-date">{{ formatDate(userReview.createdAt) }}</span>
        </div>
        <p class="review-comment">{{ userReview.comment }}</p>
        <div class="review-actions">
          <button class="btn btn-sm btn-outline-primary" @click="editReview">Edit</button>
          <button class="btn btn-sm btn-outline-danger" @click="deleteReview">Delete</button>
        </div>
      </div>
    </div>

    <div class="reviews-list">
      <h3>Customer Reviews</h3>
      <div v-if="reviews.length === 0" class="no-reviews">
        No reviews yet. Be the first to review this product!
      </div>
      <div v-else>
        <div v-for="review in reviews" :key="review.id" class="review-card">
          <div class="review-header">
            <div class="stars">
              <i v-for="n in 5" :key="n" class="fas fa-star" :class="{ 'active': n <= review.rating }"></i>
            </div>
            <span class="review-date">{{ formatDate(review.createdAt) }}</span>
          </div>
          <p class="reviewer-name">{{ review.userName }}</p>
          <p class="review-comment">{{ review.comment }}</p>
          
          <div v-if="isCurrentUserReview(review)" class="review-actions">
            <button class="btn btn-sm btn-outline-primary" @click="editUserReview(review)">Edit</button>
            <button class="btn btn-sm btn-outline-danger" @click="deleteUserReview(review.id)">Delete</button>
          </div>
        </div>
        
        <div class="pagination" v-if="totalPages > 1">
          <button class="btn btn-outline-primary" 
                  :disabled="currentPage === 1"
                  @click="changePage(currentPage - 1)">
            Previous
          </button>
          <span class="page-info">Page {{ currentPage }} of {{ totalPages }}</span>
          <button class="btn btn-outline-primary" 
                  :disabled="currentPage === totalPages"
                  @click="changePage(currentPage + 1)">
            Next
          </button>
        </div>
      </div>
    </div>

    <!-- Edit Review Modal -->
    <div class="modal" v-if="showEditModal">
      <div class="modal-content">
        <h3>Edit Your Review</h3>
        <form @submit.prevent="updateReview">
          <div class="rating-input">
            <span>Your Rating:</span>
            <div class="stars-input">
              <i v-for="n in 5" :key="n" 
                 class="fas fa-star" 
                 :class="{ 'active': n <= editingReview.rating }"
                 @click="editingReview.rating = n"></i>
            </div>
          </div>
          <div class="form-group">
            <label for="edit-comment">Your Review:</label>
            <textarea id="edit-comment" 
                      v-model="editingReview.comment" 
                      :class="{ 'is-invalid': errors.comment }"
                      rows="4"></textarea>
            <div class="invalid-feedback" v-if="errors.comment">{{ errors.comment }}</div>
          </div>
          <div class="modal-actions">
            <button type="button" class="btn btn-secondary" @click="showEditModal = false">Cancel</button>
            <button type="submit" class="btn btn-primary" :disabled="isSubmitting">
              {{ isSubmitting ? 'Updating...' : 'Update Review' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, computed, onMounted } from 'vue'
import { useStore } from 'vuex'
import axios from 'axios'

export default {
  name: 'ProductReview',
  props: {
    productId: {
      type: Number,
      required: true
    }
  },
  setup(props) {
    const store = useStore()
    const reviews = ref([])
    const currentPage = ref(1)
    const totalPages = ref(1)
    const totalReviews = ref(0)
    const isSubmitting = ref(false)
    const showEditModal = ref(false)
    const errors = ref({})
    const hoverRating = ref(0)

    const newReview = ref({
      rating: 0,
      comment: ''
    })

    const editingReview = ref({
      id: null,
      rating: 0,
      comment: ''
    })

    const userReview = computed(() => {
      return reviews.value.find(review => review.userName === store.state.auth.user?.userName)
    })

    const averageRating = computed(() => {
      if (reviews.value.length === 0) return 0
      const sum = reviews.value.reduce((acc, review) => acc + review.rating, 0)
      return sum / reviews.value.length
    })

    const isAuthenticated = computed(() => store.state.auth.isAuthenticated)
    const hasPurchased = computed(() => store.state.auth.user?.hasPurchasedProducts?.includes(props.productId))

    const fetchReviews = async (page = 1) => {
      try {
        const response = await axios.get(`/api/review/product/${props.productId}?page=${page}`)
        reviews.value = response.data.reviews
        totalPages.value = response.data.totalPages
        totalReviews.value = response.data.totalReviews
        currentPage.value = page
      } catch (error) {
        console.error('Error fetching reviews:', error)
      }
    }

    const submitReview = async () => {
      errors.value = {}
      if (!validateReview(newReview.value)) return

      isSubmitting.value = true
      try {
        await axios.post('/api/review', {
          productId: props.productId,
          rating: newReview.value.rating,
          comment: newReview.value.comment
        })
        await fetchReviews(currentPage.value)
        newReview.value = { rating: 0, comment: '' }
      } catch (error) {
        if (error.response?.data?.errors) {
          errors.value = error.response.data.errors
        }
        console.error('Error submitting review:', error)
      } finally {
        isSubmitting.value = false
      }
    }

    const updateReview = async () => {
      errors.value = {}
      if (!validateReview(editingReview.value)) return

      isSubmitting.value = true
      try {
        await axios.put(`/api/review/${editingReview.value.id}`, {
          rating: editingReview.value.rating,
          comment: editingReview.value.comment
        })
        await fetchReviews(currentPage.value)
        showEditModal.value = false
      } catch (error) {
        if (error.response?.data?.errors) {
          errors.value = error.response.data.errors
        }
        console.error('Error updating review:', error)
      } finally {
        isSubmitting.value = false
      }
    }

    const deleteReview = async (reviewId) => {
      if (!confirm('Are you sure you want to delete your review?')) return

      try {
        await axios.delete(`/api/review/${reviewId}`)
        await fetchReviews(currentPage.value)
      } catch (error) {
        console.error('Error deleting review:', error)
      }
    }

    const validateReview = (review) => {
      errors.value = {}
      if (review.rating < 1 || review.rating > 5) {
        errors.value.rating = 'Please select a rating between 1 and 5 stars'
      }
      if (!review.comment || review.comment.length < 10) {
        errors.value.comment = 'Review must be at least 10 characters long'
      }
      return Object.keys(errors.value).length === 0
    }

    const changePage = (page) => {
      fetchReviews(page)
    }

    const formatDate = (date) => {
      return new Date(date).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
      })
    }

    const getRatingCount = (rating) => {
      return reviews.value.filter(review => review.rating === rating).length
    }

    const getRatingPercentage = (rating) => {
      if (totalReviews.value === 0) return 0
      return (getRatingCount(rating) / totalReviews.value) * 100
    }

    const isCurrentUserReview = (review) => {
      return userReview.value && review.userName === userReview.value.userName
    }

    const editReview = (review) => {
      editingReview.value = {
        id: review.id,
        rating: review.rating,
        comment: review.comment
      }
      showEditModal.value = true
    }

    const editUserReview = (review) => {
      editingReview.value = {
        id: review.id,
        rating: review.rating,
        comment: review.comment
      }
      showEditModal.value = true
    }

    const deleteUserReview = async (reviewId) => {
      if (!confirm('Are you sure you want to delete your review?')) return

      try {
        await axios.delete(`/api/review/${reviewId}`)
        await fetchReviews(currentPage.value)
      } catch (error) {
        console.error('Error deleting review:', error)
      }
    }

    onMounted(() => {
      fetchReviews()
    })

    return {
      reviews,
      currentPage,
      totalPages,
      totalReviews,
      averageRating,
      isSubmitting,
      showEditModal,
      errors,
      hoverRating,
      newReview,
      editingReview,
      userReview,
      isAuthenticated,
      hasPurchased,
      submitReview,
      updateReview,
      deleteReview,
      changePage,
      formatDate,
      getRatingCount,
      getRatingPercentage,
      isCurrentUserReview,
      editReview,
      editUserReview,
      deleteUserReview
    }
  }
}
</script>

<style scoped>
.product-review {
  margin: 2rem 0;
}

.review-summary {
  margin-bottom: 2rem;
}

.rating-overview {
  display: flex;
  gap: 2rem;
}

.average-rating {
  text-align: center;
}

.rating-number {
  font-size: 3rem;
  font-weight: bold;
}

.stars {
  color: #ddd;
  margin: 0.5rem 0;
}

.stars .fa-star.active {
  color: #ffc107;
}

.review-count {
  color: #666;
}

.rating-distribution {
  flex: 1;
}

.rating-bar {
  display: flex;
  align-items: center;
  margin: 0.5rem 0;
}

.star-label {
  width: 60px;
}

.progress {
  flex: 1;
  height: 8px;
  background-color: #eee;
  margin: 0 1rem;
  border-radius: 4px;
  overflow: hidden;
}

.progress-bar {
  height: 100%;
  background-color: #ffc107;
}

.count {
  width: 40px;
  text-align: right;
  color: #666;
}

.add-review {
  margin: 2rem 0;
  padding: 1rem;
  border: 1px solid #ddd;
  border-radius: 8px;
}

.rating-input {
  margin-bottom: 1rem;
}

.stars-input {
  display: inline-block;
  margin-left: 1rem;
  cursor: pointer;
}

.stars-input .fa-star {
  margin-right: 0.25rem;
}

.form-group {
  margin-bottom: 1rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
}

.form-group textarea {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
}

.form-group textarea.is-invalid {
  border-color: #dc3545;
}

.invalid-feedback {
  color: #dc3545;
  font-size: 0.875rem;
  margin-top: 0.25rem;
}

.review-card {
  margin: 1rem 0;
  padding: 1rem;
  border: 1px solid #ddd;
  border-radius: 8px;
}

.review-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.5rem;
}

.review-date {
  color: #666;
  font-size: 0.875rem;
}

.reviewer-name {
  font-weight: bold;
  margin-bottom: 0.5rem;
}

.review-comment {
  margin: 0;
  white-space: pre-wrap;
}

.review-actions {
  margin-top: 1rem;
  display: flex;
  gap: 0.5rem;
}

.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  margin-top: 2rem;
  gap: 1rem;
}

.page-info {
  color: #666;
}

.modal {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
}

.modal-content {
  background-color: white;
  padding: 2rem;
  border-radius: 8px;
  width: 100%;
  max-width: 500px;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  margin-top: 1rem;
}

.no-reviews {
  text-align: center;
  color: #666;
  padding: 2rem;
  border: 1px dashed #ddd;
  border-radius: 8px;
}
</style> 