using System;
using System.Collections.Generic;

namespace Demo.Models.ViewModel
{
    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public string Notes { get; set; }
        public string? VoucherCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public ProductDto Product { get; set; }
    }
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
    }
} 