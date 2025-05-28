using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Security.Claims;
using Demo.Models;

namespace Demo.Hubs
{
    [Authorize]
    public class OrderHub : Hub
    {
        /// <summary>
        /// Called when a client connects to the hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            
            if (!string.IsNullOrEmpty(userId))
            {
                // Add user to their personal group for order notifications
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
                
                // Add admin users to admin group for all order notifications
                if (userRole == "Admin")
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "admin-orders");
                }
            }
            
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when a client disconnects from the hub
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            
            if (!string.IsNullOrEmpty(userId))
            {
                // Remove user from their personal group
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
                
                // Remove admin users from admin group
                if (userRole == "Admin")
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admin-orders");
                }
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Join a specific order tracking group (for real-time order updates)
        /// </summary>
        /// <param name="orderId">The order ID to track</param>
        public async Task JoinOrderGroup(string orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");
        }

        /// <summary>
        /// Leave a specific order tracking group
        /// </summary>
        /// <param name="orderId">The order ID to stop tracking</param>
        public async Task LeaveOrderGroup(string orderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order-{orderId}");
        }

        /// <summary>
        /// Join admin dashboard for real-time order management
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task JoinAdminDashboard()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            
            Console.WriteLine($"🔧 JoinAdminDashboard called by user {userName} (ID: {userId}, Role: {userRole}) with connection {Context.ConnectionId}");
            
            await Groups.AddToGroupAsync(Context.ConnectionId, "admin-dashboard");
            
            // Log the successful join
            Console.WriteLine($"✅ Admin user {userName} (ID: {userId}) joined admin-dashboard group with connection {Context.ConnectionId}");
            
            // Send a test message to confirm the connection
            await Clients.Caller.SendAsync("AdminDashboardJoined", new
            {
                Message = "Successfully joined admin dashboard",
                Timestamp = DateTime.UtcNow,
                UserId = userId,
                UserName = userName
            });
        }

        /// <summary>
        /// Leave admin dashboard
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task LeaveAdminDashboard()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admin-dashboard");
        }

        /// <summary>
        /// Send a message to a specific order group (for customer service)
        /// </summary>
        /// <param name="orderId">The order ID</param>
        /// <param name="message">The message to send</param>
        [Authorize(Roles = "Admin")]
        public async Task SendOrderMessage(string orderId, string message)
        {
            var senderName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Admin";
            
            await Clients.Group($"order-{orderId}").SendAsync("ReceiveOrderMessage", new
            {
                OrderId = orderId,
                Message = message,
                Sender = senderName,
                Timestamp = DateTime.UtcNow,
                Type = "admin_message"
            });
        }

        /// <summary>
        /// Notify about order status change
        /// </summary>
        /// <param name="orderId">The order ID</param>
        /// <param name="oldStatus">Previous status</param>
        /// <param name="newStatus">New status</param>
        /// <param name="userId">User ID who owns the order</param>
        public async Task NotifyOrderStatusChange(int orderId, OrderStatus oldStatus, OrderStatus newStatus, int userId)
        {
            var statusUpdate = new
            {
                OrderId = orderId,
                OldStatus = oldStatus.ToString(),
                NewStatus = newStatus.ToString(),
                Timestamp = DateTime.UtcNow,
                Message = GetStatusChangeMessage(oldStatus, newStatus)
            };

            // Notify the specific user
            await Clients.Group($"user-{userId}").SendAsync("OrderStatusChanged", statusUpdate);
            
            // Notify specific order group
            await Clients.Group($"order-{orderId}").SendAsync("OrderStatusChanged", statusUpdate);
            
            // Notify admin dashboard
            await Clients.Group("admin-dashboard").SendAsync("OrderStatusChanged", statusUpdate);
        }

        /// <summary>
        /// Notify about new order creation
        /// </summary>
        /// <param name="orderId">The new order ID</param>
        /// <param name="userId">User ID who created the order</param>
        /// <param name="totalAmount">Order total amount</param>
        public async Task NotifyNewOrder(int orderId, int userId, decimal totalAmount)
        {
            var newOrderNotification = new
            {
                OrderId = orderId,
                UserId = userId,
                TotalAmount = totalAmount,
                Timestamp = DateTime.UtcNow,
                Message = $"New order #{orderId} created with total ${totalAmount:F2}"
            };

            // Notify the user who created the order
            await Clients.Group($"user-{userId}").SendAsync("NewOrderCreated", newOrderNotification);
            
            // Notify admin dashboard about new order
            await Clients.Group("admin-dashboard").SendAsync("NewOrderReceived", newOrderNotification);
            
            // Notify admin group
            await Clients.Group("admin-orders").SendAsync("NewOrderReceived", newOrderNotification);
        }

        /// <summary>
        /// Notify about order cancellation
        /// </summary>
        /// <param name="orderId">The cancelled order ID</param>
        /// <param name="userId">User ID who owns the order</param>
        /// <param name="reason">Cancellation reason</param>
        public async Task NotifyOrderCancellation(int orderId, int userId, string reason = null)
        {
            var cancellationNotification = new
            {
                OrderId = orderId,
                UserId = userId,
                Reason = reason,
                Timestamp = DateTime.UtcNow,
                Message = $"Order #{orderId} has been cancelled" + (string.IsNullOrEmpty(reason) ? "" : $": {reason}")
            };

            // Notify the user
            await Clients.Group($"user-{userId}").SendAsync("OrderCancelled", cancellationNotification);
            
            // Notify specific order group
            await Clients.Group($"order-{orderId}").SendAsync("OrderCancelled", cancellationNotification);
            
            // Notify admin dashboard
            await Clients.Group("admin-dashboard").SendAsync("OrderCancelled", cancellationNotification);
        }

        /// <summary>
        /// Send real-time order statistics update to admin dashboard
        /// </summary>
        /// <param name="statistics">Order statistics object</param>
        [Authorize(Roles = "Admin")]
        public async Task UpdateOrderStatistics(object statistics)
        {
            await Clients.Group("admin-dashboard").SendAsync("OrderStatisticsUpdated", new
            {
                Statistics = statistics,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Get user-friendly status change message
        /// </summary>
        /// <param name="oldStatus">Previous status</param>
        /// <param name="newStatus">New status</param>
        /// <returns>User-friendly message</returns>
        private string GetStatusChangeMessage(OrderStatus oldStatus, OrderStatus newStatus)
        {
            return newStatus switch
            {
                OrderStatus.Pending => "Your order is now pending confirmation.",
                OrderStatus.Processing => "Your order is currently being processed.",
                OrderStatus.Shipped => "Your order has been shipped and is on its way to you!",
                OrderStatus.Delivered => "Your order has been delivered successfully!",
                OrderStatus.Cancelled => "Your order has been cancelled.",
                _ => $"Your order status has been updated from {oldStatus} to {newStatus}."
            };
        }
    }
}