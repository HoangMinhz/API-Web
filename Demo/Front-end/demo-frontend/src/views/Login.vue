<template>
  <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-purple-50">
    <div class="bg-white p-8 rounded-xl shadow-xl w-full max-w-md transform transition-all duration-300 hover:shadow-2xl">
      <div class="text-center mb-8">
        <h1 class="text-3xl font-bold text-gray-800 mb-2">Welcome Back</h1>
        <p class="text-gray-600">Sign in to your account</p>
      </div>

      <!-- Error Message -->
      <div v-if="error" class="bg-red-50 border-l-4 border-red-500 p-4 mb-6 rounded-md">
        <div class="flex items-center">
          <svg class="h-5 w-5 text-red-500 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <p class="text-red-700">{{ error }}</p>
        </div>
      </div>

      <!-- Login Form -->
      <form @submit.prevent="handleLogin" class="space-y-6">
        <!-- Email Field -->
        <div class="space-y-2">
          <label for="email" class="block text-sm font-medium text-gray-700">Email</label>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <svg class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
              </svg>
            </div>
            <input
              v-model="email"
              id="email"
              type="email"
              class="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors duration-200"
              placeholder="Enter your email"
              required
              autocomplete="email"
            />
          </div>
        </div>

        <!-- Password Field -->
        <div class="space-y-2">
          <label for="password" class="block text-sm font-medium text-gray-700">Password</label>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <svg class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
              </svg>
            </div>
            <input
              v-model="password"
              id="password"
              :type="showPassword ? 'text' : 'password'"
              class="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors duration-200"
              placeholder="Enter your password"
              required
              autocomplete="current-password"
            />
            <button
              type="button"
              @click="showPassword = !showPassword"
              class="absolute inset-y-0 right-0 pr-3 flex items-center"
            >
              <svg class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path v-if="showPassword" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 5.411m0 0L21 21" />
                <path v-else stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
              </svg>
            </button>
          </div>
        </div>

        <!-- Remember Me & Forgot Password -->
        <div class="flex items-center justify-between">
          <div class="flex items-center">
            <input
              v-model="rememberMe"
              id="remember-me"
              type="checkbox"
              class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
            />
            <label for="remember-me" class="ml-2 block text-sm text-gray-700">Remember me</label>
          </div>
          <a href="#" class="text-sm text-blue-600 hover:text-blue-500">Forgot password?</a>
        </div>

        <!-- Login Button -->
        <button
          type="submit"
          class="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-all duration-200"
          :disabled="loading"
        >
          <span v-if="loading" class="flex items-center">
            <svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-white" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
            Signing in...
          </span>
          <span v-else>Sign in</span>
        </button>
      </form>

      <!-- Social Login Divider -->
      <div class="mt-6">
        <div class="relative">
          <div class="absolute inset-0 flex items-center">
            <div class="w-full border-t border-gray-300"></div>
          </div>
          <div class="relative flex justify-center text-sm">
            <span class="px-2 bg-white text-gray-500">Or continue with</span>
          </div>
        </div>

        <!-- Social Login Buttons -->
        <div class="mt-6 grid grid-cols-2 gap-3">
          <button
            @click="handleGoogleLogin"
            class="w-full inline-flex justify-center py-2 px-4 border border-gray-300 rounded-md shadow-sm bg-white text-sm font-medium text-gray-500 hover:bg-gray-50"
          >
            <svg class="w-5 h-5" aria-hidden="true" fill="currentColor" viewBox="0 0 24 24">
              <path d="M12.48 10.92v3.28h7.84c-.24 1.84-.853 3.187-1.787 4.133-1.147 1.147-2.933 2.4-6.053 2.4-4.827 0-8.6-3.893-8.6-8.72s3.773-8.72 8.6-8.72c2.6 0 4.507 1.027 5.907 2.347l2.307-2.307C18.747 1.44 16.133 0 12.48 0 5.867 0 .307 5.387.307 12s5.56 12 12.173 12c3.573 0 6.267-1.173 8.373-3.36 2.16-2.16 2.84-5.213 2.84-7.667 0-.76-.053-1.467-.173-2.053H12.48z"/>
            </svg>
            <span class="ml-2">Google</span>
          </button>
          <button
            @click="handleFacebookLogin"
            class="w-full inline-flex justify-center py-2 px-4 border border-gray-300 rounded-md shadow-sm bg-white text-sm font-medium text-gray-500 hover:bg-gray-50"
          >
            <svg class="w-5 h-5" aria-hidden="true" fill="currentColor" viewBox="0 0 24 24">
              <path d="M22 12c0-5.523-4.477-10-10-10S2 6.477 2 12c0 4.991 3.657 9.128 8.438 9.878v-6.987h-2.54V12h2.54V9.797c0-2.506 1.492-3.89 3.777-3.89 1.094 0 2.238.195 2.238.195v2.46h-1.26c-1.243 0-1.63.771-1.63 1.562V12h2.773l-.443 2.89h-2.33v6.988C18.343 21.128 22 16.991 22 12z"/>
            </svg>
            <span class="ml-2">Facebook</span>
          </button>
        </div>
      </div>

      <!-- Register Link -->
      <div class="mt-8 text-center">
        <p class="text-sm text-gray-600">
          Don't have an account?
          <router-link to="/register" class="font-medium text-blue-600 hover:text-blue-500">
            Create one now
          </router-link>
        </p>
      </div>
    </div>
  </div>
</template>

<script>
import api from '../services/api';
import { mapActions } from 'vuex';

