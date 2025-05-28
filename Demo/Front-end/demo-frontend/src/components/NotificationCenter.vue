<template>
  <div class="notification-center">
    <!-- Notification Bell Icon -->
    <div class="relative">
      <button
        @click="toggleNotifications"
        class="relative p-2 text-gray-600 hover:text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500 rounded-lg"
        :class="{ 'text-blue-600': showNotifications }"
      >
        <svg
          class="w-6 h-6"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
          xmlns="http://www.w3.org/2000/svg"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"
          ></path>
        </svg>
        
        <!-- Unread count badge -->
        <span
          v-if="unreadCount > 0"
          class="absolute -top-1 -right-1 bg-red-500 text-white text-xs rounded-full h-5 w-5 flex items-center justify-center"
        >
          {{ unreadCount > 99 ? '99+' : unreadCount }}
        </span>
      </button>

      <!-- Notifications Dropdown -->
      <div
        v-if="showNotifications"
        class="absolute right-0 mt-2 w-80 bg-white rounded-lg shadow-lg border border-gray-200 z-50"
        @click.stop
      >
        <!-- Header -->
        <div class="px-4 py-3 border-b border-gray-200 flex justify-between items-center">
          <h3 class="text-lg font-semibold text-gray-900">Notifications</h3>
          <div class="flex space-x-2">
            <button
              v-if="unreadCount > 0"
              @click="markAllAsRead"
              class="text-sm text-blue-600 hover:text-blue-800"
            >
              Mark all read
            </button>
            <button
              @click="clearAllNotifications"
              class="text-sm text-red-600 hover:text-red-800"
            >
              Clear all
            </button>
          </div>
        </div>

        <!-- Connection Status -->
        <div class="px-4 py-2 border-b border-gray-100">
          <div class="flex items-center space-x-2">
            <div
              class="w-2 h-2 rounded-full"
              :class="{
                'bg-green-500': isConnected,
                'bg-red-500': !isConnected,
                'bg-yellow-500': connectionState === 'Reconnecting'
              }"
            ></div>
            <span class="text-sm text-gray-600">
              {{ connectionStatusText }}
            </span>
          </div>
        </div>

        <!-- Notifications List -->
        <div class="max-h-96 overflow-y-auto">
          <div v-if="notifications.length === 0" class="px-4 py-8 text-center text-gray-500">
            <svg
              class="w-12 h-12 mx-auto mb-2 text-gray-300"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2M4 13h2m13-8l-4 4m0 0l-4-4m4 4V3"
              ></path>
            </svg>
            <p>No notifications yet</p>
          </div>

          <div
            v-for="notification in notifications"
            :key="notification.id"
            class="px-4 py-3 border-b border-gray-100 hover:bg-gray-50 cursor-pointer"
            :class="{
              'bg-blue-50': !notification.read,
              'bg-white': notification.read
            }"
            @click="markAsRead(notification.id)"
          >
            <div class="flex items-start space-x-3">
              <!-- Notification Icon -->
              <div
                class="flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center"
                :class="getNotificationIconClass(notification.type)"
              >
                <svg
                  class="w-4 h-4"
                  fill="currentColor"
                  viewBox="0 0 20 20"
                >
                  <path
                    v-if="notification.type === 'success'"
                    fill-rule="evenodd"
                    d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
                    clip-rule="evenodd"
                  ></path>
                  <path
                    v-else-if="notification.type === 'warning'"
                    fill-rule="evenodd"
                    d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z"
                    clip-rule="evenodd"
                  ></path>
                  <path
                    v-else
                    fill-rule="evenodd"
                    d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z"
                    clip-rule="evenodd"
                  ></path>
                </svg>
              </div>

              <!-- Notification Content -->
              <div class="flex-1 min-w-0">
                <p class="text-sm font-medium text-gray-900">
                  {{ notification.title }}
                </p>
                <p class="text-sm text-gray-600 mt-1">
                  {{ notification.message }}
                </p>
                <div class="flex items-center justify-between mt-2">
                  <p class="text-xs text-gray-500">
                    {{ formatTimestamp(notification.timestamp) }}
                  </p>
                  <span
                    v-if="notification.orderId"
                    class="text-xs bg-gray-100 text-gray-700 px-2 py-1 rounded"
                  >
                    Order #{{ notification.orderId }}
                  </span>
                </div>
              </div>

              <!-- Unread indicator -->
              <div
                v-if="!notification.read"
                class="flex-shrink-0 w-2 h-2 bg-blue-500 rounded-full"
              ></div>
            </div>
          </div>
        </div>

        <!-- Footer -->
        <div class="px-4 py-3 border-t border-gray-200">
          <button
            @click="viewAllNotifications"
            class="w-full text-center text-sm text-blue-600 hover:text-blue-800"
          >
            View all notifications
          </button>
        </div>
      </div>
    </div>

    <!-- Overlay to close dropdown -->
    <div
      v-if="showNotifications"
      class="fixed inset-0 z-40"
      @click="showNotifications = false"
    ></div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useGlobalSignalR } from '@/composables/useSignalR'

// SignalR composable
const {
  isConnected,
  connectionState,
  notifications,
  unreadCount,
  markNotificationAsRead,
  clearNotifications
} = useGlobalSignalR()

// Local state
const showNotifications = ref(false)

// Computed properties
const connectionStatusText = computed(() => {
  if (isConnected.value) {
    return 'Connected'
  } else if (connectionState.value === 'Reconnecting') {
    return 'Reconnecting...'
  } else {
    return 'Disconnected'
  }
})

// Methods
const toggleNotifications = () => {
  showNotifications.value = !showNotifications.value
}

const markAsRead = (notificationId) => {
  markNotificationAsRead(notificationId)
}

const markAllAsRead = () => {
  notifications.forEach(notification => {
    if (!notification.read) {
      markNotificationAsRead(notification.id)
    }
  })
}

const clearAllNotifications = () => {
  clearNotifications()
  showNotifications.value = false
}

const viewAllNotifications = () => {
  // Navigate to notifications page
  showNotifications.value = false
  // You can implement navigation here
  console.log('Navigate to notifications page')
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
  if (!event.target.closest('.notification-center')) {
    showNotifications.value = false
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
.max-h-96::-webkit-scrollbar {
  width: 4px;
}

.max-h-96::-webkit-scrollbar-track {
  background: #f1f1f1;
}

.max-h-96::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 2px;
}

.max-h-96::-webkit-scrollbar-thumb:hover {
  background: #a8a8a8;
}
</style> 