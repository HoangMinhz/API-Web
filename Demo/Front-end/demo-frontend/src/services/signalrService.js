import * as signalR from '@microsoft/signalr'

class SignalRService {
  constructor() {
    this.connection = null
    this.isConnected = false
    this.reconnectAttempts = 0
    this.maxReconnectAttempts = 5
    this.reconnectInterval = 5000 // 5 seconds
    this.eventHandlers = new Map()
  }

  /**
   * Initialize SignalR connection
   * @param {string} token - JWT token for authentication
   * @param {string} baseUrl - Base URL of the API (default: http://localhost:5000)
   */
  async initialize(token, baseUrl = 'http://localhost:5000') {
    try {
      if (this.connection) {
        await this.disconnect()
      }

      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(`${baseUrl}/orderhub`, {
          accessTokenFactory: () => token,
          // Remove transport restriction to allow automatic negotiation
          // transport: signalR.HttpTransportType.WebSockets,
          // skipNegotiation: true
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: retryContext => {
            if (retryContext.previousRetryCount < this.maxReconnectAttempts) {
              return this.reconnectInterval
            }
            return null // Stop reconnecting
          }
        })
        .configureLogging(signalR.LogLevel.Information)
        .build()

      // Set up connection event handlers
      this.setupConnectionEvents()

      // Set up order-related event handlers
      this.setupOrderEventHandlers()

      await this.connection.start()
      this.isConnected = true
      this.reconnectAttempts = 0
      
      console.log('SignalR Connected successfully')
      this.emit('connected')
      
      return true
    } catch (error) {
      console.error('SignalR Connection failed:', error)
      this.isConnected = false
      this.emit('connectionError', error)
      return false
    }
  }

  /**
   * Set up connection-related event handlers
   */
  setupConnectionEvents() {
    this.connection.onclose(async (error) => {
      this.isConnected = false
      console.log('SignalR Connection closed:', error)
      this.emit('disconnected', error)
    })

    this.connection.onreconnecting((error) => {
      this.isConnected = false
      console.log('SignalR Reconnecting:', error)
      this.emit('reconnecting', error)
    })

    this.connection.onreconnected((connectionId) => {
      this.isConnected = true
      this.reconnectAttempts = 0
      console.log('SignalR Reconnected:', connectionId)
      this.emit('reconnected', connectionId)
    })
  }

  /**
   * Set up order-related event handlers
   */
  setupOrderEventHandlers() {
    // New order created
    this.connection.on('NewOrderCreated', (data) => {
      console.log('🔔 NewOrderCreated received:', data)
      this.emit('newOrderCreated', data)
    })

    // Order status changed
    this.connection.on('OrderStatusChanged', (data) => {
      console.log('🔔 OrderStatusChanged received:', data)
      this.emit('orderStatusChanged', data)
    })

    // Order cancelled
    this.connection.on('OrderCancelled', (data) => {
      console.log('🔔 OrderCancelled received:', data)
      this.emit('orderCancelled', data)
    })

    // New order received (for admin)
    this.connection.on('NewOrderReceived', (data) => {
      console.log('🔔 NewOrderReceived received (admin):', data)
      console.log('🔍 Admin notification data structure:', {
        OrderId: data.OrderId || data.orderId,
        UserId: data.UserId || data.userId, 
        TotalAmount: data.TotalAmount || data.totalAmount,
        Message: data.Message || data.message,
        Timestamp: data.Timestamp || data.timestamp
      })
      this.emit('newOrderReceived', data)
    })

    // Order message received
    this.connection.on('ReceiveOrderMessage', (data) => {
      console.log('🔔 ReceiveOrderMessage received:', data)
      this.emit('orderMessageReceived', data)
    })

    // Order statistics updated (for admin)
    this.connection.on('OrderStatisticsUpdated', (data) => {
      console.log('🔔 OrderStatisticsUpdated received:', data)
      this.emit('orderStatisticsUpdated', data)
    })

    // Admin dashboard joined confirmation
    this.connection.on('AdminDashboardJoined', (data) => {
      console.log('✅ AdminDashboardJoined confirmation received:', data)
      this.emit('adminDashboardJoined', data)
    })
  }

  /**
   * Join user-specific group for order notifications
   * @param {number} userId - User ID
   */
  async joinUserGroup(userId) {
    if (this.isConnected && userId) {
      try {
        // User groups are automatically joined on connection
        console.log(`Joined user group: user-${userId}`)
        return true
      } catch (error) {
        console.error('Error joining user group:', error)
        return false
      }
    }
    return false
  }

