using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Demo.Data;
using Demo.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace Demo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(AppDbContext context, ILogger<CategoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("list")]
        [AllowAnonymous]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var query = _context.Categories.ToQueryString();
                _logger.LogInformation("SQL Query: {Query}", query);

                var categories = await _context.Categories.ToListAsync();
                
                foreach (var category in categories)
                {
                    _logger.LogInformation("Category Data - Id: {Id}, Name: {Name}, ImageUrl: {ImageUrl}", 
                        category.Id, category.Name, category.ImageUrl);
                }

                var json = JsonSerializer.Serialize(categories);
                _logger.LogInformation("Response JSON: {Json}", json);

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return StatusCode(500, "An error occurred while retrieving categories");
            }
        }

        [HttpGet("get/{id}")]
        [AllowAnonymous]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> GetCategory(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                    return NotFound();

                _logger.LogInformation("Category Data - Id: {Id}, Name: {Name}, ImageUrl: {ImageUrl}", 
                    category.Id, category.Name, category.ImageUrl);

                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the category");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PostCategory([FromBody] Category category)
        {
            try
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created Category - Id: {Id}, Name: {Name}, ImageUrl: {ImageUrl}", 
                    category.Id, category.Name, category.ImageUrl);

                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, "An error occurred while creating the category");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutCategory(int id, [FromBody] Category category)
        {
            try
            {
                var existingCategory = await _context.Categories.FindAsync(id);
                if (existingCategory == null)
                    return NotFound();

                existingCategory.Name = category.Name;
                existingCategory.ImageUrl = category.ImageUrl;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated Category - Id: {Id}, Name: {Name}, ImageUrl: {ImageUrl}", 
                    id, category.Name, category.ImageUrl);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {Id}", id);
                return StatusCode(500, "An error occurred while updating the category");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                    return NotFound();

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted Category - Id: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {Id}", id);
                return StatusCode(500, "An error occurred while deleting the category");
            }
        }
    }
}