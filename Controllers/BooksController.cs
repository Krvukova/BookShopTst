using BookShopTest.Data;
using BookShopTest.Models;
using BookShopTest.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

namespace BookShopTest.Controllers
{
    [Authorize(Roles = "Admin")]
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
        public async Task<IActionResult> Add(AddBookViewModel viewModel)
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

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var books = await dbContext.Books.ToListAsync();
            return View(books);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var book = await dbContext.Books.FindAsync(id);

            return View(book);


        }

        [HttpPost]
        public async Task<IActionResult> Edit(Book viewModel)
        {
            var book = await dbContext.Books.FindAsync(viewModel.Id);

            if (book is not null)
            {
                book.Title = viewModel.Title;
                book.Author = viewModel.Author;
                book.Price = viewModel.Price;
                book.Genre = viewModel.Genre;

                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("List", "Books");

        
        }

        [HttpPost]  // Change to POST since we're deleting data
        public async Task<IActionResult> Delete(int id)
        {
            var book = await dbContext.Books.FindAsync(id);
            if (book is not null)
            {
                dbContext.Books.Remove(book);
                await dbContext.SaveChangesAsync();
            }
            return RedirectToAction("List", "Books");
        }
    }
}
