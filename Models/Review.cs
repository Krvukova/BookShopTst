using BookShopTest.Areas.Identity.Data;
using System;
using System.ComponentModel.DataAnnotations;

namespace BookShopTest.Models.Entities
{
    public class Review
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string UserId { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        public string Comment { get; set; } // Comment is no longer required

        public DateTime DatePosted { get; set; }

        public virtual Book Book { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}