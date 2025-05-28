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

        // GET: api/Review/product/{productId} - Lấy tất cả reviews của sản phẩm (public)
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

        // GET: api/Review/list/{productId} - Alias cho GetProductReviews
        [HttpGet("list/{productId}")]
        public async Task<IActionResult> GetReviewsList(int productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            return await GetProductReviews(productId, page, pageSize);
        }

        // GET: api/Review/can-review/{productId} - Kiểm tra user có thể đánh giá không
        [Authorize]
        [HttpGet("can-review/{productId}")]
        public async Task<IActionResult> CanReview(int productId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized("User not authenticated");
                }

                _logger.LogInformation("Checking review eligibility for User {UserId} and Product {ProductId}", userId, productId);

                // Kiểm tra user đã mua sản phẩm và đơn hàng đã delivered
                var hasPurchasedAndDelivered = await _context.OrderItems
                    .Join(_context.Orders,
                        oi => oi.OrderId,
                        o => o.Id,
                        (oi, o) => new { oi, o })
                    .AnyAsync(x => x.oi.ProductId == productId 
                                && x.o.UserId == userId 
                                && x.o.Status == OrderStatus.Delivered);

                _logger.LogInformation("User {UserId} can review product {ProductId}: {CanReview}", userId, productId, hasPurchasedAndDelivered);
                
                return Ok(new { CanReview = hasPurchasedAndDelivered });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking review eligibility for product {ProductId}", productId);
                return StatusCode(500, "An error occurred while checking review eligibility");
            }
        }

        // GET: api/Review/user-review/{productId} - Lấy review của user hiện tại cho sản phẩm
        [Authorize]
        [HttpGet("user-review/{productId}")]
        public async Task<IActionResult> GetUserReview(int productId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized("User not authenticated");
                }

                var userReview = await _context.Reviews
                    .Include(r => r.User)
                    .Where(r => r.UserId == userId && r.ProductId == productId)
                    .Select(r => new ReviewViewModel
                    {
                        Id = r.Id,
                        ProductId = r.ProductId,
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt,
                        UserName = r.User.UserName
                    })
                    .FirstOrDefaultAsync();

                if (userReview == null)
                {
                    return NotFound("User has not reviewed this product");
                }

                return Ok(userReview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user review for product {ProductId}", productId);
                return StatusCode(500, "An error occurred while retrieving user review");
            }
        }

        // POST: api/Review/submit-review - Tạo review mới
        [Authorize]
        [HttpPost("submit-review")]
        public async Task<IActionResult> SubmitReview([FromBody] CreateReviewViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized("User not authenticated");
                }

                // Kiểm tra sản phẩm có tồn tại
                var product = await _context.Products.FindAsync(model.ProductId);
                if (product == null)
                {
                    return BadRequest("Product not found");
                }

                // Kiểm tra user đã mua sản phẩm và delivered chưa
                var canReview = await _context.OrderItems
                    .Join(_context.Orders,
                        oi => oi.OrderId,
                        o => o.Id,
                        (oi, o) => new { oi, o })
                    .AnyAsync(x => x.oi.ProductId == model.ProductId 
                                && x.o.UserId == userId 
                                && x.o.Status == OrderStatus.Delivered);

                if (!canReview)
                {
                    return BadRequest("You must purchase and receive this product before reviewing it");
                }

                // Kiểm tra user đã review chưa
                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == model.ProductId);

                if (existingReview != null)
                {
                    return BadRequest("You have already reviewed this product");
                }

                // Tạo review mới
                var review = new Review
                {
                    ProductId = model.ProductId,
                    UserId = userId,
                    Rating = model.Rating,
                    Comment = model.Comment ?? "",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                // Cập nhật rating của sản phẩm
                await UpdateProductRating(model.ProductId);

                _logger.LogInformation("User {UserId} successfully created review for product {ProductId}", userId, model.ProductId);

                return Ok(new { Message = "Review submitted successfully", ReviewId = review.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting review for product {ProductId}", model.ProductId);
                return StatusCode(500, "An error occurred while submitting the review");
            }
        }

        // PUT: api/Review/update-review/{reviewId} - Cập nhật review
        [Authorize]
        [HttpPut("update-review/{reviewId}")]
        public async Task<IActionResult> UpdateReview(int reviewId, [FromBody] UpdateReviewViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized("User not authenticated");
                }

                var review = await _context.Reviews.FindAsync(reviewId);
                if (review == null)
                {
                    return NotFound("Review not found");
                }

                // Chỉ cho phép user sửa review của chính mình
                if (review.UserId != userId)
                {
                    return Forbid("You can only edit your own reviews");
                }

                review.Rating = model.Rating;
                review.Comment = model.Comment ?? "";

                await _context.SaveChangesAsync();

                // Cập nhật rating của sản phẩm
                await UpdateProductRating(review.ProductId);

                _logger.LogInformation("User {UserId} successfully updated review {ReviewId}", userId, reviewId);

                return Ok(new { Message = "Review updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review {ReviewId}", reviewId);
                return StatusCode(500, "An error occurred while updating the review");
            }
        }

        // DELETE: api/Review/delete-review/{reviewId} - Xóa review
        [Authorize]
        [HttpDelete("delete-review/{reviewId}")]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized("User not authenticated");
                }

                var review = await _context.Reviews.FindAsync(reviewId);
                if (review == null)
                {
                    return NotFound("Review not found");
                }

                // Chỉ cho phép user xóa review của chính mình hoặc admin
                var isAdmin = User.IsInRole("Admin");
                if (review.UserId != userId && !isAdmin)
                {
                    return Forbid("You can only delete your own reviews");
                }

                var productId = review.ProductId;
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();

                // Cập nhật rating của sản phẩm
                await UpdateProductRating(productId);

                _logger.LogInformation("User {UserId} successfully deleted review {ReviewId}", userId, reviewId);

                return Ok(new { Message = "Review deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review {ReviewId}", reviewId);
                return StatusCode(500, "An error occurred while deleting the review");
            }
        }

        // Helper methods
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private async Task UpdateProductRating(int productId)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product != null)
                {
                    var reviews = await _context.Reviews
                        .Where(r => r.ProductId == productId)
                        .ToListAsync();

                    if (reviews.Any())
                    {
                        product.Rating = (float)reviews.Average(r => r.Rating);
                        product.ReviewCount = reviews.Count;
                    }
                    else
                    {
                        product.Rating = 0;
                        product.ReviewCount = 0;
                    }

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product rating for product {ProductId}", productId);
            }
        }
    }
}