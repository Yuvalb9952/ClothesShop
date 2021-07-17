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
        private readonly IWebHostEnvironment webHostEnvironment;

        public AdminController(ClothesShopContext context, IWebHostEnvironment iwebHostEnvironment) 
        {
            _context = context;
            webHostEnvironment = iwebHostEnvironment;
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

        #region Products

        public IActionResult EditProducts()
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }

            List<Product> products = _context.Products.ToList();
            List<Category> categories = _context.Categories.Where(cat => !cat.IsDeleted).ToList();

            ViewBag.Products = products;
            ViewBag.Categories = categories;
            return View();
        }

        public IActionResult RemoveProduct(int id)
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }

            _context.Products.Single(p => p.Id == id).IsDeleted = true;

            try
            {
                _context.SaveChanges();
            }
            catch (Exception)
            {
                TempData["ProductRemovalFailed"] = true;
            }

            return Redirect("/Admin/EditProducts");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(string name, int category, int price, IFormFile img)
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }

            string pathToSave = await SaveImageFile(img, name);

            Product newProduct = new Product()
            {
                Name = name,
                Category = _context.Categories.Single(c => c.Id == category),
                Price = price,
                ImageSrc = pathToSave
            };

            _context.Add(newProduct);
            await _context.SaveChangesAsync();
            return Redirect("/Admin/EditProducts");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id,
                                                     string name,
                                                     int category,
                                                     int price,
                                                     IFormFile img)
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }

            Product productToEdit = _context.Products.Single(p => p.Id == id);
            productToEdit.Name = name;
            productToEdit.Category = _context.Categories.Single(c => c.Id == category);
            productToEdit.Price = price;

            if (img != null)
            {
                productToEdit.ImageSrc = await SaveImageFile(img, name);
            }

            _context.Update(productToEdit);
            await _context.SaveChangesAsync();
            return Redirect("/Admin/EditProducts");
        }

        #endregion

        #region Costumers
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

        #endregion

        #region Admins
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

        #endregion

        private async Task<string> SaveImageFile(IFormFile img, string name)
        {
            // Create a File Info 
            FileInfo fi = new FileInfo(img.FileName);

            // This code creates a unique file name to prevent duplications 
            // stored at the file location
            var newFilename = name + "_" + String.Format("{0:d}",
                              (DateTime.Now.Ticks / 10) % 100000000) + fi.Extension;
            var webPath = webHostEnvironment.WebRootPath;
            var path = Path.Combine("", webPath + @"\ImageFiles\" + newFilename);

            // IMPORTANT: The pathToSave variable will be save on the column in the database
            var pathToSave = @"/ImageFiles/" + newFilename;

            // This stream the physical file to the allocate wwwroot/ImageFiles folder
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await img.CopyToAsync(stream);
            }

            return pathToSave;
        }

    }
}
