using Microsoft.AspNetCore.Mvc;
using BookShopTest.Models.Entities;
using System.Collections.Generic;

public class CartController : Controller
{
    public IActionResult Index()
    {
        // Retrieve the cart from session
        var cart = HttpContext.Session.GetObjectFromJson<List<Book>>("Cart") ?? new List<Book>();
        return View(cart);
    }
}
