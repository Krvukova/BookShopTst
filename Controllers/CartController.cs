using Microsoft.AspNetCore.Mvc;
using BookShopTest.Data;
using BookShopTest.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class CartController : Controller
{
    private readonly ApplicationDbContext _context;

    public CartController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var userId = User.Identity.Name;
        var cartItems = _context.CartItems.Include(c => c.Book).Where(c => c.UserId == userId).ToList();

        ViewBag.TotalQuantity = cartItems.Sum(c => c.Quantity);
        ViewBag.TotalPrice = cartItems.Sum(c => c.Quantity * c.Book.Price);

        return View(cartItems);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateCart(Dictionary<int, int> quantities, int? RemoveItem)
    {
        var userId = User.Identity.Name;
        var cartItems = _context.CartItems.Where(c => c.UserId == userId).ToList();

        if (RemoveItem.HasValue)
        {
            var itemToRemove = cartItems.FirstOrDefault(c => c.BookId == RemoveItem.Value);
            if (itemToRemove != null)
            {
                _context.CartItems.Remove(itemToRemove);
            }
        }

        foreach (var item in quantities)
        {
            var cartItem = cartItems.FirstOrDefault(c => c.BookId == item.Key);
            if (cartItem != null)
            {
                cartItem.Quantity = item.Value > 0 ? item.Value : 1;
            }
        }

        _context.SaveChanges();

        TempData["SuccessMessage"] = "Cart updated successfully.";

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveItem(int bookId)
    {
        var userId = User.Identity.Name;
        var cartItem = _context.CartItems.FirstOrDefault(c => c.BookId == bookId && c.UserId == userId);

        if (cartItem != null)
        {
            _context.CartItems.Remove(cartItem);
            _context.SaveChanges();
        }

        TempData["SuccessMessage"] = "Item removed from cart.";

        return RedirectToAction("Index");
    }

    public IActionResult Checkout()
    {
        var userId = User.Identity.Name;
        var cartItems = _context.CartItems.Include(c => c.Book).Where(c => c.UserId == userId).ToList();

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
        var book = _context.Books.FirstOrDefault(b => b.Id == bookId);
        if (book == null)
        {
            return NotFound();
        }

        var userId = User.Identity.Name;
        var existingCartItem = _context.CartItems.FirstOrDefault(c => c.BookId == bookId && c.UserId == userId);

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
            _context.CartItems.Add(cartItem);
        }

        _context.SaveChanges();

        TempData["CartMessage"] = "Book has been added to cart";

        return RedirectToAction("Index");
    }
}