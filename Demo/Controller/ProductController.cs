using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Demo.Data;
using Demo.Models;

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
        }

        /// <summary>
        /// Gets all products, optionally filtered by category or search query.
        /// </summary>
        [HttpGet]
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
        [HttpGet("{id}")]
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
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] Product model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var product = new Product
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    Stock = model.Stock,
                    ImageUrl = model.ImageUrl,
                    CategoryId = model.CategoryId,
                    Rating = 0,
                    ReviewCount = 0,
                    SoldCount = 0,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created product {ProductId}.", product.Id);
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, "An error occurred while creating the product");
            }
        }

        /// <summary>
        /// Updates a product (Admin only).
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product model)
        {
            try
            {
                if (id != model.Id || !ModelState.IsValid)
                    return BadRequest();

                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    _logger.LogWarning("Product {Id} not found for update.", id);
                    return NotFound();
                }

                product.Name = model.Name;
                product.Description = model.Description;
                product.Price = model.Price;
                product.ImageUrl = model.ImageUrl;
                product.Stock = model.Stock;
                product.CategoryId = model.CategoryId;
                product.Rating = model.Rating;
                product.ReviewCount = model.ReviewCount;
                product.SoldCount = model.SoldCount;

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
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
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