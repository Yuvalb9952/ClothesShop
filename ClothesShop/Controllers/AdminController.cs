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

        public class CustomerViewModel
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

        #region Customers
        public IActionResult Customers()
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }

            List<Customer> customers = _context.Customers.ToList();
            List<Order> orders = _context.Orders.ToList();
            List<CustomerViewModel> customerView = orders.GroupBy(g => g.Customer.Email)
                .Select(g =>
                {
                    var customer = customers.Single(c => c.Email == g.Key);
                    return new CustomerViewModel
                    {
                        FullName = $"{customer.FirstName} {customer.LastName}",
                        Gender = customer.Gender,
                        Email = customer.Email,
                        OrdersNumber = g.Count()
                    };
                }).ToList();

            ViewBag.CustomersView = customerView;
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

        #region Orders

        public IActionResult Orders()
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }
            List<Order> orders = _context.Orders.Include("Customer").ToList();
            List<OrderStatus> statuses = _context.OrderStatuses.ToList();
            ViewBag.Orders = orders;
            ViewBag.statuses = statuses;

            return View();
        }

        public IActionResult UpdateOrderStatus(int orderId, string id)
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }

            try
            {
                List<Order> orders = _context.Orders.Include("Customer").ToList();
                Order orderToEdit = orders.Single(Order => Order.Id == orderId);

                OrderStatus orderStatus = _context.OrderStatuses.Single(s => s.Id == int.Parse(id));
                orderToEdit.Status = orderStatus;

                _context.Update(orderToEdit);
                _context.SaveChanges();

                return Redirect("/Admin/Orders");
            }
            catch (Exception)
            {
                return Redirect("/Admin/Orders");
            }
        }

        [HttpPost]
        public IActionResult Orders(int orderId, int orderStatus, DateTime? orderDate)
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }
            List<Order> orders;

            if (orderId != 0)
            {
                orders = _context.Orders.Where((order) => order.Id == orderId).Include("Customer").ToList();
            }
            else if (orderId == 0 && orderStatus == 0 && orderDate == null)
            {
                orders = _context.Orders.Include("Customer").ToList();
            }
            else
            {
                orders = _context.Orders.Where((order) => (orderStatus != 0 && orderDate != null && orderStatus == order.Status.Id) ||
                        (orderStatus != 0 && orderStatus == order.Status.Id) ||
                        (orderDate != null && orderDate.Equals(order.OrderDate))).Include("Customer").ToList();
            }

            List<OrderStatus> statuses = _context.OrderStatuses.ToList();

            ViewBag.Orders = orders;
            ViewBag.statuses = statuses;

            return View();
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
