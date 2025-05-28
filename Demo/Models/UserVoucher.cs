using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demo.Models
{
    public class UserVoucher
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserVoucherId { get; set; } // Primary Key
        
        [Required]
        public int UserId { get; set; }        // ID người dùng
        
        [Required]
        public int VoucherId { get; set; }     // ID voucher
        
        public DateTime? UsedAt { get; set; }  // Thời gian sử dụng
        
        public int? OrderId { get; set; }      // Liên kết với đơn hàng

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser? User { get; set; }
        
        [ForeignKey("VoucherId")]
        public virtual Voucher? Voucher { get; set; }
        
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }
    }
}