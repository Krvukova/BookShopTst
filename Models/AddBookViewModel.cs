using Microsoft.EntityFrameworkCore;

namespace BookShopTest.Models
{
    public class AddBookViewModel
    {
        public string Title { get; set; }
        public string Author { get; set; }
        [Precision(18, 2)]
        public decimal Price { get; set; }
        public string Genre { get; set; }
        public IFormFile CoverImage { get; set; }

        public string Description { get; set; }
    }
}
