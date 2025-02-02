using BookShopTest.Models.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShopTest.Models
{
    public class CartItem
    {
        public int Id { get; set; } 
        public int BookId { get; set; } 
        public int Quantity { get; set; }
        public decimal Price { get; set; } 

        [ForeignKey("BookId")]
        public Book Book { get; set; } 

        public string UserId { get; set; } 
    }
}