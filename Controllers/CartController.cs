using Microsoft.AspNetCore.Mvc;
using BookShopTest.Data;
using BookShopTest.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class CartController : Controller
{
    private readonly ApplicationDbContext _context;

    public CartController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

        // Calculate total quantity and price for display
        ViewBag.TotalQuantity = cart.Sum(c => c.Quantity);
        ViewBag.TotalPrice = cart.Sum(c => c.Quantity * c.Book.Price);

        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateCart(Dictionary<int, int> quantities, int? RemoveItem)
    {
        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

        if (cart.Count == 0)
        {
            TempData["InfoMessage"] = "Your cart is empty.";
            return RedirectToAction("Index");
        }

        if (quantities == null || !quantities.Any())
        {
            TempData["ErrorMessage"] = "Invalid cart update request.";
            return RedirectToAction("Index");
        }

        bool cartUpdated = false;

        if (RemoveItem.HasValue)
        {
            var itemToRemove = cart.FirstOrDefault(c => c.Book.Id == RemoveItem.Value);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                cartUpdated = true;
            }
        }

        foreach (var item in quantities)
        {
            var cartItem = cart.FirstOrDefault(c => c.Book.Id == item.Key);
            if (cartItem != null)
            {
                cartItem.Quantity = item.Value > 0 ? item.Value : 1;
                cartUpdated = true;
            }
        }

        HttpContext.Session.SetObjectAsJson("Cart", cart);

        ViewBag.TotalQuantity = cart.Sum(c => c.Quantity);
        ViewBag.TotalPrice = cart.Sum(c => c.Quantity * c.Book.Price);

        if (cartUpdated)
        {
            TempData["SuccessMessage"] = "Cart updated successfully.";
        }
        else
        {
            TempData["InfoMessage"] = "No changes were made.";
        }

        return RedirectToAction("Index");
    }

    // New Checkout action
    public IActionResult Checkout()
    {
        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

        if (cart.Count == 0)
        {
            TempData["ErrorMessage"] = "Your cart is empty.";
            return RedirectToAction("Index");
        }

        return View(cart);
    }

    // New PlaceOrder action
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder()
    {
        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

        if (cart.Count == 0)
        {
            TempData["ErrorMessage"] = "Your cart is empty.";
            return RedirectToAction("Index");
        }

        var order = new Order
        {
            UserId = User.Identity.Name,
            OrderDate = DateTime.Now,
            TotalAmount = cart.Sum(c => c.Quantity * c.Book.Price),
            OrderStatus = "Pending",
            OrderItems = cart.Select(c => new OrderItem
            {
                BookId = c.BookId,
                Quantity = c.Quantity,
                UnitPrice = c.Book.Price,
                TotalPrice = c.Quantity * c.Book.Price
            }).ToList()
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Clear the cart
        HttpContext.Session.SetObjectAsJson("Cart", new List<CartItem>());

        TempData["SuccessMessage"] = "Order placed successfully!";
        return RedirectToAction("Details", "Orders", new { id = order.OrderId });
    }
}