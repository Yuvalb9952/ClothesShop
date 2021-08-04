using ClothesShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ClothesShop.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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
            ViewBag.CartSize = getAmountOfProductsInCart();

            return View();
        }

        public IActionResult Contact()
        {
            ViewBag.Admins = _context.Admins.ToList();
            ViewBag.Branches = _context.Branches.ToList();
            ViewBag.CartSize = getAmountOfProductsInCart();


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
            List<Product> products = _context.Products.Include(p => p.Tags).Include(p => p.Category).ToList();
            ViewBag.Products = products;

            List<Category> categories = _context.Categories.Where(cat => !cat.IsDeleted).ToList();
            ViewBag.Categories = categories;

            List<Tag> tags = _context.Tags.Where(cat => !cat.IsDeleted).ToList();
            ViewBag.tags = tags;

            ViewBag.CartSize = getAmountOfProductsInCart();

            return View();
        }


        [HttpPost]
        public IActionResult Shop(string searchText, int category, int tag, int minPrice, int maxPrice, int gender)
        {
            List<Category> categories = _context.Categories.Where(cat => !cat.IsDeleted).ToList();
            ViewBag.Categories = categories;

            List<Tag> tags = _context.Tags.Where(cat => !cat.IsDeleted).ToList();
            ViewBag.tags = tags;

            List<Product> products;

            if (searchText != null)
            {
                products = _context.Products.Include(p => p.Tags).Include(p => p.Category).Where((p) => p.Name.Contains(searchText)).ToList();
            }
            else
            {
                products = _context.Products.Include(p => p.Tags).Include(p => p.Category).ToList();
            }

            if (category != 0)
            {
                products = products.Where((p) => p.Category.Id == category).ToList();
            }
            if (tag != 0 )
            {
                products = products.Where((p) => p.Tags.Any(a => tag == a.Id)).ToList();
            }

            if (gender != 0)
            {
                products = products.Where((p) => p.Gender == (gender == 1 ? Gender.Male : Gender.Female)).ToList();
            }

            if (maxPrice <= 0)
            {
                maxPrice = int.MaxValue;
            }

            products = products.Where((p) => p.Price >= minPrice && p.Price <= maxPrice).ToList();

            ViewBag.Products = products;
            ViewBag.CartSize = getAmountOfProductsInCart();

            return View();
        }
        private dynamic getAmountOfProductsInCart()
        {
            var keys = HttpContext.Session.Keys.Where(key => key != "adminId" && key != "UserName");
            int CartSize = 0;
            foreach (var key in keys)
            {
                var productMD = JsonConvert.DeserializeObject<List<ProductMetaData>>(HttpContext.Session.GetString(key));
                foreach (var product in productMD)
                {
                    CartSize += product.Quantity;
                }
            }

            return CartSize;
        }

        public IActionResult About()
        {
            ViewBag.CartSize = getAmountOfProductsInCart();

            return View();
        }

        public IActionResult NotFound()
        {
            return View();
        }
    }
}
