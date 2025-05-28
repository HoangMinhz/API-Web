<template>
  <div class="relative">
    <!-- Notification Bell Icon -->
    <button
      @click="toggleDropdown"
      class="relative p-2 text-gray-600 hover:text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500 rounded-full"
      :class="{ 'text-blue-600': hasUnreadNotifications }"
    >
      <!-- Bell Icon -->
      <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
              d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
      </svg>
      
      <!-- Unread Count Badge -->
      <span
        v-if="unreadCount > 0"
        class="absolute -top-1 -right-1 bg-red-500 text-white text-xs rounded-full h-5 w-5 flex items-center justify-center font-medium"
      >
        {{ unreadCount > 99 ? '99+' : unreadCount }}
      </span>
      
      <!-- Connection Status Indicator -->
      <div
        class="absolute -bottom-1 -right-1 w-3 h-3 rounded-full border-2 border-white"
        :class="{
          'bg-green-500': connectionStatus.isConnected,
          'bg-red-500': !connectionStatus.isConnected,
          'bg-yellow-500': connectionStatus.state === 'Reconnecting'
        }"
      ></div>
    </button>

    <!-- Dropdown Panel -->
    <div
      v-if="showDropdown"
      class="absolute right-0 mt-2 w-80 bg-white rounded-lg shadow-lg border border-gray-200 z-50"
      @click.stop
    >
      <!-- Header -->
      <div class="px-4 py-3 border-b border-gray-200 flex items-center justify-between">
        <h3 class="text-lg font-semibold text-gray-900">Notifications</h3>
        <div class="flex items-center space-x-2">
          <!-- Connection Status -->
          <div class="flex items-center space-x-1">
            <div
              class="w-2 h-2 rounded-full"
              :class="{
                'bg-green-500': connectionStatus.isConnected,
                'bg-red-500': !connectionStatus.isConnected,
                'bg-yellow-500': connectionStatus.state === 'Reconnecting'
              }"
            ></div>
            <span class="text-xs text-gray-500">
              {{ connectionStatus.isConnected ? 'Live' : 'Offline' }}
            </span>
          </div>
          
          <!-- Clear All Button -->
          <button
            v-if="notifications.length > 0"
            @click="clearAllNotifications"
            class="text-xs text-blue-600 hover:text-blue-800"
          >
            Clear All
          </button>
        </div>
      </div>

      <!-- Notifications List -->
      <div class="max-h-96 overflow-y-auto">
        <!-- Empty State -->
        <div v-if="notifications.length === 0" class="px-4 py-8 text-center">
          <svg class="w-12 h-12 mx-auto text-gray-300 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                  d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
          </svg>
          <p class="text-gray-500 text-sm">No notifications yet</p>
          <p class="text-gray-400 text-xs mt-1">You'll see real-time updates here</p>
        </div>

        <!-- Notification Items -->
        <div v-else class="divide-y divide-gray-100">
          <div
            v-for="notification in displayedNotifications"
            :key="notification.id"
            class="px-4 py-3 hover:bg-gray-50 cursor-pointer transition-colors"
            :class="{ 'bg-blue-50': !notification.read }"
            @click="markAsRead(notification.id)"
          >
            <div class="flex items-start space-x-3">
              <!-- Icon -->
              <div
                class="flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center"
                :class="getNotificationIconClass(notification.type)"
              >
                <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                  <!-- Success Icon -->
                  <path
                    v-if="notification.type === 'success'"
                    fill-rule="evenodd"
                    d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
                    clip-rule="evenodd"
                  />
                  <!-- Warning Icon -->
                  <path
                    v-else-if="notification.type === 'warning'"
                    fill-rule="evenodd"
                    d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z"
                    clip-rule="evenodd"
                  />
                  <!-- Error Icon -->
                  <path
                    v-else-if="notification.type === 'error'"
                    fill-rule="evenodd"
                    d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
                    clip-rule="evenodd"
                  />
                  <!-- Info Icon (default) -->
                  <path
                    v-else
                    fill-rule="evenodd"
                    d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z"
                    clip-rule="evenodd"
                  />
                </svg>
              </div>

              <!-- Content -->
              <div class="flex-1 min-w-0">
                <p class="text-sm font-medium text-gray-900">{{ notification.title }}</p>
                <p class="text-sm text-gray-600 mt-1">{{ notification.message }}</p>
                <div class="flex items-center justify-between mt-2">
                  <span class="text-xs text-gray-500">{{ formatTimestamp(notification.timestamp) }}</span>
                  <span
                    v-if="notification.orderId"
                    class="text-xs bg-gray-100 text-gray-600 px-2 py-1 rounded"
                  >
                    Order #{{ notification.orderId }}
                  </span>
                </div>
              </div>

              <!-- Unread Indicator -->
              <div v-if="!notification.read" class="flex-shrink-0 w-2 h-2 bg-blue-500 rounded-full"></div>
            </div>
          </div>
        </div>
      </div>

      <!-- Footer -->
      <div v-if="notifications.length > maxDisplayed" class="px-4 py-3 border-t border-gray-200 text-center">
        <button
          @click="showAll = !showAll"
          class="text-sm text-blue-600 hover:text-blue-800"
        >
          {{ showAll ? 'Show Less' : `View All ${notifications.length} Notifications` }}
        </button>
      </div>
    </div>

    <!-- Backdrop -->
    <div
      v-if="showDropdown"
      class="fixed inset-0 z-40"
      @click="closeDropdown"
    ></div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useGlobalSignalR } from '@/composables/useSignalR'

