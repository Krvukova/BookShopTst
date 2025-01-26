﻿using BookShopTest.Data;
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
        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchQuery, string genre)
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

            return View(await books.ToListAsync());
        }

        [HttpGet]
        [AllowAnonymous]
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
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(coverImage.FileName);
                var filePath = Path.Combine(_env.WebRootPath, "images", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await coverImage.CopyToAsync(stream);
                }

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
                existingBook.Quantity = book.Quantity; // Add this line

                if (coverImage != null)
                {
                    if (!string.IsNullOrEmpty(existingBook.CoverImageUrl))
                    {
                        var oldFilePath = Path.Combine(_env.WebRootPath, existingBook.CoverImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

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
        public IActionResult AddToCart(int bookId)
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
                return RedirectToAction("Details", new { id = bookId });
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
                    TempData["ErrorMessage"] = "Not enough stock available.";
                    return RedirectToAction("Details", new { id = bookId });
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

            return RedirectToAction("Index");
        }
    }
}