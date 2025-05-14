<template>
  <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-purple-50">
    <div class="bg-white p-8 rounded-xl shadow-xl w-full max-w-md transform transition-all duration-300">
      <div class="text-center mb-8">
        <h1 class="text-3xl font-bold text-gray-800 mb-2">Xác thực Email</h1>
        <p class="text-gray-600">Vui lòng đợi trong giây lát...</p>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="flex justify-center items-center py-8">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
      </div>

      <!-- Success Message -->
      <div v-else-if="success" class="text-center py-8">
        <div class="mb-4">
          <svg class="mx-auto h-16 w-16 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
          </svg>
        </div>
        <h2 class="text-xl font-semibold text-gray-800 mb-2">Xác thực thành công!</h2>
        <p class="text-gray-600 mb-6">Tài khoản của bạn đã được xác thực. Bạn có thể đăng nhập ngay bây giờ.</p>
        <router-link 
          to="/login" 
          class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
        >
          Đăng nhập
        </router-link>
      </div>

      <!-- Error Message -->
      <div v-else-if="error" class="text-center py-8">
        <div class="mb-4">
          <svg class="mx-auto h-16 w-16 text-red-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
          </svg>
        </div>
        <h2 class="text-xl font-semibold text-gray-800 mb-2">Xác thực thất bại</h2>
        <p class="text-gray-600 mb-6">{{ error }}</p>
        <div class="space-y-4">
          <button 
            @click="retryConfirmation" 
            class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
          >
            Thử lại
          </button>
          <div>
            <router-link 
              to="/login" 
              class="text-sm text-blue-600 hover:text-blue-500"
            >
              Quay lại trang đăng nhập
            </router-link>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import api from '../services/api';
import { mapActions } from 'vuex';

export default {
  name: 'ConfirmEmail',
  data() {
    return {
      loading: true,
      success: false,
      error: null
    };
  },
  methods: {
    ...mapActions('notification', ['showNotification']),
    async confirmEmail() {
      try {
        const userId = this.$route.query.userId;
        const token = this.$route.query.token;

        if (!userId || !token) {
          throw new Error('Thông tin xác thực không hợp lệ');
        }

        console.log('Confirming email with:', { 
          userId, 
          token,
          url: '/Auth/confirm-email'
        });

        const response = await api.post('/Auth/confirm-email', {
          userId,
          token
        });

        console.log('Confirmation response:', response);

        // Check if the response indicates success
        if (response.status === 200 && response.data && response.data.success) {
          this.success = true;
          this.showNotification({
            type: 'success',
            message: 'Xác thực email thành công! Bạn có thể đăng nhập ngay bây giờ.'
          });
          
          // Redirect to login page after a short delay
          setTimeout(() => {
            this.$router.push('/login');
          }, 2000);
        } else {
          throw new Error('Xác thực không thành công. Vui lòng thử lại.');
        }
      } catch (error) {
        console.error('Email confirmation error:', error);
        console.error('Error details:', {
          message: error.message,
          response: error.response?.data,
          status: error.response?.status,
          headers: error.response?.headers,
          config: error.config
        });
        
        let errorMessage = 'Xác thực thất bại. Vui lòng thử lại sau.';
        
        if (error.response) {
          const responseData = error.response.data;
          console.log('Response data:', responseData);
          
          if (typeof responseData === 'string') {
            errorMessage = responseData;
          } else if (responseData?.message) {
            errorMessage = responseData.message;
          } else if (responseData?.error) {
            errorMessage = responseData.error;
          } else if (responseData?.errors) {
            errorMessage = responseData.errors.join(', ');
          } else {
            switch (error.response.status) {
              case 400:
                errorMessage = 'Link xác thực không hợp lệ hoặc đã hết hạn';
                break;
              case 404:
                errorMessage = 'Không tìm thấy tài khoản';
                break;
              case 500:
                errorMessage = 'Lỗi máy chủ. Vui lòng thử lại sau.';
                break;
            }
          }
        } else if (error.message === 'Network Error') {
          errorMessage = 'Không thể kết nối đến máy chủ. Vui lòng kiểm tra kết nối internet của bạn.';
        }

        this.error = errorMessage;
        this.showNotification({
          type: 'error',
          message: errorMessage
        });
      } finally {
        this.loading = false;
      }
    },
    retryConfirmation() {
      this.loading = true;
      this.error = null;
      this.confirmEmail();
    }
  },
  mounted() {
    this.confirmEmail();
  }
};
</script>

<style scoped>
.bg-gradient-to-br {
  background-image: linear-gradient(to bottom right, var(--tw-gradient-stops));
}
</style> 