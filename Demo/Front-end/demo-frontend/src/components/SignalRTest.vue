<template>
  <div class="signalr-test bg-white rounded-lg shadow p-6 mb-6">
    <h2 class="text-xl font-semibold mb-4">SignalR Connection Test</h2>
    
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
          Status: {{ connectionStatus.isConnected ? 'Connected' : 'Disconnected' }}
        </span>
        <span class="text-sm text-gray-500">
          ({{ connectionStatus.state }})
        </span>
      </div>
      <div v-if="connectionStatus.id" class="text-xs text-gray-500 mt-1">
        Connection ID: {{ connectionStatus.id }}
      </div>
    </div>

    <!-- Manual Connection Controls -->
    <div class="mb-4 space-x-2">
      <button
        @click="manualConnect"
        :disabled="isConnecting"
        class="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50"
      >
        {{ isConnecting ? 'Connecting...' : 'Connect' }}
      </button>
      <button
        @click="manualDisconnect"
        :disabled="!connectionStatus.isConnected"
        class="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700 disabled:opacity-50"
      >
        Disconnect
      </button>
      <button
        @click="testNotification"
        class="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700"
      >
        Test Notification
      </button>
    </div>

    <!-- Test Order Events -->
    <div class="mb-4">
      <h3 class="font-medium mb-2">Test Order Events:</h3>
      <div class="space-x-2">
        <button
          @click="simulateOrderCreated"
          class="px-3 py-1 bg-purple-600 text-white rounded text-sm hover:bg-purple-700"
        >
          Simulate Order Created
        </button>
        <button
          @click="simulateStatusChange"
          class="px-3 py-1 bg-orange-600 text-white rounded text-sm hover:bg-orange-700"
        >
          Simulate Status Change
        </button>
        <button
          @click="simulateOrderCancelled"
          class="px-3 py-1 bg-red-600 text-white rounded text-sm hover:bg-red-700"
        >
          Simulate Order Cancelled
        </button>
      </div>
    </div>

    <!-- Connection Logs -->
    <div class="mb-4">
      <h3 class="font-medium mb-2">Connection Logs:</h3>
      <div class="bg-gray-100 p-3 rounded max-h-40 overflow-y-auto text-sm">
        <div v-if="logs.length === 0" class="text-gray-500">No logs yet...</div>
        <div v-for="log in logs" :key="log.id" class="mb-1">
          <span class="text-gray-500">{{ log.timestamp }}</span>
          <span :class="getLogClass(log.type)">{{ log.message }}</span>
        </div>
      </div>
      <button
        @click="clearLogs"
        class="mt-2 px-3 py-1 bg-gray-600 text-white rounded text-sm hover:bg-gray-700"
      >
        Clear Logs
      </button>
    </div>

    <!-- Current Token -->
    <div class="text-xs text-gray-500">
      <strong>Current Token:</strong> {{ currentToken ? 'Available' : 'Not available' }}
      <br>
      <strong>Backend URL:</strong> http://localhost:5285/orderhub
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { useGlobalSignalR } from '@/composables/useSignalR'

const {
  connectionStatus,
  connect,
  disconnect,
  addNotification
} = useGlobalSignalR()

const isConnecting = ref(false)
const logs = ref([])
const currentToken = ref(localStorage.getItem('token'))

const addLog = (message, type = 'info') => {
  logs.value.unshift({
    id: Date.now(),
    timestamp: new Date().toLocaleTimeString(),
    message,
    type
  })
  
  // Keep only last 20 logs
  if (logs.value.length > 20) {
    logs.value.splice(20)
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

const manualConnect = async () => {
  if (isConnecting.value) return
  
  isConnecting.value = true
  addLog('Attempting to connect...', 'info')
  
  try {
    const token = localStorage.getItem('token')
    if (!token) {
      addLog('No token found in localStorage', 'error')
      return
    }
    
    const success = await connect(token, 'http://localhost:5285')
    if (success) {
      addLog('Connected successfully!', 'success')
    } else {
      addLog('Connection failed', 'error')
    }
  } catch (error) {
    addLog(`Connection error: ${error.message}`, 'error')
  } finally {
    isConnecting.value = false
  }
}

const manualDisconnect = async () => {
  addLog('Disconnecting...', 'info')
  try {
    await disconnect()
    addLog('Disconnected successfully', 'success')
  } catch (error) {
    addLog(`Disconnect error: ${error.message}`, 'error')
  }
}

const testNotification = () => {
  addNotification({
    title: 'Test Notification',
    message: 'This is a test notification to verify the system is working',
    type: 'info'
  })
  addLog('Test notification added', 'success')
}

const simulateOrderCreated = () => {
  addNotification({
    title: 'Order Created',
    message: 'Your order #12345 has been created successfully!',
    orderId: 12345,
    type: 'success'
  })
  addLog('Simulated order created notification', 'success')
}

const simulateStatusChange = () => {
  addNotification({
    title: 'Order Status Updated',
    message: 'Order #12345 status changed to Processing',
    orderId: 12345,
    type: 'info'
  })
  addLog('Simulated status change notification', 'success')
}

const simulateOrderCancelled = () => {
  addNotification({
    title: 'Order Cancelled',
    message: 'Order #12345 has been cancelled',
    orderId: 12345,
    type: 'warning'
  })
  addLog('Simulated order cancelled notification', 'success')
}

const clearLogs = () => {
  logs.value = []
}

// Log connection state changes
let lastConnectionState = connectionStatus.value.isConnected
setInterval(() => {
  if (connectionStatus.value.isConnected !== lastConnectionState) {
    lastConnectionState = connectionStatus.value.isConnected
    if (connectionStatus.value.isConnected) {
      addLog('Connection established', 'success')
    } else {
      addLog('Connection lost', 'warning')
    }
  }
}, 1000)

onMounted(() => {
  addLog('SignalR Test component mounted', 'info')
  currentToken.value = localStorage.getItem('token')
  
  if (currentToken.value) {
    addLog('Token found, auto-connect should work', 'info')
  } else {
    addLog('No token found, please login first', 'warning')
  }
})

onUnmounted(() => {
  addLog('SignalR Test component unmounted', 'info')
})
</script>

<style scoped>
.signalr-test {
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