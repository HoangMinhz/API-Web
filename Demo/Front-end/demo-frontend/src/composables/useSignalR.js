import { ref, reactive, onMounted, onUnmounted, computed } from 'vue'
import signalRService from '@/services/signalrService'

// Storage keys
const NOTIFICATIONS_STORAGE_KEY = 'signalr_notifications'
const ADMIN_NOTIFICATIONS_STORAGE_KEY = 'signalr_admin_notifications'

// Load notifications from localStorage
const loadNotificationsFromStorage = () => {
  try {
    const stored = localStorage.getItem(NOTIFICATIONS_STORAGE_KEY)
    return stored ? JSON.parse(stored) : []
  } catch (error) {
    console.error('Error loading notifications from storage:', error)
    return []
  }
}

const loadAdminNotificationsFromStorage = () => {
  try {
    const stored = localStorage.getItem(ADMIN_NOTIFICATIONS_STORAGE_KEY)
    return stored ? JSON.parse(stored) : []
  } catch (error) {
    console.error('Error loading admin notifications from storage:', error)
    return []
  }
}

// Save notifications to localStorage
const saveNotificationsToStorage = (notifications) => {
  try {
    localStorage.setItem(NOTIFICATIONS_STORAGE_KEY, JSON.stringify(notifications))
  } catch (error) {
    console.error('Error saving notifications to storage:', error)
  }
}

const saveAdminNotificationsToStorage = (adminNotifications) => {
  try {
    localStorage.setItem(ADMIN_NOTIFICATIONS_STORAGE_KEY, JSON.stringify(adminNotifications))
  } catch (error) {
    console.error('Error saving admin notifications to storage:', error)
  }
}

