using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ClothesShop.Data;
using Microsoft.AspNetCore.Http;

namespace ClothesShop.Controllers
{
    public class UserController : Controller
    {
        private readonly ClothesShopContext _context;

        public UserController(ClothesShopContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (email == null || password == null)
            {
                TempData["LoginFailed"] = true;
                return Redirect("/Authentication/SignIn");
            }

            var admin = _context.Admins.SingleOrDefault(u => u.Email == email && u.Password == password);

            if (admin == null)
            {
                TempData["LoginFailed"] = true;
                return Redirect("/Authentication/SignIn");
            }

            HttpContext.Session.SetInt32("adminId", admin.Id);
            HttpContext.Session.SetString("UserName", admin.UserName);

            return RedirectToAction("Welcome", "Admin", null);
        }

        public IActionResult Logout()
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }

            HttpContext.Session.Remove("adminId");
            return RedirectToAction("Index", "Home", null);
        }
    }
}
