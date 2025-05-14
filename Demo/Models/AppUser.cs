using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Demo.Models
{   
    public class AppUser : IdentityUser<int>
    {
        [MaxLength(100)]
        public string? FullName { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? State { get; set; }

        [MaxLength(10)]
        public string? Province { get; set; }

        [MaxLength(10)]
        public string? District { get; set; }
        public string? EmailConfirmationToken { get; set; }
        public DateTime? EmailConfirmationTokenExpiry { get; set; }

        public virtual Cart? Cart { get; set; }
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}