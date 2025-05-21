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
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                
                // Check if user has purchased the product
                var hasPurchased = await _context.OrderItems
                    .AnyAsync(oi => oi.Order.UserId == userId && oi.ProductId == model.ProductId);

                if (!hasPurchased)
                {
                    return BadRequest("You must purchase the product before reviewing it");
                }

                // Check if user has already reviewed this product
                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == model.ProductId);

                if (existingReview != null)
                {
                    return BadRequest("You have already reviewed this product");
                }

                var review = new Review
                {
                    ProductId = model.ProductId,
                    UserId = userId,
                    Rating = model.Rating,
                    Comment = model.Comment,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Reviews.Add(review);

                // Update product rating
                var product = await _context.Products.FindAsync(model.ProductId);
                if (product != null)
                {
                    product.ReviewCount++;
                    product.Rating = (float)await _context.Reviews
                        .Where(r => r.ProductId == model.ProductId)
                        .AverageAsync(r => r.Rating);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Created review {ReviewId} for product {ProductId}", review.Id, model.ProductId);
                return CreatedAtAction(nameof(GetProductReviews), new { productId = model.ProductId }, new ReviewViewModel
                {
                    Id = review.Id,
                    ProductId = review.ProductId,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedAt = review.CreatedAt,
                    UserName = User.Identity.Name
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review");
                return StatusCode(500, "An error occurred while creating the review");
            }
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