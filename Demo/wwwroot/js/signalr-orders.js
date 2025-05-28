// SignalR Order Management Integration
// This file demonstrates how to connect to the OrderHub and handle real-time notifications

class OrderSignalRService {
    constructor() {
        this.connection = null;
        this.isConnected = false;
        this.reconnectAttempts = 0;
        this.maxReconnectAttempts = 5;
    }

    // Initialize SignalR connection
    async initialize(accessToken) {
        try {
            // Create connection with JWT token
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/orderHub", {
                    accessTokenFactory: () => accessToken,
                    transport: signalR.HttpTransportType.WebSockets
                })
                .withAutomaticReconnect([0, 2000, 10000, 30000])
                .configureLogging(signalR.LogLevel.Information)
                .build();

            // Set up event handlers
            this.setupEventHandlers();

            // Start connection
            await this.connection.start();
            this.isConnected = true;
            this.reconnectAttempts = 0;
            
            console.log("SignalR Connected to OrderHub");
            
            // Join user-specific group automatically
            await this.joinUserGroup();
            
            return true;
        } catch (error) {
            console.error("SignalR Connection Error:", error);
            this.isConnected = false;
            return false;
        }
    }

    // Set up event handlers for different order notifications
    setupEventHandlers() {
        // Handle new order creation notifications
        this.connection.on("NewOrderCreated", (notification) => {
            console.log("New Order Created:", notification);
            this.showNotification("success", notification.Message);
            this.updateOrdersList();
        });

        // Handle order status change notifications
        this.connection.on("OrderStatusChanged", (notification) => {
            console.log("Order Status Changed:", notification);
            this.showNotification("info", notification.Message);
            this.updateOrderStatus(notification.OrderId, notification.NewStatus);
        });

        // Handle order cancellation notifications
        this.connection.on("OrderCancelled", (notification) => {
            console.log("Order Cancelled:", notification);
            this.showNotification("warning", notification.Message);
            this.updateOrderStatus(notification.OrderId, "Cancelled");
        });

        // Admin-specific notifications
        this.connection.on("NewOrderReceived", (notification) => {
            console.log("New Order Received (Admin):", notification);
            this.showAdminNotification("info", notification.Message);
            this.updateAdminDashboard();
        });

        // Handle admin messages
        this.connection.on("ReceiveOrderMessage", (message) => {
            console.log("Order Message Received:", message);
            this.showOrderMessage(message);
        });

        // Handle order statistics updates
        this.connection.on("OrderStatisticsUpdated", (data) => {
            console.log("Order Statistics Updated:", data);
            this.updateOrderStatistics(data.Statistics);
        });

        // Connection state handlers
        this.connection.onreconnecting((error) => {
            console.log("SignalR Reconnecting:", error);
            this.isConnected = false;
            this.showNotification("warning", "Connection lost. Reconnecting...");
        });

        this.connection.onreconnected((connectionId) => {
            console.log("SignalR Reconnected:", connectionId);
            this.isConnected = true;
            this.showNotification("success", "Connection restored!");
            this.joinUserGroup();
        });

        this.connection.onclose((error) => {
            console.log("SignalR Connection Closed:", error);
            this.isConnected = false;
            this.handleConnectionClosed();
        });
    }

    // Join user-specific group for notifications
    async joinUserGroup() {
        if (this.isConnected) {
            try {
                // User groups are automatically joined on connection
                console.log("Joined user notification group");
            } catch (error) {
                console.error("Error joining user group:", error);
            }
        }
    }

    // Join specific order tracking group
    async joinOrderGroup(orderId) {
        if (this.isConnected) {
            try {
                await this.connection.invoke("JoinOrderGroup", orderId.toString());
                console.log(`Joined order group for order ${orderId}`);
            } catch (error) {
                console.error("Error joining order group:", error);
            }
        }
    }

    // Leave specific order tracking group
    async leaveOrderGroup(orderId) {
        if (this.isConnected) {
            try {
                await this.connection.invoke("LeaveOrderGroup", orderId.toString());
                console.log(`Left order group for order ${orderId}`);
            } catch (error) {
                console.error("Error leaving order group:", error);
            }
        }
    }

    // Admin: Join admin dashboard
    async joinAdminDashboard() {
        if (this.isConnected) {
            try {
                await this.connection.invoke("JoinAdminDashboard");
                console.log("Joined admin dashboard");
            } catch (error) {
                console.error("Error joining admin dashboard:", error);
            }
        }
    }

    // Admin: Send message to order group
    async sendOrderMessage(orderId, message) {
        if (this.isConnected) {
            try {
                await this.connection.invoke("SendOrderMessage", orderId.toString(), message);
                console.log(`Message sent to order ${orderId}`);
            } catch (error) {
                console.error("Error sending order message:", error);
            }
        }
    }

    // Disconnect from SignalR
    async disconnect() {
        if (this.connection) {
            try {
                await this.connection.stop();
                this.isConnected = false;
                console.log("SignalR Disconnected");
            } catch (error) {
                console.error("Error disconnecting:", error);
            }
        }
    }

    // Handle connection closed
    handleConnectionClosed() {
        if (this.reconnectAttempts < this.maxReconnectAttempts) {
            this.reconnectAttempts++;
            setTimeout(() => {
                this.showNotification("info", `Attempting to reconnect... (${this.reconnectAttempts}/${this.maxReconnectAttempts})`);
            }, 2000 * this.reconnectAttempts);
        } else {
            this.showNotification("error", "Connection lost. Please refresh the page.");
        }
    }

    // UI Helper Methods
    showNotification(type, message) {
        // This should be implemented based on your UI framework
        console.log(`[${type.toUpperCase()}] ${message}`);
        
        // Example implementation with toast notifications
        if (typeof window !== 'undefined' && window.showToast) {
            window.showToast(type, message);
        }
    }

    showAdminNotification(type, message) {
        // Admin-specific notifications
        this.showNotification(type, `[ADMIN] ${message}`);
    }

    showOrderMessage(message) {
        // Display order-specific messages
        console.log(`Order Message: ${message.Message} from ${message.Sender}`);
    }

    updateOrdersList() {
        // Refresh the orders list in the UI
        if (typeof window !== 'undefined' && window.refreshOrders) {
            window.refreshOrders();
        }
    }

    updateOrderStatus(orderId, newStatus) {
        // Update specific order status in the UI
        console.log(`Updating order ${orderId} status to ${newStatus}`);
        
        // Example: Update DOM element
        const orderElement = document.querySelector(`[data-order-id="${orderId}"]`);
        if (orderElement) {
            const statusElement = orderElement.querySelector('.order-status');
            if (statusElement) {
                statusElement.textContent = newStatus;
                statusElement.className = `order-status status-${newStatus.toLowerCase()}`;
            }
        }
    }

    updateAdminDashboard() {
        // Refresh admin dashboard
        if (typeof window !== 'undefined' && window.refreshAdminDashboard) {
            window.refreshAdminDashboard();
        }
    }

    updateOrderStatistics(statistics) {
        // Update order statistics in admin dashboard
        console.log("Updating order statistics:", statistics);
    }
}

// Usage Example:
/*
// Initialize SignalR service
const orderSignalR = new OrderSignalRService();

// Connect with JWT token
const token = localStorage.getItem('authToken');
orderSignalR.initialize(token).then(connected => {
    if (connected) {
        console.log('Connected to order notifications');
        
        // For order detail page, join specific order group
        const orderId = getCurrentOrderId();
        if (orderId) {
            orderSignalR.joinOrderGroup(orderId);
        }
        
        // For admin users, join admin dashboard
        if (isAdmin()) {
            orderSignalR.joinAdminDashboard();
        }
    }
});

// Cleanup on page unload
window.addEventListener('beforeunload', () => {
    orderSignalR.disconnect();
});
*/

// Export for use in modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = OrderSignalRService;
}

// Global access
if (typeof window !== 'undefined') {
    window.OrderSignalRService = OrderSignalRService;
} 