<template>
  <div class="min-h-screen bg-gray-50 py-12">
    <div class="container mx-auto px-4 max-w-4xl">
      <div class="bg-white rounded-xl shadow-sm overflow-hidden">
        <!-- Profile Header -->
        <div class="bg-blue-600 px-8 py-6">
          <h1 class="text-2xl font-bold text-white">My Profile</h1>
          <p class="text-blue-100 mt-1">Manage your account information</p>
        </div>

        <!-- Loading State -->
        <div v-if="loading" class="flex justify-center items-center py-12">
          <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
        </div>

        <!-- Error State -->
        <div v-else-if="error" class="bg-red-100 border border-red-400 text-red-700 px-4 py-3 m-4 rounded-lg">
          {{ error }}
        </div>

        <!-- Profile Content -->
        <div v-else class="p-8">
          <form @submit.prevent="updateProfile" class="space-y-6">
            <!-- Personal Information -->
            <div>
              <h2 class="text-lg font-semibold text-gray-900 mb-4">Personal Information</h2>
              <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Username</label>
                  <input
                    v-model="profile.username"
                    type="text"
                    disabled
                    class="w-full px-4 py-3 bg-gray-50 border border-gray-300 rounded-lg text-gray-500"
                  />
                </div>

                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Email</label>
                  <input
                    v-model="profile.email"
                    type="email"
                    disabled
                    class="w-full px-4 py-3 bg-gray-50 border border-gray-300 rounded-lg text-gray-500"
                  />
                </div>

                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Full Name</label>
                  <input
                    v-model="profile.fullName"
                    type="text"
                    class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    placeholder="Enter your full name"
                  />
                </div>

                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Phone Number</label>
                  <input
                    v-model="profile.phoneNumber"
                    type="tel"
                    class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    placeholder="Enter your phone number"
                  />
                </div>

                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Address</label>
                  <input
                    v-model="profile.address"
                    type="text"
                    class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    placeholder="Enter your address"
                  />
                </div>

                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Province/City</label>
                  <select
                    v-model="profile.province"
                    class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  >
                    <option value="">Select province/city</option>
                    <option v-for="province in provinces" :key="province.code" :value="province.code">
                      {{ province.name }}
                    </option>
                  </select>
                </div>

                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">District</label>
                  <select
                    v-model="profile.district"
                    class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    :disabled="!profile.province"
                  >
                    <option value="">Select district</option>
                    <option v-for="district in districts" :key="district.code" :value="district.code">
                      {{ district.name }}
                    </option>
                  </select>
                </div>
              </div>
            </div>

            <!-- Change Password -->
            <div>
              <h2 class="text-lg font-semibold text-gray-900 mb-4">Change Password</h2>
              <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Current Password</label>
                  <input
                    v-model="password.currentPassword"
                    type="password"
                    class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    placeholder="Enter current password"
                  />
                </div>

                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">New Password</label>
                  <input
                    v-model="password.newPassword"
                    type="password"
                    class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    placeholder="Enter new password"
                  />
                </div>
              </div>
            </div>

            <!-- Submit Button -->
            <div class="flex justify-end space-x-4">
              <button
                type="button"
                @click="resetForm"
                class="px-6 py-3 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
              >
                Reset
              </button>
              <button
                type="submit"
                :disabled="isSubmitting"
                class="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {{ isSubmitting ? 'Saving...' : 'Save Changes' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import api from '../services/api';
import { provinces, districts } from '../data/vietnam-locations';
import { mapActions } from 'vuex';

export default {
  name: 'ProfilePage',
  data() {
    return {
      loading: true,
      error: null,
      isSubmitting: false,
      provinces,
      districts: [],
      profile: {
        username: '',
        email: '',
        fullName: '',
        phoneNumber: '',
        address: '',
        province: '',
        district: ''
      },
      password: {
        currentPassword: '',
        newPassword: ''
      },
      originalProfile: null
    };
  },
  watch: {
    'profile.province': {
      handler(newProvince) {
        if (newProvince) {
          this.districts = districts[newProvince] || [];
          if (this.districts.length === 0) {
            this.profile.district = '';
          }
        } else {
          this.districts = [];
          this.profile.district = '';
        }
      },
      immediate: true
    }
  },
  async created() {
    await this.fetchProfile();
  },
  methods: {
    ...mapActions('notification', ['showNotification']),
    async fetchProfile() {
      this.loading = true;
      this.error = null;
      try {
        const response = await api.get('/User/current');
        if (response.data) {
          this.profile = {
            username: response.data.username || '',
            email: response.data.email || '',
            fullName: response.data.fullName || '',
            phoneNumber: response.data.phoneNumber || '',
            address: response.data.address || '',
            province: response.data.province || '',
            district: response.data.district || ''
          };
          this.originalProfile = { ...this.profile };
        }
      } catch (error) {
        console.error('Failed to fetch profile:', error);
        this.error = 'Failed to load profile information';
        this.showNotification({
          type: 'error',
          message: this.error
        });
      } finally {
        this.loading = false;
      }
    },
    async updateProfile() {
      if (this.isSubmitting) return;

      this.loading = true;
      this.error = null;
      try {
        const response = await api.put('/User/update-profile', {
          fullName: this.profile.fullName.trim(),
          phoneNumber: this.profile.phoneNumber,
          address: this.profile.address,
          province: this.profile.province,
          district: this.profile.district
        });

        if (response.data) {
          this.showNotification({
            type: 'success',
            message: 'Profile updated successfully'
          });
          this.originalProfile = { ...this.profile };
        }
      } catch (error) {
        console.error('Failed to update profile:', error);
        this.error = error.response?.data?.message || 'Failed to update profile';
        this.showNotification({
          type: 'error',
          message: this.error
        });
      } finally {
        this.loading = false;
        this.isSubmitting = false;
      }
    },
    resetForm() {
      this.profile = { ...this.originalProfile };
      this.password = { currentPassword: '', newPassword: '' };
    }
  }
};
</script> 