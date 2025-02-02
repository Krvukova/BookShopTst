using BookShopTest.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShopTest.Models
{
    public class OrderItem
    {
        public int Id { get; set; } 

        [Required]
        public int OrderId { get; set; } 

        [Required]
        public int BookId { get; set; } 

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; } 

        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        [ForeignKey("BookId")]
        public Book Book { get; set; } 
    }
}