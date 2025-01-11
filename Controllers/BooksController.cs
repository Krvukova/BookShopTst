using BookShopTest.Data;
using BookShopTest.Models;
using BookShopTest.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BookShopTest.Controllers
{
    public class BooksController : Controller

    {
        private readonly ApplicationDbContext dbContext;

        public BooksController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }
        [HttpPost]
        public async Task <IActionResult> Add(AddBookViewModel viewModel)
        {
            var book = new Book
            {
                Title = viewModel.Title,
                Author = viewModel.Author,
                Price = viewModel.Price,
                Genre = viewModel.Genre,
            };
            await dbContext.Books.AddAsync(book);
            await dbContext.SaveChangesAsync();


            return View();

        }
    }
}
