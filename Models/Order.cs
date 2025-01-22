using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookShopTest.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        [Required]
        public string UserId { get; set; }

        public DateTime OrderDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Total amount must be a positive value.")]
        public decimal TotalAmount { get; set; }

        [Required]
        public string OrderStatus { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}