// SignalR composable
const {
  connectionStatus,
  notifications,
  unreadCount,
  markNotificationAsRead,
  clearNotifications
} = useGlobalSignalR()

// Local state
const showDropdown = ref(false)
const showAll = ref(false)
const maxDisplayed = 10

// Computed properties
const hasUnreadNotifications = computed(() => unreadCount.value > 0)

const displayedNotifications = computed(() => {
  if (showAll.value) {
    return notifications
  }
  return notifications.slice(0, maxDisplayed)
})

// Methods
const toggleDropdown = () => {
  showDropdown.value = !showDropdown.value
}

const closeDropdown = () => {
  showDropdown.value = false
  showAll.value = false
}

const markAsRead = (notificationId) => {
  markNotificationAsRead(notificationId)
}

const clearAllNotifications = () => {
  clearNotifications()
  closeDropdown()
}

const getNotificationIconClass = (type) => {
  switch (type) {
    case 'success':
      return 'bg-green-100 text-green-600'
    case 'warning':
      return 'bg-yellow-100 text-yellow-600'
    case 'error':
      return 'bg-red-100 text-red-600'
    default:
      return 'bg-blue-100 text-blue-600'
  }
}

const formatTimestamp = (timestamp) => {
  const date = new Date(timestamp)
  const now = new Date()
  const diffInMinutes = Math.floor((now - date) / (1000 * 60))
  
  if (diffInMinutes < 1) {
    return 'Just now'
  } else if (diffInMinutes < 60) {
    return `${diffInMinutes}m ago`
  } else if (diffInMinutes < 1440) {
    const hours = Math.floor(diffInMinutes / 60)
    return `${hours}h ago`
  } else {
    const days = Math.floor(diffInMinutes / 1440)
    return `${days}d ago`
  }
}

// Close dropdown when clicking outside
const handleClickOutside = (event) => {
  if (!event.target.closest('.notification-bell')) {
    closeDropdown()
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})
</script>

<style scoped>
/* Custom scrollbar for notifications list */
.overflow-y-auto::-webkit-scrollbar {
  width: 4px;
}

.overflow-y-auto::-webkit-scrollbar-track {
  background: #f1f1f1;
}

.overflow-y-auto::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 2px;
}

.overflow-y-auto::-webkit-scrollbar-thumb:hover {
  background: #a8a8a8;
}
</style> 