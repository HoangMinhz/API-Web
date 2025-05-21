using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Demo.Data;
using Demo.Models;
using Demo.Models.ViewModel;

namespace Demo.Controllers
{
    /// <summary>
    /// Manages product-related operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductController> _logger;

        public ProductController(AppDbContext context, ILogger<ProductController> logger)
        {
            _context = context;
            _logger = logger;
            
            // Configure JSON options to handle circular references
            _context.ChangeTracker.LazyLoadingEnabled = false;
        }

        /// <summary>
        /// Gets all products, optionally filtered by category or search query.
        /// </summary>
        [HttpGet("list")]
        [AllowAnonymous]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> GetProducts([FromQuery] int? categoryId, [FromQuery] string? search, [FromQuery] bool? newProducts)
        {
            try
            {
                var query = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Reviews)
                    .AsQueryable();

                if (categoryId.HasValue && categoryId.Value > 0)
                    query = query.Where(p => p.CategoryId == categoryId.Value);

                if (!string.IsNullOrEmpty(search))
                    query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));

                if (newProducts == true)
                    query = query.OrderByDescending(p => p.CreatedAt);

                var products = await query.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.Stock,
                    p.ImageUrl,
                    p.CategoryId,
                    p.Rating,
                    p.ReviewCount,
                    p.SoldCount,
                    p.CreatedAt,
                    CategoryName = p.Category.Name,
                    Reviews = p.Reviews.Select(r => new
                    {
                        r.Id,
                        r.Rating,
                        r.Comment,
                        r.CreatedAt,
                        UserName = r.User.UserName
                    })
                }).ToListAsync();

                _logger.LogInformation("Retrieved {Count} products.", products.Count);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return StatusCode(500, "An error occurred while retrieving products");
            }
        }

        /// <summary>
        /// Gets a product by ID.
        /// </summary>
        [HttpGet("get/{id}")]
        [AllowAnonymous]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Description,
                        p.Price,
                        p.Stock,
                        p.ImageUrl,
                        p.CategoryId,
                        p.Rating,
                        p.ReviewCount,
                        p.SoldCount,
                        p.CreatedAt,
                        CategoryName = p.Category.Name,
                        Reviews = p.Reviews.Select(r => new
                        {
                            r.Id,
                            r.Rating,
                            r.Comment,
                            r.CreatedAt,
                            UserName = r.User.UserName
                        })
                    })
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    _logger.LogWarning("Product {Id} not found.", id);
                    return NotFound();
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the product");
            }
        }

        /// <summary>
        /// Creates a new product (Admin only).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [HttpPost]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> PostProduct(CreateProductDto productDto)
{
    try
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Check if category exists
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == productDto.CategoryId);
            
        if (category == null)
        {
            ModelState.AddModelError("CategoryId", "Danh mục không tồn tại");
            return BadRequest(ModelState);
        }

        // Create new product from DTO
        var product = new Product
        {
            Name = productDto.Name,
            Description = productDto.Description,
            Price = productDto.Price,
            Stock = productDto.Stock,
            ImageUrl = productDto.ImageUrl,
            CategoryId = productDto.CategoryId,
            Rating = 0,
            ReviewCount = 0,
            SoldCount = 0,
            CreatedAt = DateTime.UtcNow,
            Reviews = new List<Review>()
        };

        // Add and save
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Return DTO to avoid circular references
        var response = new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId
        };

        return Ok(response);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Lỗi khi tạo sản phẩm: {Message}", ex.Message);
        return StatusCode(500, "Đã xảy ra lỗi khi tạo sản phẩm");
    }
}

        /// <summary>
        /// Updates a product (Admin only).
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutProduct(int id, UpdateProductDto productDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existingProduct = await _context.Products.FindAsync(id);
                if (existingProduct == null)
                {
                    _logger.LogWarning("Product {Id} not found for update.", id);
                    return NotFound();
                }

                // Check if category exists
                if (await _context.Categories.FindAsync(productDto.CategoryId) == null)
                {
                    ModelState.AddModelError("CategoryId", "Selected category does not exist");
                    return BadRequest(ModelState);
                }

                // Update product properties
                existingProduct.Name = productDto.Name;
                existingProduct.Description = productDto.Description;
                existingProduct.Price = productDto.Price;
                existingProduct.ImageUrl = productDto.ImageUrl;
                existingProduct.Stock = productDto.Stock;
                existingProduct.CategoryId = productDto.CategoryId;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated product {ProductId}.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {Id}", id);
                return StatusCode(500, "An error occurred while updating the product");
            }
        }

        /// <summary>
        /// Deletes a product (Admin only).
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Reviews)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    _logger.LogWarning("Product {Id} not found for deletion.", id);
                    return NotFound();
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted product {ProductId}.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {Id}", id);
                return StatusCode(500, "An error occurred while deleting the product");
            }
        }

        /// <summary>
        /// Gets featured products.
        /// </summary>
        [HttpGet("featured")]
        public async Task<IActionResult> GetFeaturedProducts()
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Reviews)
                    .OrderByDescending(p => p.SoldCount * 0.6 + p.Rating * 0.4)
                    .Take(8)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Description,
                        p.Price,
                        p.Stock,
                        p.ImageUrl,
                        p.CategoryId,
                        p.Rating,
                        p.ReviewCount,
                        p.SoldCount,
                        p.CreatedAt,
                        CategoryName = p.Category.Name,
                        Reviews = p.Reviews.Select(r => new
                        {
                            r.Id,
                            r.Rating,
                            r.Comment,
                            r.CreatedAt,
                            UserName = r.User.UserName
                        })
                    })
                    .ToListAsync();

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving featured products");
                return StatusCode(500, "An error occurred while retrieving featured products");
            }
        }
    }
}
