using Microsoft.EntityFrameworkCore;

namespace BookShopTest.Models.Entities
{
    public class Book
    {

        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        [Precision(18, 2)]
        public decimal Price { get; set; }
        public string Genre { get; set; }

        public string CoverImageUrl { get; set; }
        public string Description { get; set; }

    }
}
