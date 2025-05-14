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

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusModel model)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                    return NotFound();

                order.Status = model.Status;
                await _context.SaveChangesAsync();

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for order {OrderId}", id);
                return StatusCode(500, "An error occurred while updating the order status");
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
        public OrderStatus Status { get; set; }
    }
}