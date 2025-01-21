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

        // Calculate total quantity and price for display
        ViewBag.TotalQuantity = cart.Sum(c => c.Quantity);
        ViewBag.TotalPrice = cart.Sum(c => c.Quantity * c.Book.Price);

        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateCart(Dictionary<int, int> quantities, int? RemoveItem)
    {
        // Retrieve the cart from the session
        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

        // If the cart is empty, redirect to the index with an info message
        if (cart.Count == 0)
        {
            TempData["InfoMessage"] = "Your cart is empty.";
            return RedirectToAction("Index");
        }

        // Validate quantities parameter
        if (quantities == null || !quantities.Any())
        {
            TempData["ErrorMessage"] = "Invalid cart update request.";
            return RedirectToAction("Index");
        }

        bool cartUpdated = false;

        // If a book is to be removed
        if (RemoveItem.HasValue)
        {
            var itemToRemove = cart.FirstOrDefault(c => c.Book.Id == RemoveItem.Value);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                cartUpdated = true;
            }
        }

        // Update quantities for items in the cart
        foreach (var item in quantities)
        {
            var cartItem = cart.FirstOrDefault(c => c.Book.Id == item.Key);
            if (cartItem != null)
            {
                cartItem.Quantity = item.Value > 0 ? item.Value : 1; // Ensure quantity is at least 1
                cartUpdated = true;
            }
        }

        // Save the updated cart
        HttpContext.Session.SetObjectAsJson("Cart", cart);

        // Calculate total quantity and total price
        ViewBag.TotalQuantity = cart.Sum(c => c.Quantity);
        ViewBag.TotalPrice = cart.Sum(c => c.Quantity * c.Book.Price);

        // Set messages
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
}
