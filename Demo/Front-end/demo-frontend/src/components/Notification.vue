<template>
  <div class="fixed top-4 right-4 z-50 space-y-3">
    <TransitionGroup
      name="notification"
      tag="div"
      class="space-y-3"
    >
      <div
        v-for="notification in $store.state.notification.notifications"
        :key="notification.id"
        class="relative flex items-start p-4 pr-12 rounded-lg shadow-lg max-w-sm transform transition-all duration-300"
        :class="{
          'bg-green-50 border border-green-200': notification.type === 'success',
          'bg-red-50 border border-red-200': notification.type === 'error',
          'bg-blue-50 border border-blue-200': notification.type === 'info',
          'bg-yellow-50 border border-yellow-200': notification.type === 'warning'
        }"
      >
        <!-- Icon -->
        <div class="flex-shrink-0 mr-3">
          <svg
            v-if="notification.type === 'success'"
            class="w-6 h-6 text-green-500"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
          </svg>
          <svg
            v-else-if="notification.type === 'error'"
            class="w-6 h-6 text-red-500"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
          <svg
            v-else-if="notification.type === 'info'"
            class="w-6 h-6 text-blue-500"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <svg
            v-else-if="notification.type === 'warning'"
            class="w-6 h-6 text-yellow-500"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
          </svg>
        </div>

        <!-- Content -->
        <div class="flex-1">
          <p class="text-sm font-medium" :class="{
            'text-green-800': notification.type === 'success',
            'text-red-800': notification.type === 'error',
            'text-blue-800': notification.type === 'info',
            'text-yellow-800': notification.type === 'warning'
          }">
            {{ notification.message }}
          </p>
        </div>

        <!-- Close Button -->
        <button
          @click="$store.commit('notification/removeNotification', notification.id)"
          class="absolute top-3 right-3 p-1 rounded-full hover:bg-white/50 transition-colors duration-150"
          :class="{
            'text-green-600 hover:text-green-700': notification.type === 'success',
            'text-red-600 hover:text-red-700': notification.type === 'error',
            'text-blue-600 hover:text-blue-700': notification.type === 'info',
            'text-yellow-600 hover:text-yellow-700': notification.type === 'warning'
          }"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>
    </TransitionGroup>
  </div>
</template>

<script>
export default {
  name: 'NotificationToast'
};
</script>

<style scoped>
.notification-enter-active,
.notification-leave-active {
  transition: all 0.3s ease;
}

.notification-enter-from {
  opacity: 0;
  transform: translateX(100%);
}

.notification-leave-to {
  opacity: 0;
  transform: translateX(100%);
}

.notification-move {
  transition: transform 0.3s ease;
}
</style> 