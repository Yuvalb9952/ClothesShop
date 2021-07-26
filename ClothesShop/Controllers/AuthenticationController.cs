using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace ClothesShop.Controllers
{
    public class AuthenticationController : Controller
    {
        public IActionResult SignIn()
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Welcome", "Admin", null);
            }
        }
    }
}
