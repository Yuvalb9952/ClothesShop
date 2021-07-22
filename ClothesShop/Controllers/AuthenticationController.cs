using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ClothesShop.Models;
using Microsoft.AspNetCore.Http;

namespace final_project.Controllers
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
