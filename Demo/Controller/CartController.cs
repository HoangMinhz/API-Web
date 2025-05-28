using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Demo.Data;
using Demo.Models;
using Demo.Models.ViewModel;
using System.Security.Claims;

namespace Demo.Controllers
{
    /// <summary>
    /// Manages user shopping cart operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CartController> _logger;

        public CartController(AppDbContext context, ILogger<CartController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Gets the current user's cart.
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId, Items = new List<CartItem>() };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Retrieved cart for user {UserId}.", userId);
            return Ok(cart);
        }

        /// <summary>
        /// Adds a product to the cart.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] CartItemModel model)
        {
            try
            {
                if (model == null || model.ProductId <= 0 || model.Quantity <= 0)
                {
                    return BadRequest("Invalid cart item data");
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                _logger.LogInformation("Adding product {ProductId} to cart for user {UserId}", model.ProductId, userId);

                // Verify product exists and has sufficient stock
                var product = await _context.Products.FindAsync(model.ProductId);
                if (product == null)
                {
                    return NotFound($"Product with ID {model.ProductId} not found");
                }

                if (product.Stock < model.Quantity)
                {
                    return BadRequest($"Only {product.Stock} items available in stock");
                }

                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new Cart { UserId = userId, Items = new List<CartItem>() };
                    _context.Carts.Add(cart);
                }

                var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == model.ProductId);
                if (existingItem != null)
                {
                    // Check if new total quantity exceeds stock
                    if (existingItem.Quantity + model.Quantity > product.Stock)
                    {
                        return BadRequest($"Cannot add {model.Quantity} more items. Only {product.Stock - existingItem.Quantity} items available in stock");
                    }
                    existingItem.Quantity += model.Quantity;
                }
                else
                {
                    cart.Items.Add(new CartItem
                    {
                        ProductId = model.ProductId,
                        Quantity = model.Quantity
                    });
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully added product {ProductId} to cart for user {UserId}", model.ProductId, userId);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart");
                return StatusCode(500, "An error occurred while adding item to cart");
            }
        }

        /// <summary>
        /// Updates the quantity of a product in the cart.
        /// </summary>
        [Authorize]
        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateCart(int productId, [FromBody] int quantity)
        {
            try
            {
                if (quantity < 1)
                {
                    return BadRequest("Quantity must be at least 1");
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                _logger.LogInformation("Updating quantity for product {ProductId} in cart for user {UserId}", productId, userId);

                // Verify product exists and has sufficient stock
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    return NotFound($"Product with ID {productId} not found");
                }

                if (quantity > product.Stock)
                {
                    return BadRequest($"Only {product.Stock} items available in stock");
                }

                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    return NotFound("Cart not found");
                }

                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                if (item == null)
                {
                    return NotFound("Item not found in cart");
                }

                item.Quantity = quantity;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully updated quantity for product {ProductId} in cart for user {UserId}", productId, userId);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item");
                return StatusCode(500, "An error occurred while updating cart item");
            }
        }

        /// <summary>
        /// Removes a product from the cart.
        /// </summary>
        [Authorize]
        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return NotFound("Cart not found.");

            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null)
                return NotFound("Item not found in cart.");

            cart.Items.Remove(item);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Removed product {ProductId} from cart for user {UserId}.", productId, userId);
            return Ok(cart);
        }

        /// <summary>
        /// Clears all items from the cart.
        /// </summary>
        [Authorize]
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return NotFound("Cart not found.");

            cart.Items.Clear();
            await _context.SaveChangesAsync();
            _logger.LogInformation("Cleared cart for user {UserId}.", userId);
            return Ok(new { message = "Cart cleared successfully" });
        }
    }
}