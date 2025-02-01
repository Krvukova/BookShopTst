using BookShopTest.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BookShopTest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Index", "Books");
        }

        public IActionResult ContactUs()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ContactUs(ContactUsModel model)
        {
            if (ModelState.IsValid)
            {
                TempData["SuccessMessage"] = "Your message has been sent successfully.";
                return RedirectToAction("ContactUs");
            }

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult UserSettings()
        {
            return View();
        }
    }
}
