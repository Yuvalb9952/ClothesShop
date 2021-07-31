using ClothesShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ClothesShop.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ClothesShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ClothesShopContext _context;
        public HomeController(ClothesShopContext context,ILogger<HomeController> logger)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Contact()
        {
            ViewBag.Admins = _context.Admins.ToList();
            ViewBag.Branches = _context.Branches.ToList();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult Shop()
        {
            List<Product> products = _context.Products.ToList();
            ViewBag.Products = products;

            List<Category> categories = _context.Categories.Where(cat => !cat.IsDeleted).ToList();
            ViewBag.Categories = categories;

            ViewBag.productsInBagCount = getProductsInBagCount();

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Shop(string searchText, int category, int minPrice, int maxPrice)
        {
            List<Category> categories = _context.Categories.Where(cat => !cat.IsDeleted).ToList();
            ViewBag.Categories = categories;

            List<Product> products;

            if (searchText != null)
            {
                products = _context.Products.Where((p) => p.Name.Contains(searchText)).Include(product => product.Category).ToList();
            }
            else
            {
                products = _context.Products.Include(product => product.Category).ToList();
            }

            if (category != 0)
            {
                products = products.Where((p) => p.Category.Id == category).ToList();
            }

            if (maxPrice <= 0)
            {
                maxPrice = int.MaxValue;
            }

            products = products.Where((p) => p.Price >= minPrice && p.Price <= maxPrice).ToList();

            ViewBag.Products = products;

            ViewBag.productsInBagCount = getProductsInBagCount();

            return View();
        }

        private dynamic getProductsInBagCount()
        {
            var keys = HttpContext.Session.Keys.Where(key => key != "adminId" && key != "fullName");
            int productsInBagCount = 0;
            foreach (var key in keys)
            {
                productsInBagCount += int.Parse(HttpContext.Session.GetString(key));
            }

            return productsInBagCount;
        }
    }
}
