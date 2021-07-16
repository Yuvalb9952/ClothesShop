using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ClothesShop.Models;
using ClothesShop.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace final_project.Controllers
{
    public class AdminController : Controller
    {
        private readonly ClothesShopContext _context;

        public AdminController(ClothesShopContext context)
        {
            _context = context;
        }

        public class CostumerViewModel
        {
            public string FullName { get; set; }
            public string Gender { get; set; }
            public string Email { get; set; }
            public int OrdersNumber { get; set; }
        }

        public IActionResult Welcome()
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }
            ViewBag.Name = HttpContext.Session.GetString("userName");
            return View();
        }

        public IActionResult Costumers()
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }

            List<Customer> costumers = _context.Costumers.ToList();
            List<Order> orders = _context.Orders.ToList();
            List<CostumerViewModel> costumerView = orders.GroupBy(g => g.Customer.Email)
                .Select(g =>
                {
                    var costumer = costumers.Single(c => c.Email == g.Key);
                    return new CostumerViewModel
                    {
                        FullName = $"{costumer.FirstName} {costumer.LastName}",
                        Gender = costumer.Gender,
                        Email = costumer.Email,
                        OrdersNumber = g.Count()
                    };
                }).ToList();

            ViewBag.CostumersView = costumerView;
            return View();
        }

        public IActionResult Admins()
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }
            List<Admin> admins = _context.Admins.ToList();
            ViewBag.Admins = admins;
            ViewBag.SelfAdminId = HttpContext.Session.GetInt32("adminId");
            return View();
        }

        public IActionResult RemoveAdmin(int id)
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }
            _context.Remove(_context.Admins.Single(b => b.Id == id));
            _context.SaveChanges();
            return Redirect("/Admin/Admins");
        }

        public IActionResult EditAdmin(int id, string userName, string email, string password)
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }
            Admin adminToEdit = _context.Admins.Single(b => b.Id == id);

            adminToEdit.UserName = userName;
            adminToEdit.Email = email;
            adminToEdit.Password = password;

            _context.Update(adminToEdit);
            _context.SaveChanges();
            return Redirect("/Admin/Admins");
        }

        public IActionResult AddAdmin(string userName, string email, string password)
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }
            Admin AdminToAdd = new Admin()
            {
                UserName = userName,
                Email = email,
                Password = password
            };

            _context.Add(AdminToAdd);
            _context.SaveChanges();
            return Redirect("/Admin/Admins");
        }

    }
}
