using BookShopTest.Data;
using BookShopTest.Models;
using BookShopTest.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BookShopTest.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IWebHostEnvironment _env;

        public BooksController(ApplicationDbContext dbContext, IWebHostEnvironment env)
        {
            this.dbContext = dbContext;
            _env = env;
        }

        [HttpGet]
        [AllowAnonymous] // Ensure this is accessible to all users
        public async Task<IActionResult> Index(string searchQuery, string genre)
        {
            var books = dbContext.Books.AsQueryable();

            // Filter by search query
            if (!string.IsNullOrEmpty(searchQuery))
            {
                books = books.Where(b => b.Title.Contains(searchQuery) || b.Author.Contains(searchQuery));
            }

            // Filter by genre
            if (!string.IsNullOrEmpty(genre))
            {
                books = books.Where(b => b.Genre == genre);
            }

            return View(await books.ToListAsync());
        }

        [HttpGet]
        [AllowAnonymous] // Accessible to all users
        public async Task<IActionResult> Details(int id)
        {
            var book = await dbContext.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Book book, IFormFile coverImage)
        {
            if (coverImage != null)
            {
                // Create a unique filename
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(coverImage.FileName);
                var filePath = Path.Combine(_env.WebRootPath, "images", fileName);

                // Save the file to the wwwroot/images directory
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await coverImage.CopyToAsync(stream);
                }

                // Set the CoverImageUrl property to the relative path
                book.CoverImageUrl = "/images/" + fileName;
            }

            dbContext.Books.Add(book);
            await dbContext.SaveChangesAsync();
            return RedirectToAction("Index");
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

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Book book, IFormFile coverImage)
        {
            var existingBook = await dbContext.Books.FindAsync(book.Id);

            if (existingBook != null)
            {
                existingBook.Title = book.Title;
                existingBook.Author = book.Author;
                existingBook.Genre = book.Genre;
                existingBook.Price = book.Price;
                existingBook.Description = book.Description;

                if (coverImage != null)
                {
                    // Delete old image file (optional)
                    if (!string.IsNullOrEmpty(existingBook.CoverImageUrl))
                    {
                        var oldFilePath = Path.Combine(_env.WebRootPath, existingBook.CoverImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Save the new file
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(coverImage.FileName);
                    var filePath = Path.Combine(_env.WebRootPath, "images", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await coverImage.CopyToAsync(stream);
                    }

                    existingBook.CoverImageUrl = "/images/" + fileName;
                }

                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]  // Change to POST since we're deleting data
        public async Task<IActionResult> Delete(int id)
        {
            var book = await dbContext.Books.FindAsync(id);
            if (book != null)
            {
                dbContext.Books.Remove(book);
                await dbContext.SaveChangesAsync();
            }
            return RedirectToAction("List", "Books");
        }
        [HttpPost]
        public IActionResult AddToCart(int bookId)
        {
            // Retrieve the book from the database by ID
            var book = dbContext.Books.FirstOrDefault(b => b.Id == bookId);
            if (book == null)
            {
                return NotFound();
            }

            // Retrieve the current cart from the session (or create a new one if it doesn't exist)
            var cart = HttpContext.Session.GetObjectFromJson<List<Book>>("Cart") ?? new List<Book>();

            // Add the selected book to the cart
            cart.Add(book);

            // Save the updated cart back to the session
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            // Redirect back to the details page of the same book
            return RedirectToAction("Details", new { id = bookId });
        }
    }
}
