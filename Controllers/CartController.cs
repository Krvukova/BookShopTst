using BookShopTest.Data;
using BookShopTest.Models;
using BookShopTest.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShopTest.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public CartController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var userId = User.Identity.Name;
            var cartItems = dbContext.CartItems.Include(c => c.Book).Where(c => c.UserId == userId).ToList();

            ViewBag.TotalQuantity = cartItems.Sum(c => c.Quantity);
            ViewBag.TotalPrice = cartItems.Sum(c => c.Quantity * c.Book.Price);

            return View(cartItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCart(Dictionary<int, int> quantities, int? RemoveItem)
        {
            var userId = User.Identity.Name;
            var cartItems = dbContext.CartItems.Include(c => c.Book).Where(c => c.UserId == userId).ToList();

            if (RemoveItem.HasValue)
            {
                var itemToRemove = cartItems.FirstOrDefault(c => c.BookId == RemoveItem.Value);
                if (itemToRemove != null)
                {
                    dbContext.CartItems.Remove(itemToRemove);
                }
            }

            foreach (var item in quantities)
            {
                var cartItem = cartItems.FirstOrDefault(c => c.BookId == item.Key);
                if (cartItem != null)
                {
                    var book = dbContext.Books.FirstOrDefault(b => b.Id == cartItem.BookId);
                    if (book != null)
                    {
                        if (item.Value > book.Quantity)
                        {
                            TempData["ErrorMessage"] = $"Only {book.Quantity} copies of {book.Title} are available for purchase.";
                            return RedirectToAction("Index");
                        }
                        cartItem.Quantity = item.Value > 0 ? item.Value : 1;
                    }
                }
            }

            dbContext.SaveChanges();

            TempData["SuccessMessage"] = "Cart updated successfully.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveItem(int bookId)
        {
            var userId = User.Identity.Name;
            var cartItem = dbContext.CartItems.FirstOrDefault(c => c.BookId == bookId && c.UserId == userId);

            if (cartItem != null)
            {
                dbContext.CartItems.Remove(cartItem);
                dbContext.SaveChanges();
            }

            TempData["SuccessMessage"] = "Item removed from cart.";

            return RedirectToAction("Index");
        }

        public IActionResult Checkout()
        {
            var userId = User.Identity.Name;
            var cartItems = dbContext.CartItems.Include(c => c.Book).Where(c => c.UserId == userId).ToList();

            if (cartItems == null || !cartItems.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }

            return View(cartItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int bookId)
        {
            var book = dbContext.Books.FirstOrDefault(b => b.Id == bookId);
            if (book == null)
            {
                return NotFound();
            }

            var userId = User.Identity.Name;
            var existingCartItem = dbContext.CartItems.FirstOrDefault(c => c.BookId == bookId && c.UserId == userId);

            if (existingCartItem != null)
            {
                existingCartItem.Quantity++;
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

        [HttpPost("PlaceOrder")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder()
        {
            var userId = User.Identity.Name;
            var cartItems = dbContext.CartItems.Include(c => c.Book).Where(c => c.UserId == userId).ToList();
            var totalPrice = cartItems.Sum(c => c.Quantity * c.Book.Price);

            foreach (var item in cartItems)
            {
                var book = await dbContext.Books.FindAsync(item.BookId);
                if (book != null)
                {
                    if (book.Quantity < item.Quantity)
                    {
                        TempData["ErrorMessage"] = $"Not enough stock for {book.Title}. Available: {book.Quantity}";
                        return RedirectToAction("Index");
                    }
                    book.Quantity -= item.Quantity;
                    dbContext.Books.Update(book);
                }
            }

            await dbContext.SaveChangesAsync();

            ViewBag.TotalPrice = totalPrice;

            return View("ShippingInfo");
        }

        [HttpPost("CompleteOrder")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteOrder(ShippingInfo shippingInfo)
        {
            if (ModelState.IsValid)
            {
                // Process the order and save shipping information
                // You can add your order processing logic here

                // Clear the cart
                var userId = User.Identity.Name;
                var cartItems = dbContext.CartItems.Where(c => c.UserId == userId).ToList();
                dbContext.CartItems.RemoveRange(cartItems);
                await dbContext.SaveChangesAsync();

                return RedirectToAction("OrderConfirmation");
            }

            ViewBag.TotalPrice = dbContext.CartItems.Include(c => c.Book).Where(c => c.UserId == User.Identity.Name).Sum(c => c.Quantity * c.Book.Price);
            return View("ShippingInfo", shippingInfo);
        }

        [HttpGet("OrderConfirmation")]
        public IActionResult OrderConfirmation()
        {
            return View();
        }
    }
}