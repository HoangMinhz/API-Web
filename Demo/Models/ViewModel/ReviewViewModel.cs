using System.ComponentModel.DataAnnotations;

namespace Demo.Models.ViewModel
{
    public class ReviewViewModel
    {
        public int Id { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        [Range(1, 5, ErrorMessage = "Diem danh gia phai tu 1 den 5")]
        public int Rating { get; set; }
        
        [Required]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Binh luan phai tu 10 den 500 ky tu")]
        public string Comment { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; }
    }

    public class CreateReviewViewModel
    {
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        [Range(1, 5, ErrorMessage = "Diem danh gia phai tu 1 den 5")]
        public int Rating { get; set; }
        
        [Required]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Binh luan phai tu 10 den 500 ky tu")]
        public string Comment { get; set; }
    }

    public class UpdateReviewViewModel
    {
        [Required]
        [Range(1, 5, ErrorMessage = "Diem danh gia phai tu 1 den 5")]
        public int Rating { get; set; }
        
        [Required]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Binh luan phai tu 10 den 500 ky tu")]
        public string Comment { get; set; }
    }
} 