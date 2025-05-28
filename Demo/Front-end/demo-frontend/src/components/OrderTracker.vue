<template>
  <div class="order-tracker">
    <div class="bg-white rounded-lg shadow-md p-6">
      <!-- Header -->
      <div class="flex items-center justify-between mb-6">
        <h2 class="text-xl font-semibold text-gray-900">
          Order #{{ orderId }}
        </h2>
        <div class="flex items-center space-x-2">
          <div
            class="w-3 h-3 rounded-full"
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

      <!-- Order Status Timeline -->
      <div class="relative">
        <div class="absolute left-4 top-0 bottom-0 w-0.5 bg-gray-200"></div>
        
        <div
          v-for="(status, index) in orderStatuses"
          :key="status.value"
          class="relative flex items-center mb-8 last:mb-0"
        >
          <!-- Status Icon -->
          <div
            class="relative z-10 flex items-center justify-center w-8 h-8 rounded-full border-2"
            :class="getStatusIconClass(status, index)"
          >
            <svg
              v-if="isStatusCompleted(status, index)"
              class="w-4 h-4"
              fill="currentColor"
              viewBox="0 0 20 20"
            >
              <path
                fill-rule="evenodd"
                d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
                clip-rule="evenodd"
              ></path>
            </svg>
            <div
              v-else-if="isStatusCurrent(status, index)"
              class="w-3 h-3 rounded-full bg-current animate-pulse"
            ></div>
            <div
              v-else
              class="w-3 h-3 rounded-full bg-current opacity-30"
            ></div>
          </div>

          <!-- Status Content -->
          <div class="ml-4 flex-1">
            <div class="flex items-center justify-between">
              <h3
                class="text-sm font-medium"
                :class="{
                  'text-green-600': isStatusCompleted(status, index),
                  'text-blue-600': isStatusCurrent(status, index),
                  'text-gray-400': !isStatusCompleted(status, index) && !isStatusCurrent(status, index)
                }"
              >
                {{ status.label }}
              </h3>
              <span
                v-if="status.timestamp"
                class="text-xs text-gray-500"
              >
                {{ formatTimestamp(status.timestamp) }}
              </span>
            </div>
            <p
              class="text-sm mt-1"
              :class="{
                'text-gray-600': isStatusCompleted(status, index) || isStatusCurrent(status, index),
                'text-gray-400': !isStatusCompleted(status, index) && !isStatusCurrent(status, index)
              }"
            >
              {{ status.description }}
            </p>
          </div>
        </div>
      </div>

      <!-- Real-time Updates -->
      <div v-if="recentUpdates.length > 0" class="mt-6 pt-6 border-t border-gray-200">
        <h3 class="text-sm font-medium text-gray-900 mb-3">Recent Updates</h3>
        <div class="space-y-2">
          <div
            v-for="update in recentUpdates"
            :key="update.id"
            class="flex items-start space-x-3 p-3 bg-blue-50 rounded-lg"
          >
            <div class="flex-shrink-0 w-2 h-2 bg-blue-500 rounded-full mt-2"></div>
            <div class="flex-1">
              <p class="text-sm text-gray-900">{{ update.message }}</p>
              <p class="text-xs text-gray-500 mt-1">
                {{ formatTimestamp(update.timestamp) }}
              </p>
            </div>
          </div>
        </div>
      </div>

      <!-- Order Actions -->
      <div v-if="canCancelOrder" class="mt-6 pt-6 border-t border-gray-200">
        <button
          @click="cancelOrder"
          :disabled="isCancelling"
          class="px-4 py-2 bg-red-600 text-white text-sm font-medium rounded-md hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-500 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <span v-if="isCancelling">Cancelling...</span>
          <span v-else>Cancel Order</span>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useGlobalSignalR } from '@/composables/useSignalR'

// Props
const props = defineProps({
  orderId: {
    type: [String, Number],
    required: true
  },
  initialStatus: {
    type: String,
    default: 'Pending'
  },
  orderData: {
    type: Object,
    default: () => ({})
  }
})

// Emits
const emit = defineEmits(['orderCancelled', 'statusChanged'])

// SignalR composable
const {
  isConnected,
  connectionState,
  trackOrder,
  stopTrackingOrder,
  getOrderUpdates,
  clearOrderUpdates
} = useGlobalSignalR()

// Local state
const currentStatus = ref(props.initialStatus)
const recentUpdates = ref([])
const isCancelling = ref(false)

// Order statuses configuration
const orderStatuses = ref([
  {
    value: 'Pending',
    label: 'Order Placed',
    description: 'Your order has been received and is being reviewed.',
    timestamp: null
  },
  {
    value: 'Confirmed',
    label: 'Order Confirmed',
    description: 'Your order has been confirmed and is being prepared.',
    timestamp: null
  },
  {
    value: 'Processing',
    label: 'Processing',
    description: 'Your order is being processed and prepared for shipment.',
    timestamp: null
  },
  {
    value: 'Shipped',
    label: 'Shipped',
    description: 'Your order has been shipped and is on its way to you.',
    timestamp: null
  },
  {
    value: 'Delivered',
    label: 'Delivered',
    description: 'Your order has been delivered successfully.',
    timestamp: null
  }
])