  /**
   * Join specific order tracking group
   * @param {number} orderId - Order ID to track
   */
  async joinOrderGroup(orderId) {
    if (this.isConnected && orderId) {
      try {
        await this.connection.invoke('JoinOrderGroup', orderId.toString())
        console.log(`Joined order group: order-${orderId}`)
        return true
      } catch (error) {
        console.error('Error joining order group:', error)
        return false
      }
    }
    return false
  }

  /**
   * Leave specific order tracking group
   * @param {number} orderId - Order ID to stop tracking
   */
  async leaveOrderGroup(orderId) {
    if (this.isConnected && orderId) {
      try {
        await this.connection.invoke('LeaveOrderGroup', orderId.toString())
        console.log(`Left order group: order-${orderId}`)
        return true
      } catch (error) {
        console.error('Error leaving order group:', error)
        return false
      }
    }
    return false
  }

  /**
   * Join admin dashboard (admin only)
   */
  async joinAdminDashboard() {
    if (!this.connection || this.connection.state !== 'Connected') {
      console.error('❌ Cannot join admin dashboard: SignalR not connected')
      console.log('🔍 Connection state:', this.connection?.state)
      console.log('🔍 Is connected:', this.isConnected)
      return false
    }

    try {
      console.log('🔧 Attempting to join admin dashboard...')
      console.log('🔍 Connection ID:', this.connection.connectionId)
      await this.connection.invoke('JoinAdminDashboard')
      console.log('✅ Successfully joined admin dashboard')
      return true
    } catch (error) {
      console.error('❌ Failed to join admin dashboard:', error)
      console.log('🔍 Error details:', error.message)
      return false
    }
  }

  /**
   * Leave admin dashboard (admin only)
   */
  async leaveAdminDashboard() {
    if (this.isConnected) {
      try {
        await this.connection.invoke('LeaveAdminDashboard')
        console.log('Left admin dashboard')
        return true
      } catch (error) {
        console.error('Error leaving admin dashboard:', error)
        return false
      }
    }
    return false
  }

  /**
   * Send message to specific order group (admin only)
   * @param {number} orderId - Order ID
   * @param {string} message - Message to send
   */
  async sendOrderMessage(orderId, message) {
    if (this.isConnected && orderId && message) {
      try {
        await this.connection.invoke('SendOrderMessage', orderId.toString(), message)
        console.log(`Sent message to order ${orderId}:`, message)
        return true
      } catch (error) {
        console.error('Error sending order message:', error)
        return false
      }
    }
    return false
  }

  /**
   * Update order statistics (admin only)
   * @param {object} statistics - Statistics object
   */
  async updateOrderStatistics(statistics) {
    if (this.isConnected && statistics) {
      try {
        await this.connection.invoke('UpdateOrderStatistics', statistics)
        console.log('Updated order statistics:', statistics)
        return true
      } catch (error) {
        console.error('Error updating order statistics:', error)
        return false
      }
    }
    return false
  }

  /**
   * Disconnect from SignalR hub
   */
  async disconnect() {
    if (this.connection) {
      try {
        await this.connection.stop()
        this.isConnected = false
        console.log('SignalR Disconnected')
        this.emit('disconnected')
      } catch (error) {
        console.error('Error disconnecting:', error)
      }
    }
  }

  /**
   * Add event listener
   * @param {string} event - Event name
   * @param {function} handler - Event handler function
   */
  on(event, handler) {
    if (!this.eventHandlers.has(event)) {
      this.eventHandlers.set(event, [])
    }
    this.eventHandlers.get(event).push(handler)
  }

  /**
   * Remove event listener
   * @param {string} event - Event name
   * @param {function} handler - Event handler function to remove
   */
  off(event, handler) {
    if (this.eventHandlers.has(event)) {
      const handlers = this.eventHandlers.get(event)
      const index = handlers.indexOf(handler)
      if (index > -1) {
        handlers.splice(index, 1)
      }
    }
  }

  /**
   * Emit event to all registered handlers
   * @param {string} event - Event name
   * @param {any} data - Event data
   */
  emit(event, data) {
    if (this.eventHandlers.has(event)) {
      this.eventHandlers.get(event).forEach(handler => {
        try {
          handler(data)
        } catch (error) {
          console.error(`Error in event handler for ${event}:`, error)
        }
      })
    }
  }

  /**
   * Get connection state
   */
  getConnectionState() {
    return {
      isConnected: this.isConnected,
      connectionState: this.connection?.state || 'Disconnected',
      connectionId: this.connection?.connectionId || null
    }
  }

  /**
   * Check if connected
   */
  get connected() {
    return this.isConnected
  }
}

// Export singleton instance
export default new SignalRService() 