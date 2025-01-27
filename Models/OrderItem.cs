using BookShopTest.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShopTest.Models
{
    public class OrderItem
    {
        public int Id { get; set; } // Primary Key

        [Required]
        public int OrderId { get; set; } // Foreign Key

        [Required]
        public int BookId { get; set; } // Foreign Key

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; } // Add this line

        [ForeignKey("OrderId")]
        public Order Order { get; set; } // Navigation property

        [ForeignKey("BookId")]
        public Book Book { get; set; } // Navigation property
    }
}