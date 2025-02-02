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
        public IActionResult UpdateCartItem(int bookId, int quantity)
        {
            var userId = User.Identity.Name;
            var cartItem = dbContext.CartItems.Include(c => c.Book).FirstOrDefault(c => c.BookId == bookId && c.UserId == userId);

            if (cartItem != null)
            {
                var book = dbContext.Books.FirstOrDefault(b => b.Id == cartItem.BookId);
                if (book != null)
                {
                    if (quantity > book.Quantity)
                    {
                        TempData["ErrorMessage"] = $"Only {book.Quantity} copies of {book.Title} are available for purchase.";
                        return RedirectToAction("Index");
                    }
                    cartItem.Quantity = quantity > 0 ? quantity : 1;
                    dbContext.SaveChanges();
                    TempData["SuccessMessage"] = "Cart updated successfully.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Item not found in cart.";
            }

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
                TempData["SuccessMessage"] = "Item removed from cart.";
            }
            else
            {
                TempData["ErrorMessage"] = "Item not found in cart.";
            }

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
                    return Redirect(returnUrl ?? "/Books/Details/" + bookId);
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
                        return Redirect(returnUrl ?? "/Books/Details/" + bookId);
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
                
            }

            return Redirect(returnUrl ?? "/Books/Details/" + bookId);
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
                }
            }

            ViewBag.TotalPrice = totalPrice;
            return View("ShippingInfo");
        }

        [HttpPost("CompleteOrder")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteOrder(ShippingInfo shippingInfo)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.Name;
                var cartItems = dbContext.CartItems.Include(c => c.Book).Where(c => c.UserId == userId).ToList();
                var totalPrice = cartItems.Sum(c => c.Quantity * c.Book.Price);

                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.Now,
                    TotalAmount = totalPrice,
                    OrderStatus = "Pending",
                    OrderItems = cartItems.Select(c => new OrderItem
                    {
                        BookId = c.BookId,
                        Quantity = c.Quantity,
                        Price = c.Book.Price
                    }).ToList()
                };

                dbContext.Orders.Add(order);

                foreach (var item in cartItems)
                {
                    var book = await dbContext.Books.FindAsync(item.BookId);
                    if (book != null)
                    {
                        book.Quantity -= item.Quantity;
                        dbContext.Books.Update(book);
                    }
                }

                await dbContext.SaveChangesAsync();

                
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