export default {
  name: 'UserLogin',
  data() {
    return {
      email: '',
      password: '',
      showPassword: false,
      rememberMe: false,
      loading: false,
      error: null
    };
  },
  methods: {
    ...mapActions('user', ['login']),
    ...mapActions('notification', ['showNotification']),
    async handleLogin() {
      this.loading = true;
      
      // Basic validation
      if (!this.email || !this.password) {
        this.showNotification({
          type: 'error',
          message: 'Vui lòng nhập đầy đủ email và mật khẩu'
        });
        this.loading = false;
        return;
      }

      try {
        // Make the login request
        const response = await api.post('/Auth/login', {
          email: this.email.trim(),
          password: this.password
        });

        // Check if we got a valid response with token
        if (response.data && response.data.token) {
          // Store the token and user info
          await this.login(response.data);
          
          // Store email if remember me is checked
          if (this.rememberMe) {
            localStorage.setItem('rememberedEmail', this.email);
          } else {
            localStorage.removeItem('rememberedEmail');
          }

          // Show success message
          this.showNotification({
            type: 'success',
            message: 'Đăng nhập thành công!'
          });

          // Redirect to home or intended page
          const redirectPath = this.$route.query.redirect || '/';
          this.$router.push(redirectPath);
        } else {
          throw new Error('Phản hồi không hợp lệ từ máy chủ');
        }
      } catch (error) {
        console.error('Login error:', error);
        
        // Handle specific error cases
        let errorMessage = 'Đã xảy ra lỗi không mong muốn. Vui lòng thử lại sau.';
        
        if (error.response) {
          const serverError = error.response.data;
          console.log('Server error response:', serverError); // Debug log

          // Check for specific error messages from server
          if (serverError?.message) {
            errorMessage = serverError.message;
          } else if (serverError?.error) {
            errorMessage = serverError.error;
          } else {
            // Handle based on status code if no specific message
            switch (error.response.status) {
              case 401:
                if (serverError?.includes('email')) {
                  errorMessage = 'Tài khoản của bạn chưa được xác thực email. Vui lòng kiểm tra hộp thư và xác thực email trước khi đăng nhập.';
                } else if (serverError?.includes('locked')) {
                  errorMessage = 'Tài khoản của bạn đã bị khóa do đăng nhập sai quá nhiều lần. Vui lòng thử lại sau 5 phút hoặc liên hệ hỗ trợ.';
                } else {
                  errorMessage = 'Email hoặc mật khẩu không chính xác. Vui lòng kiểm tra lại thông tin đăng nhập.';
                }
                break;
              case 403:
                if (serverError?.includes('password')) {
                  errorMessage = 'Mật khẩu không đúng định dạng. Vui lòng kiểm tra lại.';
                } else if (serverError?.includes('email')) {
                  errorMessage = 'Email không đúng định dạng. Vui lòng kiểm tra lại.';
                } else {
                  errorMessage = 'Thông tin đăng nhập không hợp lệ';
                }
                break;
              case 400:
                errorMessage = 'Tài khoản của bạn không có quyền truy cập. Vui lòng liên hệ quản trị viên.';
                break;
              case 404:
                errorMessage = 'Không tìm thấy tài khoản với email này. Vui lòng kiểm tra lại hoặc đăng ký tài khoản mới.';
                break;
              case 0:
                errorMessage = 'Không thể kết nối đến máy chủ. Vui lòng kiểm tra kết nối internet của bạn.';
                break;
              case 500:
                errorMessage = 'Máy chủ đang gặp sự cố. Vui lòng thử lại sau.';
                break;
              default:
                errorMessage = 'Đăng nhập thất bại. Vui lòng thử lại sau.';
            }
          }
        } else if (error.message === 'Network Error') {
          errorMessage = 'Lỗi kết nối mạng. Vui lòng kiểm tra kết nối internet của bạn.';
        } else if (error.message === 'timeout') {
          errorMessage = 'Kết nối đến máy chủ bị timeout. Vui lòng thử lại sau.';
        }

        // Log the final error message for debugging
        console.log('Final error message:', errorMessage);

        this.showNotification({
          type: 'error',
          message: errorMessage
        });
      } finally {
        this.loading = false;
      }
    },
    async handleGoogleLogin() {
      try {
        // Redirect to Google OAuth endpoint
        window.location.href = '/api/auth/google';
      } catch (error) {
        console.error('Google login error:', error);
        this.showNotification({
          type: 'error',
          message: 'Không thể kết nối với Google. Vui lòng thử lại sau.'
        });
      }
    },
    async handleFacebookLogin() {
      try {
        // Redirect to Facebook OAuth endpoint
        window.location.href = '/api/auth/facebook';
      } catch (error) {
        console.error('Facebook login error:', error);
        this.showNotification({
          type: 'error',
          message: 'Không thể kết nối với Facebook. Vui lòng thử lại sau.'
        });
      }
    }
  },
  mounted() {
    // Check for remembered email
    const rememberedEmail = localStorage.getItem('rememberedEmail');
    if (rememberedEmail) {
      this.email = rememberedEmail;
      this.rememberMe = true;
    }

    // Check for temporary credentials from registration
    const tempCredentials = localStorage.getItem('tempCredentials');
    if (tempCredentials) {
      try {
        const { email, password } = JSON.parse(tempCredentials);
        this.email = email;
        this.password = password;
        // Remove the temporary credentials after using them
        localStorage.removeItem('tempCredentials');
      } catch (error) {
        console.error('Error parsing temporary credentials:', error);
      }
    }
  },
};
</script>

<style scoped>
.bg-gradient-to-br {
  background-image: linear-gradient(to bottom right, var(--tw-gradient-stops));
}
</style>