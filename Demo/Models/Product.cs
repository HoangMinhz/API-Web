using System.ComponentModel.DataAnnotations;

namespace Demo.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        [Range(0, 5)]
        public float Rating { get; set; } = 0;
        public int ReviewCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int SoldCount { get; set; } = 0;
        public List<Review> Reviews { get; set; } = new List<Review>();
    }
}