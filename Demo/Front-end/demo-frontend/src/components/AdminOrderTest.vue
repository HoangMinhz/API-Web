<template>
  <div class="admin-order-test bg-white rounded-lg shadow p-6 mb-6">
    <h2 class="text-xl font-semibold mb-4">Admin Order Test (SignalR)</h2>
    
    <!-- Connection Status -->
    <div class="mb-4">
      <div class="flex items-center space-x-3">
        <div
          class="w-4 h-4 rounded-full"
          :class="{
            'bg-green-500': connectionStatus.isConnected,
            'bg-red-500': !connectionStatus.isConnected,
            'bg-yellow-500': connectionStatus.state === 'Reconnecting'
          }"
        ></div>
        <span class="font-medium">
          Admin SignalR: {{ connectionStatus.isConnected ? 'Connected' : 'Disconnected' }}
        </span>
      </div>
    </div>

    <!-- Test Order Updates -->
    <div class="mb-4">
      <h3 class="font-medium mb-2">Simulate Admin Actions:</h3>
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        
        <!-- Order ID Input -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Order ID:</label>
          <input
            v-model="testOrderId"
            type="number"
            class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="Enter order ID (e.g., 1)"
          >
        </div>

        <!-- Status Selection -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">New Status:</label>
          <select
            v-model="selectedStatus"
            class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="Pending">Pending</option>
            <option value="Processing">Processing</option>
            <option value="Shipped">Shipped</option>
            <option value="Delivered">Delivered</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        </div>
      </div>

      <!-- Action Buttons -->
      <div class="mt-4 space-x-2">
        <button
          @click="updateOrderStatus"
          :disabled="!testOrderId || isUpdating"
          class="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50"
        >
          {{ isUpdating ? 'Updating...' : 'Update Order Status' }}
        </button>
        <button
          @click="cancelOrder"
          :disabled="!testOrderId || isCancelling"
          class="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700 disabled:opacity-50"
        >
          {{ isCancelling ? 'Cancelling...' : 'Cancel Order' }}
        </button>
        <button
          @click="sendOrderMessageToUser"
          class="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700"
        >
          Send Message to Order
        </button>
      </div>
    </div>

    <!-- Manual SignalR Events -->
    <div class="mb-4">
      <h3 class="font-medium mb-2">Manual SignalR Events (for testing):</h3>
      <div class="space-x-2">
        <button
          @click="triggerOrderStatusChanged"
          class="px-3 py-1 bg-purple-600 text-white rounded text-sm hover:bg-purple-700"
        >
          Trigger Status Changed
        </button>
        <button
          @click="triggerNewOrderReceived"
          class="px-3 py-1 bg-indigo-600 text-white rounded text-sm hover:bg-indigo-700"
        >
          Trigger New Order
        </button>
        <button
          @click="triggerOrderCancelled"
          class="px-3 py-1 bg-red-600 text-white rounded text-sm hover:bg-red-700"
        >
          Trigger Order Cancelled
        </button>
      </div>
    </div>

    <!-- API Response Log -->
    <div class="mb-4">
      <h3 class="font-medium mb-2">API Response Log:</h3>
      <div class="bg-gray-100 p-3 rounded max-h-40 overflow-y-auto text-sm">
        <div v-if="apiLogs.length === 0" class="text-gray-500">No API calls yet...</div>
        <div v-for="log in apiLogs" :key="log.id" class="mb-1">
          <span class="text-gray-500">{{ log.timestamp }}</span>
          <span :class="getLogClass(log.type)">{{ log.message }}</span>
        </div>
      </div>
      <button
        @click="clearApiLogs"
        class="mt-2 px-3 py-1 bg-gray-600 text-white rounded text-sm hover:bg-gray-700"
      >
        Clear Logs
      </button>
    </div>

    <!-- Instructions -->
    <div class="text-sm text-gray-600 bg-blue-50 p-3 rounded">
      <strong>Instructions:</strong>
      <ol class="list-decimal list-inside mt-1 space-y-1">
        <li>Make sure both admin and user are logged in</li>
        <li>Enter an order ID that exists in the system</li>
        <li>Click "Update Order Status" to simulate admin updating order</li>
        <li>Check if user receives real-time notification (without F5)</li>
        <li>If no notification appears, check console for errors</li>
      </ol>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useGlobalSignalR } from '@/composables/useSignalR'

const {
  connectionStatus,
  joinAdminDashboard,
  sendOrderMessage
} = useGlobalSignalR()

// Local state
const testOrderId = ref(1)
const selectedStatus = ref('Processing')
const isUpdating = ref(false)
const isCancelling = ref(false)
const apiLogs = ref([])

const addApiLog = (message, type = 'info') => {
  apiLogs.value.unshift({
    id: Date.now(),
    timestamp: new Date().toLocaleTimeString(),
    message,
    type
  })
  
  // Keep only last 20 logs
  if (apiLogs.value.length > 20) {
    apiLogs.value.splice(20)
  }
}

