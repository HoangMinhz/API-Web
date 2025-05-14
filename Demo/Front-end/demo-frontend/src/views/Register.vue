<template>
  <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-purple-50">
    <div class="bg-white p-8 rounded-xl shadow-xl w-full max-w-md transform transition-all duration-300 hover:shadow-2xl">
      <div class="text-center mb-8">
        <h1 class="text-3xl font-bold text-gray-800 mb-2">Create Account</h1>
        <p class="text-gray-600">Join our community today</p>
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

      <!-- Register Form -->
      <form @submit.prevent="handleRegister" class="space-y-6">
        <!-- Full Name Field -->
        <div class="space-y-2">
          <label for="fullName" class="block text-sm font-medium text-gray-700">Full Name</label>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <svg class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
              </svg>
            </div>
            <input
              v-model="form.fullName"
              id="fullName"
              type="text"
              class="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors duration-200"
              placeholder="Enter your full name"
              required
              autocomplete="name"
            />
          </div>
        </div>

        <!-- Username Field -->
        <div class="space-y-2">
          <label for="username" class="block text-sm font-medium text-gray-700">Username</label>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <svg class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
              </svg>
            </div>
            <input
              v-model="form.username"
              id="username"
              type="text"
              class="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors duration-200"
              placeholder="Choose a username"
              required
              autocomplete="username"
            />
          </div>
        </div>

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
              v-model="form.email"
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
              v-model="form.password"
              id="password"
              :type="showPassword ? 'text' : 'password'"
              class="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors duration-200"
              placeholder="Create a password"
              required
              autocomplete="new-password"
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

        <!-- Confirm Password Field -->
        <div class="space-y-2">
          <label for="confirmPassword" class="block text-sm font-medium text-gray-700">Confirm Password</label>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <svg class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
              </svg>
            </div>
            <input
              v-model="form.confirmPassword"
              id="confirmPassword"
              :type="showPassword ? 'text' : 'password'"
              class="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors duration-200"
              placeholder="Confirm your password"
              required
              autocomplete="new-password"
            />
          </div>
        </div>

        <!-- Terms and Conditions -->
        <div class="flex items-center">
          <input
            v-model="form.acceptTerms"
            id="accept-terms"
            type="checkbox"
            class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
            required
          />
          <label for="accept-terms" class="ml-2 block text-sm text-gray-700">
            I agree to the
            <a href="#" class="text-blue-600 hover:text-blue-500">Terms of Service</a>
            and
            <a href="#" class="text-blue-600 hover:text-blue-500">Privacy Policy</a>
          </label>
        </div>

        <!-- Register Button -->
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
            Creating account...
          </span>
          <span v-else>Create Account</span>
        </button>
      </form>

      <!-- Social Register Divider -->
      <div class="mt-6">
        <div class="relative">
          <div class="absolute inset-0 flex items-center">
            <div class="w-full border-t border-gray-300"></div>
          </div>
          <div class="relative flex justify-center text-sm">
            <span class="px-2 bg-white text-gray-500">Or sign up with</span>
          </div>
        </div>

        <!-- Social Register Buttons -->
        <div class="mt-6 grid grid-cols-2 gap-3">
          <button
            @click="handleGoogleRegister"
            class="w-full inline-flex justify-center py-2 px-4 border border-gray-300 rounded-md shadow-sm bg-white text-sm font-medium text-gray-500 hover:bg-gray-50"
          >
            <svg class="w-5 h-5" aria-hidden="true" fill="currentColor" viewBox="0 0 24 24">
              <path d="M12.48 10.92v3.28h7.84c-.24 1.84-.853 3.187-1.787 4.133-1.147 1.147-2.933 2.4-6.053 2.4-4.827 0-8.6-3.893-8.6-8.72s3.773-8.72 8.6-8.72c2.6 0 4.507 1.027 5.907 2.347l2.307-2.307C18.747 1.44 16.133 0 12.48 0 5.867 0 .307 5.387.307 12s5.56 12 12.173 12c3.573 0 6.267-1.173 8.373-3.36 2.16-2.16 2.84-5.213 2.84-7.667 0-.76-.053-1.467-.173-2.053H12.48z"/>
            </svg>
            <span class="ml-2">Google</span>
          </button>
          <button
            @click="handleFacebookRegister"
            class="w-full inline-flex justify-center py-2 px-4 border border-gray-300 rounded-md shadow-sm bg-white text-sm font-medium text-gray-500 hover:bg-gray-50"
          >
            <svg class="w-5 h-5" aria-hidden="true" fill="currentColor" viewBox="0 0 24 24">
              <path d="M22 12c0-5.523-4.477-10-10-10S2 6.477 2 12c0 4.991 3.657 9.128 8.438 9.878v-6.987h-2.54V12h2.54V9.797c0-2.506 1.492-3.89 3.777-3.89 1.094 0 2.238.195 2.238.195v2.46h-1.26c-1.243 0-1.63.771-1.63 1.562V12h2.773l-.443 2.89h-2.33v6.988C18.343 21.128 22 16.991 22 12z"/>
            </svg>
            <span class="ml-2">Facebook</span>
          </button>
        </div>
      </div>

      <!-- Login Link -->
      <div class="mt-8 text-center">
        <p class="text-sm text-gray-600">
          Already have an account?
          <router-link to="/login" class="font-medium text-blue-600 hover:text-blue-500">
            Sign in
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
  name: 'UserRegister',
  data() {
    return {
      form: {
        fullName: '',
        username: '',
        email: '',
        password: '',
        confirmPassword: '',
        acceptTerms: false
      },
      showPassword: false,
      error: null,
      loading: false,
    };
  },
  methods: {
    ...mapActions('user', ['register']),
    ...mapActions('notification', ['showNotification']),
    async handleRegister() {
      this.error = null;
      this.loading = true;

      try {
        // Validate passwords match
        if (this.form.password !== this.form.confirmPassword) {
          this.error = 'Passwords do not match';
          return;
        }

        // Validate password length
        if (this.form.password.length < 6) {
          this.error = 'Password must be at least 6 characters long';
          return;
        }

        // Validate username length
        if (this.form.username.length < 3) {
          this.error = 'Username must be at least 3 characters long';
          return;
        }

        const response = await api.post('/Auth/register', {
          username: this.form.username,
          email: this.form.email,
          password: this.form.password,
          confirmPassword: this.form.confirmPassword,
          fullName: this.form.fullName.trim()
        });

        if (response.data) {
          // Store credentials for auto-fill
          localStorage.setItem('tempCredentials', JSON.stringify({
            email: this.form.email,
            password: this.form.password
          }));

          this.showNotification({
            type: 'success',
            message: 'Registration successful! Please check your email to verify your account.'
          });
          
          // Redirect to login page after a short delay
          setTimeout(() => {
            this.$router.push('/login');
          }, 3000);
        }
      } catch (error) {
        console.error('Registration error:', error);
        if (error.response?.data?.errors) {
          // Handle validation errors from backend
          const errors = error.response.data.errors;
          this.error = Object.values(errors).flat().join(', ');
        } else if (error.response?.data?.message) {
          // Handle specific error message from backend
          this.error = error.response.data.message;
        } else if (error.message) {
          // Handle general error message
          this.error = error.message;
        } else {
          this.error = 'Registration failed. Please try again.';
        }
        this.showNotification({
          type: 'error',
          message: this.error
        });
      } finally {
        this.loading = false;
      }
    },
    async handleGoogleRegister() {
      try {
        // Redirect to Google OAuth endpoint
        window.location.href = '/api/auth/google';
      } catch (error) {
        console.error('Google registration error:', error);
        this.error = 'Failed to initiate Google registration';
        this.showNotification({
          type: 'error',
          message: this.error
        });
      }
    },
    async handleFacebookRegister() {
      try {
        // Redirect to Facebook OAuth endpoint
        window.location.href = '/api/auth/facebook';
      } catch (error) {
        console.error('Facebook registration error:', error);
        this.error = 'Failed to initiate Facebook registration';
        this.showNotification({
          type: 'error',
          message: this.error
        });
      }
    }
  }
};
</script>

<style scoped>
.bg-gradient-to-br {
  background-image: linear-gradient(to bottom right, var(--tw-gradient-stops));
}
</style>