using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Demo.Models;
using System.Security.Claims;
using System.Linq;
using Demo.Data;
using System.ComponentModel.DataAnnotations;
using Demo.Models.ViewModel;
using Microsoft.AspNetCore.Identity;

namespace Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderController> _logger;
        private readonly UserManager<AppUser> _userManager;

        public OrderController(AppDbContext context, ILogger<OrderController> logger, UserManager<AppUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        private int? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return null;

            if (!int.TryParse(userIdClaim, out int userId))
                return null;

            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                _logger.LogInformation("Getting orders - Starting");

                var userId = GetUserId();
                if (!userId.HasValue)
                {
                    _logger.LogWarning("GetOrders - User ID not found in claims");
                    return Unauthorized();
                }

                _logger.LogInformation("Getting orders for user {UserId}", userId.Value);

                // First check if user exists
                var user = await _context.Users.FindAsync(userId.Value);
                if (user == null)
                {
                    _logger.LogWarning("GetOrders - User not found in database: {UserId}", userId.Value);
                    return NotFound("User not found");
                }

                // Get orders with minimal includes and prevent circular references
                var orders = await _context.Orders
                    .AsNoTracking()
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Where(o => o.UserId == userId.Value)
                    .Select(o => new OrderDto
                    {
                        Id = o.Id,
                        OrderDate = o.OrderDate,
                        Status = o.Status,
                        TotalAmount = o.TotalAmount,
                        ShippingAddress = o.ShippingAddress,
                        PhoneNumber = o.PhoneNumber,
                        FullName = o.FullName,
                        Notes = o.Notes,
                        OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                        {
                            Id = oi.Id,
                            Quantity = oi.Quantity,
                            UnitPrice = oi.UnitPrice,
                            TotalPrice = oi.TotalPrice,
                            Product = new ProductDto
                            {
                                Id = oi.Product.Id,
                                Name = oi.Product.Name,
                                ImageUrl = oi.Product.ImageUrl
                            }
                        }).ToList()
                    })
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} orders for user {UserId}", orders.Count, userId.Value);

                // Log the first order's details for debugging
                if (orders.Any())
                {
                    var firstOrder = orders.First();
                    _logger.LogInformation("First order details: Id={Id}, TotalAmount={TotalAmount}, Items={Items}, OrderItemsTotal={OrderItemsTotal}", 
                        firstOrder.Id, 
                        firstOrder.TotalAmount,
                        firstOrder.OrderItems.Count,
                        firstOrder.OrderItems.Sum(oi => oi.TotalPrice));
                }

                return Ok(orders);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation while getting orders for user {UserId}. Error: {Error}", GetUserId(), ex.ToString());
                return StatusCode(500, new { message = "Database operation error", error = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error while getting orders for user {UserId}. Error: {Error}", GetUserId(), ex.ToString());
                return StatusCode(500, new { message = "Database error", error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting orders for user {UserId}. Error: {Error}", GetUserId(), ex.ToString());
                return StatusCode(500, new { message = "An unexpected error occurred", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            try
            {
                var userId = GetUserId();
                if (!userId.HasValue)
                    return Unauthorized();

                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId.Value);

                if (order == null)
                    return NotFound();

                var orderDto = new OrderDto
                {
                    Id = order.Id,
                    OrderDate = order.OrderDate,
                    Status = order.Status,
                    TotalAmount = order.TotalAmount,
                    ShippingAddress = order.ShippingAddress,
                    PhoneNumber = order.PhoneNumber,
                    FullName = order.FullName,
                    Notes = order.Notes,
                    OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                    {
                        Id = oi.Id,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice,
                        Product = new ProductDto
                        {
                            Id = oi.Product.Id,
                            Name = oi.Product.Name,
                            ImageUrl = oi.Product.ImageUrl
                        }
                    }).ToList()
                };

                return Ok(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order {OrderId}", id);
                return StatusCode(500, "An error occurred while getting the order");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderModel model)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var user = await _userManager.FindByIdAsync(userId.ToString());
                
                if (user == null)
                {
                    return Unauthorized();
                }

                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    ShippingAddress = model.ShippingAddress,
                    PhoneNumber = model.PhoneNumber,
                    FullName = model.FullName,
                    Notes = model.Notes
                };

                decimal subtotal = 0;

                foreach (var item in model.Items)
                {
                    _logger.LogInformation("Processing order item: ProductId={ProductId}, Quantity={Quantity}", 
                        item.ProductId, item.Quantity);

                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null)
                    {
                        _logger.LogWarning("Product not found: {ProductId}", item.ProductId);
                        return BadRequest($"Product with ID {item.ProductId} not found");
                    }

                    if (product.Stock < item.Quantity)
                    {
                        _logger.LogWarning("Insufficient stock for product {ProductId}: Requested={Requested}, Available={Available}", 
                            item.ProductId, item.Quantity, product.Stock);
                        return BadRequest($"Insufficient stock for product {product.Name}");
                    }

                    var orderItem = new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        TotalPrice = Math.Round(product.Price * item.Quantity, 2)
                    };

                    subtotal += orderItem.TotalPrice;
                    product.Stock -= item.Quantity;
                    product.SoldCount += item.Quantity;

                    order.OrderItems.Add(orderItem);
                    _logger.LogInformation("Added order item: ProductId={ProductId}, Quantity={Quantity}, TotalPrice={TotalPrice}", 
                        item.ProductId, item.Quantity, orderItem.TotalPrice);
                }

                // Calculate tax (10%) and round to 2 decimal places
                decimal tax = Math.Round(subtotal * 0.1m, 2);
                order.TotalAmount = Math.Round(subtotal + tax, 2);

                _logger.LogInformation("Order total amount: Subtotal={Subtotal}, Tax={Tax}, Total={Total}", 
                    subtotal, tax, order.TotalAmount);

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Order created successfully: OrderId={OrderId}, TotalAmount={TotalAmount}", 
                    order.Id, order.TotalAmount);

                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, new
                {
                    id = order.Id,
                    orderNumber = $"ORD{order.Id:D6}",
                    totalAmount = order.TotalAmount,
                    status = order.Status,
                    orderDate = order.OrderDate
                });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating order");
                return StatusCode(500, "A database error occurred while creating the order");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, "An error occurred while creating the order");
            }
        }

        private bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
            // Define valid transitions
            switch (currentStatus)
            {
                case OrderStatus.Pending:
                    return newStatus == OrderStatus.Processing || newStatus == OrderStatus.Cancelled;
                case OrderStatus.Processing:
                    return newStatus == OrderStatus.Shipped || newStatus == OrderStatus.Cancelled;
                case OrderStatus.Shipped:
                    return newStatus == OrderStatus.Delivered || newStatus == OrderStatus.Cancelled;
                case OrderStatus.Delivered:
                    return false; // Final state, no transitions allowed
                case OrderStatus.Cancelled:
                    return false; // Final state, no transitions allowed
                default:
                    return false;
            }
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusModel model)
        {
            try
            {
                _logger.LogInformation("Attempting to update order {OrderId} status to {NewStatus}", id, model.Status);

                if (!Enum.IsDefined(typeof(OrderStatus), model.Status))
                {
                    _logger.LogWarning("Invalid status value {Status} for order {OrderId}", model.Status, id);
                    return BadRequest(new { message = $"Invalid status value: {model.Status}" });
                }

                var order = await _context.Orders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    _logger.LogWarning("Order {OrderId} not found", id);
                    return NotFound(new { message = $"Order with ID {id} not found" });
                }

                // Validate status transition
                if (!IsValidStatusTransition(order.Status, model.Status))
                {
                    _logger.LogWarning("Invalid status transition for order {OrderId} from {CurrentStatus} to {NewStatus}", 
                        id, order.Status, model.Status);
                    return BadRequest(new { 
                        message = $"Invalid status transition from {order.Status} to {model.Status}",
                        currentStatus = order.Status,
                        requestedStatus = model.Status
                    });
                }

                // Update using a new context to avoid tracking issues
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var orderToUpdate = await _context.Orders.FindAsync(id);
                        if (orderToUpdate != null)
                        {
                            var oldStatus = orderToUpdate.Status;
                            orderToUpdate.Status = model.Status;
                await _context.SaveChangesAsync();
                            await transaction.CommitAsync();

                            _logger.LogInformation("Successfully updated order {OrderId} status from {OldStatus} to {NewStatus}", 
                                id, oldStatus, model.Status);

                            return Ok(new
                            {
                                id = orderToUpdate.Id,
                                status = orderToUpdate.Status,
                                message = "Order status updated successfully"
                            });
                        }
                        return NotFound(new { message = $"Order with ID {id} not found" });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating status for order {OrderId}", id);
                return StatusCode(500, new { message = "A database error occurred while updating the order status" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for order {OrderId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the order status" });
            }
        }

        [HttpPost("recalculate-totals")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RecalculateOrderTotals()
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ToListAsync();

                foreach (var order in orders)
                {
                    decimal subtotal = order.OrderItems.Sum(oi => oi.TotalPrice);
                    decimal tax = Math.Round(subtotal * 0.1m, 2);
                    order.TotalAmount = Math.Round(subtotal + tax, 2);

                    _logger.LogInformation("Recalculating order {OrderId}: Subtotal={Subtotal}, Tax={Tax}, Total={Total}", 
                        order.Id, subtotal, tax, order.TotalAmount);
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Order totals recalculated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recalculating order totals");
                return StatusCode(500, "An error occurred while recalculating order totals");
            }
        }

        [HttpGet("admin/orders")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders([FromQuery] OrderFilterModel filter)
        {
            try
            {
                var query = _context.Orders
                    .AsNoTracking()
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Include(o => o.User)
                    .AsQueryable();

                // Apply filters
                if (filter.Status.HasValue)
                {
                    query = query.Where(o => o.Status == filter.Status.Value);
                }

                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    query = query.Where(o =>
                        o.Id.ToString().Contains(filter.SearchTerm) ||
                        o.User.UserName.Contains(filter.SearchTerm) ||
                        o.User.Email.Contains(filter.SearchTerm) ||
                        o.FullName.Contains(filter.SearchTerm) ||
                        o.PhoneNumber.Contains(filter.SearchTerm)
                    );
                }

                if (filter.FromDate.HasValue)
                {
                    query = query.Where(o => o.OrderDate >= filter.FromDate.Value);
                }

                if (filter.ToDate.HasValue)
                {
                    query = query.Where(o => o.OrderDate <= filter.ToDate.Value);
                }

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "date_desc" => query.OrderByDescending(o => o.OrderDate),
                    "date_asc" => query.OrderBy(o => o.OrderDate),
                    "amount_desc" => query.OrderByDescending(o => o.TotalAmount),
                    "amount_asc" => query.OrderBy(o => o.TotalAmount),
                    _ => query.OrderByDescending(o => o.OrderDate)
                };

                // Apply pagination
                var pageSize = filter.PageSize ?? 10;
                var page = filter.Page ?? 1;
                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var orders = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(o => new AdminOrderDto
                    {
                        Id = o.Id,
                        OrderDate = o.OrderDate,
                        Status = o.Status,
                        TotalAmount = o.TotalAmount,
                        ShippingAddress = o.ShippingAddress,
                        PhoneNumber = o.PhoneNumber,
                        FullName = o.FullName,
                        Notes = o.Notes,
                        UserInfo = new UserInfoDto
                        {
                            Id = o.User.Id,
                            UserName = o.User.UserName,
                            Email = o.User.Email
                        },
                        OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                        {
                            Id = oi.Id,
                            Quantity = oi.Quantity,
                            UnitPrice = oi.UnitPrice,
                            TotalPrice = oi.TotalPrice,
                            Product = new ProductDto
                            {
                                Id = oi.Product.Id,
                                Name = oi.Product.Name,
                                ImageUrl = oi.Product.ImageUrl
                            }
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Orders = orders,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    CurrentPage = page
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all orders");
                return StatusCode(500, "An error occurred while getting orders");
            }
        }

        [HttpPut("admin/orders/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderDetails(int id, [FromBody] UpdateOrderDetailsModel model)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return NotFound();
                }

                // Update order details
                order.ShippingAddress = model.ShippingAddress ?? order.ShippingAddress;
                order.PhoneNumber = model.PhoneNumber ?? order.PhoneNumber;
                order.FullName = model.FullName ?? order.FullName;
                order.Notes = model.Notes ?? order.Notes;

                if (model.Status.HasValue)
                {
                    order.Status = model.Status.Value;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated order {OrderId} details", id);

                return Ok(new { message = "Order updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId}", id);
                return StatusCode(500, "An error occurred while updating the order");
            }
        }

        [HttpGet("admin/orders/statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOrderStatistics([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var query = _context.Orders.AsQueryable();

                if (fromDate.HasValue)
                {
                    query = query.Where(o => o.OrderDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(o => o.OrderDate <= toDate.Value);
                }

                var statistics = await query
                    .GroupBy(o => o.Status)
                    .Select(g => new
                    {
                        Status = g.Key,
                        Count = g.Count(),
                        TotalAmount = g.Sum(o => o.TotalAmount)
                    })
                    .ToListAsync();

                var totalOrders = await query.CountAsync();
                var totalRevenue = await query.SumAsync(o => o.TotalAmount);
                var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

                return Ok(new
                {
                    OrdersByStatus = statistics,
                    TotalOrders = totalOrders,
                    TotalRevenue = totalRevenue,
                    AverageOrderValue = averageOrderValue
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order statistics");
                return StatusCode(500, "An error occurred while getting order statistics");
        }
    }

    public class CreateOrderModel
    {
        [Required]
        [MaxLength(200)]
        public string ShippingAddress { get; set; }

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required]
        [MinLength(1)]
        public List<OrderItemModel> Items { get; set; }
    }

    public class OrderItemModel
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }

    public class UpdateOrderStatusModel
    {
        [Required]
            [Range(0, 4, ErrorMessage = "Status must be between 0 and 4")]
        public OrderStatus Status { get; set; }
        }

        public class OrderFilterModel
        {
            public int? Page { get; set; }
            public int? PageSize { get; set; }
            public OrderStatus? Status { get; set; }
            public string? SearchTerm { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
            public string? SortBy { get; set; }
        }

        public class AdminOrderDto : OrderDto
        {
            public UserInfoDto UserInfo { get; set; }
        }

        public class UserInfoDto
        {
            public int Id { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }
        }

        public class UpdateOrderDetailsModel
        {
            public string ShippingAddress { get; set; }
            public string PhoneNumber { get; set; }
            public string FullName { get; set; }
            public string Notes { get; set; }
            public OrderStatus? Status { get; set; }
        }
    }
}