const getLogClass = (type) => {
  switch (type) {
    case 'success':
      return 'text-green-600 font-medium'
    case 'error':
      return 'text-red-600 font-medium'
    case 'warning':
      return 'text-yellow-600 font-medium'
    default:
      return 'text-gray-700'
  }
}

const updateOrderStatus = async () => {
  if (!testOrderId.value || isUpdating.value) return
  
  isUpdating.value = true
  addApiLog(`Updating order ${testOrderId.value} to ${selectedStatus.value}...`, 'info')
  
  try {
    const token = localStorage.getItem('token')
    const response = await fetch(`http://localhost:5285/api/order/${testOrderId.value}/status`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        Status: getStatusNumber(selectedStatus.value)
      })
    })
    
    if (response.ok) {
      const result = await response.json()
      addApiLog(`✅ Order ${testOrderId.value} updated successfully to ${selectedStatus.value}`, 'success')
      addApiLog(`Response: ${JSON.stringify(result)}`, 'info')
    } else {
      const error = await response.text()
      addApiLog(`❌ Failed to update order: ${response.status} - ${error}`, 'error')
    }
  } catch (error) {
    addApiLog(`❌ Network error: ${error.message}`, 'error')
  } finally {
    isUpdating.value = false
  }
}

const cancelOrder = async () => {
  if (!testOrderId.value || isCancelling.value) return
  
  isCancelling.value = true
  addApiLog(`Cancelling order ${testOrderId.value}...`, 'info')
  
  try {
    const token = localStorage.getItem('token')
    const response = await fetch(`http://localhost:5285/api/order/${testOrderId.value}/cancel`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    })
    
    if (response.ok) {
      const result = await response.json()
      addApiLog(`✅ Order ${testOrderId.value} cancelled successfully`, 'success')
      addApiLog(`Response: ${JSON.stringify(result)}`, 'info')
    } else {
      const error = await response.text()
      addApiLog(`❌ Failed to cancel order: ${response.status} - ${error}`, 'error')
    }
  } catch (error) {
    addApiLog(`❌ Network error: ${error.message}`, 'error')
  } finally {
    isCancelling.value = false
  }
}

const sendOrderMessageToUser = async () => {
  if (!testOrderId.value) return
  
  try {
    const success = await sendOrderMessage(testOrderId.value, `Admin message for order ${testOrderId.value}`)
    if (success) {
      addApiLog(`✅ Message sent to order ${testOrderId.value}`, 'success')
    } else {
      addApiLog(`❌ Failed to send message`, 'error')
    }
  } catch (error) {
    addApiLog(`❌ Error sending message: ${error.message}`, 'error')
  }
}

// Manual SignalR event triggers (for testing)
const triggerOrderStatusChanged = () => {
  // Simulate receiving a SignalR event
  window.dispatchEvent(new CustomEvent('signalr-test', {
    detail: {
      type: 'OrderStatusChanged',
      data: {
        OrderId: testOrderId.value,
        OldStatus: 'Pending',
        NewStatus: selectedStatus.value,
        Timestamp: new Date().toISOString(),
        Message: `Order #${testOrderId.value} status changed to ${selectedStatus.value}`
      }
    }
  }))
  addApiLog(`🧪 Triggered OrderStatusChanged event for order ${testOrderId.value}`, 'warning')
}

const triggerNewOrderReceived = () => {
  window.dispatchEvent(new CustomEvent('signalr-test', {
    detail: {
      type: 'NewOrderReceived',
      data: {
        OrderId: testOrderId.value,
        UserId: 1,
        TotalAmount: 299.99,
        Timestamp: new Date().toISOString()
      }
    }
  }))
  addApiLog(`🧪 Triggered NewOrderReceived event for order ${testOrderId.value}`, 'warning')
}

const triggerOrderCancelled = () => {
  window.dispatchEvent(new CustomEvent('signalr-test', {
    detail: {
      type: 'OrderCancelled',
      data: {
        OrderId: testOrderId.value,
        UserId: 1,
        Timestamp: new Date().toISOString(),
        Message: `Order #${testOrderId.value} has been cancelled`
      }
    }
  }))
  addApiLog(`🧪 Triggered OrderCancelled event for order ${testOrderId.value}`, 'warning')
}

const getStatusNumber = (status) => {
  const statusMap = {
    'Pending': 0,
    'Processing': 1,
    'Shipped': 2,
    'Delivered': 3,
    'Cancelled': 4
  }
  return statusMap[status] || 0
}

const clearApiLogs = () => {
  apiLogs.value = []
}

onMounted(async () => {
  addApiLog('Admin Order Test component mounted', 'info')
  
  // Join admin dashboard for SignalR
  if (connectionStatus.value.isConnected) {
    await joinAdminDashboard()
    addApiLog('Joined admin dashboard for SignalR notifications', 'success')
  }
})
</script>

<style scoped>
.admin-order-test {
  font-family: 'Inter', sans-serif;
}

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