// Computed properties
const connectionStatusText = computed(() => {
  if (isConnected.value) {
    return 'Live tracking active'
  } else if (connectionState.value === 'Reconnecting') {
    return 'Reconnecting...'
  } else {
    return 'Live tracking unavailable'
  }
})

const currentStatusIndex = computed(() => {
  return orderStatuses.value.findIndex(status => status.value === currentStatus.value)
})

const canCancelOrder = computed(() => {
  return ['Pending', 'Confirmed', 'Processing'].includes(currentStatus.value)
})

// Methods
const isStatusCompleted = (status, index) => {
  if (currentStatus.value === 'Cancelled') {
    return false
  }
  return index < currentStatusIndex.value || 
         (index === currentStatusIndex.value && currentStatus.value === status.value)
}

const isStatusCurrent = (status, index) => {
  if (currentStatus.value === 'Cancelled') {
    return false
  }
  return index === currentStatusIndex.value && currentStatus.value === status.value
}

const getStatusIconClass = (status, index) => {
  if (currentStatus.value === 'Cancelled') {
    return 'border-red-300 bg-red-50 text-red-400'
  }
  
  if (isStatusCompleted(status, index)) {
    return 'border-green-500 bg-green-500 text-white'
  } else if (isStatusCurrent(status, index)) {
    return 'border-blue-500 bg-blue-500 text-white'
  } else {
    return 'border-gray-300 bg-gray-50 text-gray-400'
  }
}

const formatTimestamp = (timestamp) => {
  if (!timestamp) return ''
  
  const date = new Date(timestamp)
  const now = new Date()
  const diffInMinutes = Math.floor((now - date) / (1000 * 60))
  
  if (diffInMinutes < 1) {
    return 'Just now'
  } else if (diffInMinutes < 60) {
    return `${diffInMinutes} minutes ago`
  } else if (diffInMinutes < 1440) {
    const hours = Math.floor(diffInMinutes / 60)
    return `${hours} hours ago`
  } else {
    return date.toLocaleDateString() + ' ' + date.toLocaleTimeString()
  }
}

const updateOrderStatus = (newStatus, message, timestamp) => {
  const oldStatus = currentStatus.value
  currentStatus.value = newStatus
  
  // Update timestamp for current status
  const statusIndex = orderStatuses.value.findIndex(s => s.value === newStatus)
  if (statusIndex !== -1) {
    orderStatuses.value[statusIndex].timestamp = timestamp || new Date().toISOString()
  }
  
  // Add to recent updates
  addRecentUpdate({
    id: Date.now(),
    message: message || `Order status changed from ${oldStatus} to ${newStatus}`,
    timestamp: timestamp || new Date().toISOString()
  })
  
  emit('statusChanged', {
    orderId: props.orderId,
    oldStatus,
    newStatus,
    timestamp
  })
}

const addRecentUpdate = (update) => {
  recentUpdates.value.unshift(update)
  
  // Keep only last 5 updates
  if (recentUpdates.value.length > 5) {
    recentUpdates.value.splice(5)
  }
}

const cancelOrder = async () => {
  if (isCancelling.value) return
  
  try {
    isCancelling.value = true
    
    // Call API to cancel order
    const response = await fetch(`/api/order/${props.orderId}/cancel`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('token')}`,
        'Content-Type': 'application/json'
      }
    })
    
    if (response.ok) {
      updateOrderStatus('Cancelled', 'Order has been cancelled successfully', new Date().toISOString())
      emit('orderCancelled', props.orderId)
    } else {
      throw new Error('Failed to cancel order')
    }
  } catch (error) {
    console.error('Error cancelling order:', error)
    addRecentUpdate({
      id: Date.now(),
      message: 'Failed to cancel order. Please try again.',
      timestamp: new Date().toISOString()
    })
  } finally {
    isCancelling.value = false
  }
}

const handleOrderUpdates = () => {
  const updates = getOrderUpdates(props.orderId)
  if (updates) {
    switch (updates.type) {
      case 'statusChanged':
        updateOrderStatus(
          updates.NewStatus,
          updates.Message,
          updates.Timestamp
        )
        break
      case 'cancelled':
        updateOrderStatus(
          'Cancelled',
          updates.Message || 'Order has been cancelled',
          updates.Timestamp
        )
        break
    }
    
    // Clear the updates after processing
    clearOrderUpdates(props.orderId)
  }
}

// Initialize order data
const initializeOrderData = () => {
  if (props.orderData.orderDate) {
    orderStatuses.value[0].timestamp = props.orderData.orderDate
  }
  
  // Set timestamps for completed statuses based on current status
  const currentIndex = currentStatusIndex.value
  for (let i = 0; i <= currentIndex; i++) {
    if (!orderStatuses.value[i].timestamp) {
      orderStatuses.value[i].timestamp = new Date().toISOString()
    }
  }
}

// Watch for order updates
watch(() => getOrderUpdates(props.orderId), handleOrderUpdates)

// Lifecycle hooks
onMounted(async () => {
  initializeOrderData()
  
  // Start tracking this order
  if (isConnected.value) {
    await trackOrder(props.orderId)
  }
})

onUnmounted(async () => {
  // Stop tracking this order
  if (isConnected.value) {
    await stopTrackingOrder(props.orderId)
  }
})
</script>

<style scoped>
.order-tracker {
  max-width: 600px;
}

@keyframes pulse {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: 0.5;
  }
}

.animate-pulse {
  animation: pulse 2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
}
</style> 