export function useSignalR() {
  // Reactive state - load from localStorage
  const isConnected = ref(false)
  const connectionState = ref('Disconnected')
  const connectionId = ref(null)
  const notifications = reactive(loadNotificationsFromStorage())
  const orderUpdates = reactive(new Map())
  const adminNotifications = reactive(loadAdminNotificationsFromStorage())

  // Connection status computed
  const connectionStatus = computed(() => ({
    isConnected: isConnected.value,
    state: connectionState.value,
    id: connectionId.value
  }))

  /**
   * Initialize SignalR connection
   * @param {string} token - JWT token
   * @param {string} baseUrl - API base URL
   */
  const connect = async (token, baseUrl) => {
    try {
      const success = await signalRService.initialize(token, baseUrl)
      updateConnectionState()
      return success
    } catch (error) {
      console.error('Failed to connect to SignalR:', error)
      return false
    }
  }

  /**
   * Disconnect from SignalR
   */
  const disconnect = async () => {
    try {
      await signalRService.disconnect()
      updateConnectionState()
    } catch (error) {
      console.error('Failed to disconnect from SignalR:', error)
    }
  }

  /**
   * Update connection state
   */
  const updateConnectionState = () => {
    const state = signalRService.getConnectionState()
    isConnected.value = state.isConnected
    connectionState.value = state.connectionState
    connectionId.value = state.connectionId
  }

  /**
   * Add notification to the list
   * @param {object} notification - Notification object
   * @param {string} type - Notification type
   */
  const addNotification = (notification, type = 'info') => {
    const notificationWithId = {
      id: Date.now() + Math.random(),
      ...notification,
      type,
      timestamp: notification.timestamp || new Date().toISOString(),
      read: false
    }
    
    notifications.unshift(notificationWithId)
    
    // Keep only last 50 notifications
    if (notifications.length > 50) {
      notifications.splice(50)
    }
    
    // Save to localStorage
    saveNotificationsToStorage(notifications)
  }

  /**
   * Mark notification as read
   * @param {string|number} notificationId - Notification ID
   */
  const markNotificationAsRead = (notificationId) => {
    const notification = notifications.find(n => n.id === notificationId)
    if (notification) {
      notification.read = true
      // Save to localStorage
      saveNotificationsToStorage(notifications)
    }
  }

  /**
   * Clear all notifications
   */
  const clearNotifications = () => {
    notifications.splice(0)
    // Save to localStorage
    saveNotificationsToStorage(notifications)
  }

  /**
   * Get unread notifications count
   */
  const unreadCount = computed(() => {
    return notifications.filter(n => !n.read).length
  })

  /**
   * Join order tracking group
   * @param {number} orderId - Order ID
   */
  const trackOrder = async (orderId) => {
    return await signalRService.joinOrderGroup(orderId)
  }

  /**
   * Stop tracking order
   * @param {number} orderId - Order ID
   */
  const stopTrackingOrder = async (orderId) => {
    return await signalRService.leaveOrderGroup(orderId)
  }

  /**
   * Join admin dashboard (admin only)
   */
  const joinAdminDashboard = async () => {
    return await signalRService.joinAdminDashboard()
  }

  /**
   * Leave admin dashboard (admin only)
   */
  const leaveAdminDashboard = async () => {
    return await signalRService.leaveAdminDashboard()
  }

  /**
   * Send message to order group (admin only)
   * @param {number} orderId - Order ID
   * @param {string} message - Message
   */
  const sendOrderMessage = async (orderId, message) => {
    return await signalRService.sendOrderMessage(orderId, message)
  }

  // Set up event handlers
  const setupEventHandlers = () => {
    // Connection events
    signalRService.on('connected', () => {
      updateConnectionState()
    })

    signalRService.on('disconnected', (error) => {
      updateConnectionState()
    })

    signalRService.on('reconnecting', () => {
      updateConnectionState()
    })

    signalRService.on('reconnected', () => {
      updateConnectionState()
    })

    // Order events
    signalRService.on('newOrderCreated', (data) => {
      const eventData = {
        OrderId: data.OrderId || data.orderId,
        TotalAmount: data.TotalAmount || data.totalAmount,
        Message: data.Message || data.message,
        Timestamp: data.Timestamp || data.timestamp || new Date().toISOString()
      }
      
      addNotification({
        title: 'Order Created',
        message: eventData.Message || `Your order #${eventData.OrderId} has been created successfully!`,
        orderId: eventData.OrderId,
        type: 'success'
      })
      
      orderUpdates.set(eventData.OrderId, {
        ...eventData,
        type: 'created'
      })
      
      window.dispatchEvent(new CustomEvent('signalr-new-order-created', {
        detail: eventData
      }))
    })

    signalRService.on('orderStatusChanged', (data) => {
      const eventData = {
        OrderId: data.OrderId || data.orderId,
        OldStatus: data.OldStatus || data.oldStatus,
        NewStatus: data.NewStatus || data.newStatus,
        Message: data.Message || data.message,
        Timestamp: data.Timestamp || data.timestamp || new Date().toISOString()
      }
      
      addNotification({
        title: 'Order Status Updated',
        message: eventData.Message || `Your order #${eventData.OrderId} status changed to ${eventData.NewStatus}`,
        orderId: eventData.OrderId,
        type: 'info'
      })
      
      orderUpdates.set(eventData.OrderId, {
        ...eventData,
        type: 'statusChanged'
      })
      
      window.dispatchEvent(new CustomEvent('signalr-order-status-changed', {
        detail: eventData
      }))
    })

    signalRService.on('orderCancelled', (data) => {
      const eventData = {
        OrderId: data.OrderId || data.orderId,
        UserId: data.UserId || data.userId,
        Message: data.Message || data.message,
        Timestamp: data.Timestamp || data.timestamp || new Date().toISOString()
      }
      
      addNotification({
        title: 'Order Cancelled',
        message: eventData.Message || `Your order #${eventData.OrderId} has been cancelled`,
        orderId: eventData.OrderId,
        type: 'warning'
      })
      
      orderUpdates.set(eventData.OrderId, {
        ...eventData,
        type: 'cancelled'
      })
      
      window.dispatchEvent(new CustomEvent('signalr-order-cancelled', {
        detail: eventData
      }))
    })

    // Admin events
    signalRService.on('newOrderReceived', (data) => {
      // Handle both uppercase and lowercase property names
      const orderId = data.OrderId || data.orderId
      const userId = data.UserId || data.userId
      const totalAmount = data.TotalAmount || data.totalAmount
      const timestamp = data.Timestamp || data.timestamp
      const message = data.Message || data.message
      
      const adminNotification = {
        id: Date.now() + Math.random(),
        title: 'New Order Received',
        message: `New order #${orderId} from user ${userId} - Total: ${totalAmount ? new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(totalAmount) : 'N/A'}`,
        orderId: orderId,
        userId: userId,
        totalAmount: totalAmount,
        timestamp: timestamp || new Date().toISOString(),
        type: 'info',
        read: false
      }
      
      adminNotifications.unshift(adminNotification)
      
      addNotification({
        title: 'New Order Received',
        message: `New order #${orderId} from user ${userId}`,
        orderId: orderId,
        type: 'info'
      })
      
      if (adminNotifications.length > 100) {
        adminNotifications.splice(100)
      }
      
      saveAdminNotificationsToStorage(adminNotifications)
    })

    signalRService.on('orderMessageReceived', (data) => {
      addNotification({
        title: 'Order Message',
        message: `Message from ${data.Sender}: ${data.Message}`,
        orderId: data.OrderId,
        type: 'info'
      })
    })
  }

  /**
   * Get order updates for specific order
   * @param {number} orderId - Order ID
   */
  const getOrderUpdates = (orderId) => {
    return orderUpdates.get(orderId) || null
  }

  /**
   * Clear order updates for specific order
   * @param {number} orderId - Order ID
   */
  const clearOrderUpdates = (orderId) => {
    orderUpdates.delete(orderId)
  }

  /**
   * Get admin notifications count
   */
  const adminUnreadCount = computed(() => {
    return adminNotifications.filter(n => !n.read).length
  })

  /**
   * Mark admin notification as read
   * @param {string|number} notificationId - Notification ID
   */
  const markAdminNotificationAsRead = (notificationId) => {
    const notification = adminNotifications.find(n => n.id === notificationId)
    if (notification) {
      notification.read = true
      // Save to localStorage
      saveAdminNotificationsToStorage(adminNotifications)
    }
  }

  /**
   * Clear admin notifications
   */
  const clearAdminNotifications = () => {
    adminNotifications.splice(0)
    // Save to localStorage
    saveAdminNotificationsToStorage(adminNotifications)
  }

  // Setup event handlers when composable is used
  setupEventHandlers()

  return {
    // State
    isConnected,
    connectionState,
    connectionId,
    connectionStatus,
    notifications,
    orderUpdates,
    adminNotifications,
    unreadCount,
    adminUnreadCount,

    // Methods
    connect,
    disconnect,
    trackOrder,
    stopTrackingOrder,
    joinAdminDashboard,
    leaveAdminDashboard,
    sendOrderMessage,
    addNotification,
    markNotificationAsRead,
    clearNotifications,
    getOrderUpdates,
    clearOrderUpdates,
    markAdminNotificationAsRead,
    clearAdminNotifications,

    // Utilities
    updateConnectionState
  }
}

// Global composable instance for app-wide usage
let globalSignalRInstance = null

export function useGlobalSignalR() {
  if (!globalSignalRInstance) {
    globalSignalRInstance = useSignalR()
  }
  return globalSignalRInstance
} 