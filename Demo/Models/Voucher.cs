using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Demo.Models
{
    public enum DiscountType
    {
        PERCENTAGE,
        FIXED_AMOUNT
    }

    public class Voucher
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VoucherId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; }

        [Required]
        public DiscountType DiscountType { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountValue { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MaxDiscount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MinOrderValue { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? UsageLimit { get; set; }

        public int UsedCount { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}