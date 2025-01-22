using BookShopTest.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace BookShopTest.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int BookId { get; set; }
        public int Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Unit price must be a positive value.")]
        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }
        public virtual Order Order { get; set; }
        public virtual Book Book { get; set; }
    }
}