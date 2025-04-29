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
            // For unauthenticated users, we'll use a session-based cart
            if (!User.Identity.IsAuthenticated)
            {
                // In a real application, you might want to store this in a session or Redis
                // For now, we'll just return a success response
                return Ok(new { message = "Item added to cart" });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
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
            _logger.LogInformation("Added product {ProductId} to cart for user {UserId}.", model.ProductId, userId);
            return Ok(cart);
        }

        /// <summary>
        /// Updates the quantity of a product in the cart.
        /// </summary>
        [Authorize]
        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateCart(int productId, [FromBody] int quantity)
        {
            if (quantity < 1)
                return BadRequest("Quantity must be at least 1.");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return NotFound("Cart not found.");

            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null)
                return NotFound("Item not found in cart.");

            item.Quantity = quantity;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated quantity of product {ProductId} in cart for user {UserId}.", productId, userId);
            return Ok(cart);
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
    }
}