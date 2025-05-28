import { useGlobalSignalR } from '@/composables/useSignalR'

export default {
  install(app, options = {}) {
    const signalR = useGlobalSignalR()
    
    // Default configuration
    const config = {
      baseUrl: 'http://localhost:5285',
      autoConnect: true,
      reconnectOnAuth: true,
      ...options
    }

    // Make SignalR available globally
    app.config.globalProperties.$signalR = signalR
    app.provide('signalR', signalR)

    // Auto-connect if token is available and autoConnect is enabled
    if (config.autoConnect) {
      const token = localStorage.getItem('token')
      if (token) {
        signalR.connect(token, config.baseUrl).then(success => {
          if (success) {
            console.log('SignalR auto-connected successfully')
          } else {
            console.warn('SignalR auto-connection failed')
          }
        })
      }
    }

    // Listen for authentication changes
    if (config.reconnectOnAuth) {
      // Watch for token changes in localStorage
      let lastToken = localStorage.getItem('token')
      
      setInterval(() => {
        const currentToken = localStorage.getItem('token')
        
        if (currentToken !== lastToken) {
          lastToken = currentToken
          
          if (currentToken) {
            // Token added or changed - connect/reconnect
            signalR.connect(currentToken, config.baseUrl).then(success => {
              if (success) {
                console.log('SignalR reconnected after auth change')
              }
            })
          } else {
            // Token removed - disconnect
            signalR.disconnect().then(() => {
              console.log('SignalR disconnected after logout')
            })
          }
        }
      }, 1000) // Check every second
    }

    // Expose methods for manual connection management
    app.config.globalProperties.$connectSignalR = async (token, baseUrl = config.baseUrl) => {
      return await signalR.connect(token, baseUrl)
    }

    app.config.globalProperties.$disconnectSignalR = async () => {
      return await signalR.disconnect()
    }

    // Helper method to get user role and join appropriate groups
    app.config.globalProperties.$joinUserGroups = async (userRole, userId) => {
      if (signalR.isConnected.value) {
        // Join user-specific group (automatically done on connection)
        await signalR.joinUserGroup(userId)
        
        // Join admin groups if user is admin
        if (userRole === 'Admin') {
          await signalR.joinAdminDashboard()
        }
      }
    }

    // Helper method to leave user groups
    app.config.globalProperties.$leaveUserGroups = async (userRole) => {
      if (signalR.isConnected.value) {
        // Leave admin groups if user was admin
        if (userRole === 'Admin') {
          await signalR.leaveAdminDashboard()
        }
      }
    }
  }
}

// Composable for easy access in components
export function useSignalRPlugin() {
  const signalR = useGlobalSignalR()
  
  return {
    ...signalR,
    
    // Helper methods
    async connectWithAuth(token, baseUrl = 'http://localhost:5285') {
      const success = await signalR.connect(token, baseUrl)
      if (success) {
        // Store connection info
        localStorage.setItem('signalr_connected', 'true')
      }
      return success
    },

    async disconnectAndCleanup() {
      await signalR.disconnect()
      localStorage.removeItem('signalr_connected')
    },

    async joinUserGroups(userRole, userId) {
      if (signalR.isConnected.value) {
        await signalR.joinUserGroup(userId)
        
        if (userRole === 'Admin') {
          await signalR.joinAdminDashboard()
        }
      }
    },

    async leaveUserGroups(userRole) {
      if (signalR.isConnected.value && userRole === 'Admin') {
        await signalR.leaveAdminDashboard()
      }
    },

    // Check if should auto-connect
    shouldAutoConnect() {
      const token = localStorage.getItem('token')
      const wasConnected = localStorage.getItem('signalr_connected')
      return token && wasConnected
    }
  }
} 