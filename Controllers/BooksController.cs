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
using Microsoft.AspNetCore.Identity;
using BookShopTest.Areas.Identity.Data;

namespace BookShopTest.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> userManager;


        public BooksController(ApplicationDbContext dbContext, IWebHostEnvironment env, UserManager<ApplicationUser> userManager)
        {
            this.dbContext = dbContext;
            _env = env;
            this.userManager = userManager;

        }

        public IActionResult Index(string searchQuery, string genre, string sortOrder)
        {
            var books = dbContext.Books.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                books = books.Where(b => b.Title.Contains(searchQuery) || b.Author.Contains(searchQuery));
            }

            if (!string.IsNullOrEmpty(genre))
            {
                books = books.Where(b => b.Genre == genre);
            }

            switch (sortOrder)
            {
                case "price_asc":
                    books = books.OrderBy(b => b.Price);
                    break;
                case "price_desc":
                    books = books.OrderByDescending(b => b.Price);
                    break;
                case "date_asc":
                    books = books.OrderBy(b => b.DateAdded);
                    break;
                case "date_desc":
                    books = books.OrderByDescending(b => b.DateAdded);
                    break;
                default:
                    books = books.OrderBy(b => b.Title); // Default sorting by title
                    break;
            }

            var genres = dbContext.Books.Select(b => b.Genre).Distinct().ToList();
            ViewBag.Genres = genres;

            return View(books.ToList());
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var book = await dbContext.Books
                .Include(b => b.Reviews)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int bookId, int rating, string comment)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "User must be logged in to leave a review.";
                return RedirectToAction("Details", new { id = bookId });
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Details", new { id = bookId });
            }

            // Check if the user exists in the database
            var userExists = await dbContext.Users.AnyAsync(u => u.Id == user.Id);
            if (!userExists)
            {
                TempData["ErrorMessage"] = "User does not exist in the database.";
                return RedirectToAction("Details", new { id = bookId });
            }

            var review = new Review
            {
                BookId = bookId,
                UserId = user.Id,
                Rating = rating,
                Comment = comment ?? string.Empty, // Set Comment to empty string if null
                DatePosted = DateTime.Now
            };

            dbContext.Reviews.Add(review);
            await dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Review added successfully.";
            return RedirectToAction("Details", new { id = bookId });
        }
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddBookViewModel model)
        {
            if (ModelState.IsValid)
            {
                var book = new Book
                {
                    Title = model.Title,
                    Author = model.Author,
                    Price = model.Price,
                    Genre = model.Genre,
                    Quantity = model.Quantity,
                    Description = model.Description,
                    DateAdded = DateTime.Now
                };

                if (model.CoverImage != null)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.CoverImage.FileName);
                    var filePath = Path.Combine(_env.WebRootPath, "images", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.CoverImage.CopyToAsync(stream);
                    }

                    book.CoverImageUrl = "/images/" + fileName;
                }

                dbContext.Books.Add(book);
                await dbContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Book added successfully.";
                return RedirectToAction("List");
            }

            return View(model);
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

            var viewModel = new EditBookViewModel
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Price = book.Price,
                Genre = book.Genre,
                Description = book.Description,
                Quantity = book.Quantity,
                ExistingCoverImageUrl = book.CoverImageUrl
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditBookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingBook = await dbContext.Books.FindAsync(model.Id);

            if (existingBook != null)
            {
                existingBook.Title = model.Title;
                existingBook.Author = model.Author;
                existingBook.Genre = model.Genre;
                existingBook.Price = model.Price;
                existingBook.Description = model.Description;
                existingBook.Quantity = model.Quantity;

                if (model.CoverImage != null)
                {
                    // Delete the old cover image if it exists
                    if (!string.IsNullOrEmpty(existingBook.CoverImageUrl))
                    {
                        var oldFilePath = Path.Combine(_env.WebRootPath, existingBook.CoverImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Save the new cover image
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.CoverImage.FileName);
                    var filePath = Path.Combine(_env.WebRootPath, "images", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.CoverImage.CopyToAsync(stream);
                    }

                    existingBook.CoverImageUrl = "/images/" + fileName;
                }
                else
                {
                    // Retain the old cover image
                    existingBook.CoverImageUrl = model.ExistingCoverImageUrl;
                }

                await dbContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Book updated successfully.";
            }

            return RedirectToAction("List");
        }

        [HttpPost]
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
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int bookId, string returnUrl)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    TempData["ErrorMessage"] = "User must be logged in to add items to cart.";
                    return Redirect("/Identity/Account/Login");
                }

                var book = dbContext.Books.FirstOrDefault(b => b.Id == bookId);
                if (book == null || book.Quantity == 0)
                {
                    TempData["ErrorMessage"] = "This book is out of stock.";
                    return Redirect(returnUrl);
                }

                var userId = User.Identity.Name;
                var existingCartItem = dbContext.CartItems.FirstOrDefault(c => c.BookId == bookId && c.UserId == userId);

                if (existingCartItem != null)
                {
                    if (existingCartItem.Quantity < book.Quantity)
                    {
                        existingCartItem.Quantity++;
                    }
                    else
                    {
                        TempData["ErrorMessage"] = $"Only {book.Quantity} copies of {book.Title} are available for purchase.";
                        return Redirect(returnUrl);
                    }
                }
                else
                {
                    var cartItem = new CartItem
                    {
                        BookId = book.Id,
                        Quantity = 1,
                        Price = book.Price,
                        UserId = userId
                    };
                    dbContext.CartItems.Add(cartItem);
                }

                dbContext.SaveChanges();

                TempData["CartMessage"] = "Book has been added to cart";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while adding the book to the cart.";
                // Log the exception (ex) for further analysis
            }

            return Redirect(returnUrl);
        }

    }
}