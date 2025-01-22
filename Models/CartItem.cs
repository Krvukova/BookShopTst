using BookShopTest.Models.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShopTest.Models
{
    public class CartItem
    {
        public int Id { get; set; } // Primary Key
        public int BookId { get; set; } // Foreign Key
        public int Quantity { get; set; }
        public decimal Price { get; set; } // Optional, can be derived from Book.Price

        [ForeignKey("BookId")]
        public Book Book { get; set; } // Navigation property

        public string UserId { get; set; } // User association
    }
}