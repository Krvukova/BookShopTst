using Microsoft.AspNetCore.Mvc;
using BookShopTest.Models.Entities;
using System.Collections.Generic;
using System.Linq;
using BookShopTest.Models;

public class CartController : Controller
{
    public IActionResult Index()
    {
        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
        return View(cart);
    }

    [HttpPost]
    public IActionResult UpdateCart(Dictionary<int, int> Quantities, int? RemoveItem)
    {
        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

        // If a book is to be removed, remove it from the cart
        if (RemoveItem.HasValue)
        {
            cart.RemoveAll(c => c.Book.Id == RemoveItem.Value);
        }

        // Update the quantities of the books in the cart
        foreach (var item in Quantities)
        {
            var cartItem = cart.FirstOrDefault(c => c.Book.Id == item.Key);
            if (cartItem != null && item.Value > 0)
            {
                cartItem.Quantity = item.Value;
            }
        }

        // Save the updated cart back to the session
        HttpContext.Session.SetObjectAsJson("Cart", cart);

        return RedirectToAction("Index");
    }
}
