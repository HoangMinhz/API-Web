using System.ComponentModel.DataAnnotations;

namespace Demo.Models
{
    public class ConfirmEmailModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Token { get; set; }
    }
}