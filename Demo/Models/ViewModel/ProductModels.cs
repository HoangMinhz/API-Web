using System.ComponentModel.DataAnnotations;

namespace Demo.Models.ViewModel
{
    public class CreateProductDto
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200)]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Product description is required")]
        public string Description { get; set; }
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be greater than or equal to 0")]
        public int Stock { get; set; }
        
        [Required(ErrorMessage = "Image URL is required")]
        public string ImageUrl { get; set; }
        
        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }
    }

    public class UpdateProductDto
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200)]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Product description is required")]
        public string Description { get; set; }
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be greater than or equal to 0")]
        public int Stock { get; set; }
        
        [Required(ErrorMessage = "Image URL is required")]
        public string ImageUrl { get; set; }
        
        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }
    }
} 