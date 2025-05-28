using System.ComponentModel.DataAnnotations;

namespace Demo.Models.ViewModel
{
    public class OrderInfoModel
    {
        public int OrderId { get; set; }
        
        public decimal Amount { get; set; }
        
        public string OrderInfo { get; set; } = string.Empty;
        
        public string ReturnUrl { get; set; } = string.Empty;
        
        public string NotifyUrl { get; set; } = string.Empty;
    }
} 