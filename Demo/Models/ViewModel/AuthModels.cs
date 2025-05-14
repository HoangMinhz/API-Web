using System.ComponentModel.DataAnnotations;

namespace Demo.Models.ViewModel
{
    public class UserInfo
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public List<string> Roles { get; set; }
        public bool EmailConfirmed { get; set; }

    }
} 