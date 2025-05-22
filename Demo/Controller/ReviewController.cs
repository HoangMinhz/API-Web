using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Demo.Data;
using Demo.Models;
using Demo.Models.ViewModel;
using System.Security.Claims;

namespace Demo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(AppDbContext context, ILogger<ReviewController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetProductReviews(int productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var reviews = await _context.Reviews
                    .Include(r => r.User)
                    .Where(r => r.ProductId == productId)
                    .OrderByDescending(r => r.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new ReviewViewModel
                    {
                        Id = r.Id,
                        ProductId = r.ProductId,
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt,
                        UserName = r.User.UserName
                    })
                    .ToListAsync();

                var totalReviews = await _context.Reviews.CountAsync(r => r.ProductId == productId);

                return Ok(new
                {
                    Reviews = reviews,
                    TotalReviews = totalReviews,
                    CurrentPage = page,
                    TotalPages = (int)Math.Ceiling(totalReviews / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews for product {ProductId}", productId);
                return StatusCode(500, "An error occurred while retrieving reviews");
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewViewModel model)
        {
            try
            {
                // Log the incoming model data
                _logger.LogInformation("Creating review for product {ProductId}, Rating: {Rating}, Comment length: {CommentLength}", 
                    model.ProductId, model.Rating, model.Comment?.Length ?? 0);
                
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state when creating review for product {ProductId}", model.ProductId);
                    return BadRequest(ModelState);
                }

                // Check if the product exists
                var product = await _context.Products.FindAsync(model.ProductId);
                if (product == null)
                {
                    _logger.LogWarning("Attempted to review non-existent product {ProductId}", model.ProductId);
                    return BadRequest($"Product with ID {model.ProductId} does not exist");
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    _logger.LogWarning("Unable to get user ID from claims");
                    return BadRequest("Unable to identify user");
                }

                // Check if user has already reviewed this product
                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == model.ProductId);

                if (existingReview != null)
                {
                    _logger.LogWarning("User {UserId} attempted to review product {ProductId} multiple times", userId, model.ProductId);
                    return BadRequest("You have already reviewed this product");
                }

                // Check if the user has purchased the product before reviewing
                var canReview = await CanUserReviewProduct(userId, model.ProductId);
                if (!canReview)
                {
                    _logger.LogWarning("User {UserId} attempted to review product {ProductId} without purchasing it", userId, model.ProductId);
                    return BadRequest("You must purchase this product before reviewing it");
                }

                var review = new Review
                {
                    ProductId = model.ProductId,
                    UserId = userId,
                    Rating = model.Rating,
                    Comment = model.Comment ?? "", // Ensure comment is not null
                    CreatedAt = DateTime.UtcNow
                };

                _context.Reviews.Add(review);

                // Update product rating
                if (product != null)
                {
                    // First save the review to get its ID
                    await _context.SaveChangesAsync();
                    
                    // Then update the product rating in a separate operation
                    product.ReviewCount++;
                    
                    var averageRating = await _context.Reviews
                        .Where(r => r.ProductId == model.ProductId)
                        .AverageAsync(r => (double?)r.Rating) ?? 0;
                    
                    product.Rating = (float)averageRating;
                    
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // If we somehow got here without a product, just save the review
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Successfully created review {ReviewId} for product {ProductId}", review.Id, model.ProductId);
                
                return CreatedAtAction(nameof(GetProductReviews), 
                    new { productId = model.ProductId }, 
                    new ReviewViewModel
                    {
                        Id = review.Id,
                        ProductId = review.ProductId,
                        Rating = review.Rating,
                        Comment = review.Comment,
                        CreatedAt = review.CreatedAt,
                        UserName = User.Identity?.Name ?? "Unknown"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review for product {ProductId}: {ErrorMessage}", 
                    model.ProductId, ex.Message);
                
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner exception: {InnerException}", ex.InnerException.Message);
                }
                
                return StatusCode(500, "An error occurred while creating the review");
            }
        }

        // Helper method to check if a user can review a product
        private async Task<bool> CanUserReviewProduct(int userId, int productId)
        {
            try
            {
                // Check if the user has purchased the product and it's been delivered
                var hasPurchased = await _context.OrderItems
                    .Join(_context.Orders,
                        od => od.OrderId,
                        o => o.Id,
                        (od, o) => new { od, o })
                    .AnyAsync(x => x.od.ProductId == productId
                                && x.o.UserId == userId
                                && x.o.Status == OrderStatus.Delivered);

                return hasPurchased;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} can review product {ProductId}", userId, productId);
                // If there's an error, we'll just allow the review to be more lenient
                return true;
            }
        }

        [Authorize]
        [HttpGet("can-review/{productId}")]
        public async Task<IActionResult> CanReview(int productId)
        {
            // Lấy UserId từ token
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            // Kiểm tra xem người dùng đã mua sản phẩm chưa
            var hasPurchased = await _context.OrderItems
                .Join(_context.Orders,
                    od => od.OrderId,
                    o => o.Id,
                    (od, o) => new { od, o })
                .AnyAsync(x => x.od.ProductId == productId
                            && x.o.UserId == int.Parse(userId)
                            && x.o.Status == OrderStatus.Delivered);

            return Ok(new { CanReview = hasPurchased });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var review = await _context.Reviews
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (review == null)
                {
                    return NotFound();
                }

                if (review.UserId != userId)
                {
                    return Forbid();
                }

                review.Rating = model.Rating;
                review.Comment = model.Comment;

                // Update product rating
                var product = await _context.Products.FindAsync(review.ProductId);
                if (product != null)
                {
                    product.Rating = (float)await _context.Reviews
                        .Where(r => r.ProductId == review.ProductId)
                        .AverageAsync(r => r.Rating);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated review {ReviewId}", id);
                return Ok(new ReviewViewModel
                {
                    Id = review.Id,
                    ProductId = review.ProductId,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedAt = review.CreatedAt,
                    UserName = review.User.UserName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review {ReviewId}", id);
                return StatusCode(500, "An error occurred while updating the review");
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var review = await _context.Reviews.FindAsync(id);

                if (review == null)
                {
                    return NotFound();
                }

                if (review.UserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var productId = review.ProductId;
                _context.Reviews.Remove(review);

                // Update product rating
                var product = await _context.Products.FindAsync(productId);
                if (product != null)
                {
                    product.ReviewCount--;
                    if (product.ReviewCount > 0)
                    {
                        product.Rating = (float)await _context.Reviews
                            .Where(r => r.ProductId == productId)
                            .AverageAsync(r => r.Rating);
                    }
                    else
                    {
                        product.Rating = 0;
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted review {ReviewId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review {ReviewId}", id);
                return StatusCode(500, "An error occurred while deleting the review");
            }
        }
    }
}