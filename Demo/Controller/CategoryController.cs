using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Demo.Data;
using Demo.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

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

        [HttpGet]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                // Debug: Log the SQL query
                var query = _context.Categories.ToQueryString();
                _logger.LogInformation("SQL Query: {Query}", query);

                // Get raw data from database
                var categories = await _context.Categories.ToListAsync();
                
                // Debug: Log each category's data
                foreach (var category in categories)
                {
                    _logger.LogInformation("Category Data - Id: {Id}, Name: {Name}, ImageUrl: {ImageUrl}", 
                        category.Id, category.Name, category.ImageUrl);
                }

                // Debug: Log the serialized response
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

        [HttpGet("{id}")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                    return NotFound();

                // Debug: Log the category data
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
        public async Task<IActionResult> Create([FromBody] Category category)
        {
            try
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                // Debug: Log the created category
                _logger.LogInformation("Created Category - Id: {Id}, Name: {Name}, ImageUrl: {ImageUrl}", 
                    category.Id, category.Name, category.ImageUrl);

                return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, "An error occurred while creating the category");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Category category)
        {
            try
            {
                var existingCategory = await _context.Categories.FindAsync(id);
                if (existingCategory == null)
                    return NotFound();

                existingCategory.Name = category.Name;
                existingCategory.ImageUrl = category.ImageUrl;
                await _context.SaveChangesAsync();

                // Debug: Log the updated category
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
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                    return NotFound();

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

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