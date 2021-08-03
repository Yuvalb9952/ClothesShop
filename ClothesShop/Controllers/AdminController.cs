using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ClothesShop.Models;
using ClothesShop.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace ClothesShop.Controllers
{
    public class AdminController : Controller
    {
        private readonly ClothesShopContext _context;

        public AdminController(ClothesShopContext context) 
        {
            _context = context;
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
            ViewBag.Name = HttpContext.Session.GetString("UserName");
            return View();
        }

        #region Products

        public IActionResult Products()
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }

            List<Product> products = _context.Products.Include(p => p.Tags).ToList();
            List<Category> categories = _context.Categories.Where(cat => !cat.IsDeleted).ToList();
            List<Tag> tags = _context.Tags.Where(tag => !tag.IsDeleted).ToList();

            ViewBag.Products = products;
            ViewBag.Categories = categories;
            ViewBag.Tags = tags;
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

            return Redirect("/Admin/Products");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(string name, int category, int price, IFormFile img, Gender gender, List<int> tags)
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }

            string pathToSave = "";

            using (MemoryStream ms = new MemoryStream())
            {
                img.CopyTo(ms);
                var imageToSave = Convert.ToBase64String(ms.ToArray());
                pathToSave = "data:image/png;base64," + imageToSave;
            }

            Product newProduct = new Product()
            {
                Name = name,
                Category = _context.Categories.Single(c => c.Id == category),
                Price = price,
                ImageSrc = pathToSave,
                Gender = gender,
                Tags = _context.Tags.Where(t => tags.Contains(t.Id)).ToList()
            };

            _context.Add(newProduct);
            await _context.SaveChangesAsync();
            return Redirect("/Admin/Products");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id,
                                                      string name,
                                                      int category,
                                                      int price,
                                                      IFormFile img,
                                                      Gender gender,
                                                      List<int> tags)
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }

            Product productToEdit = _context.Products.Include(p => p.Tags).Single(p => p.Id == id);
            productToEdit.Name = name;
            productToEdit.Category = _context.Categories.Single(c => c.Id == category);
            productToEdit.Price = price;
            productToEdit.Gender = gender;

            if (tags?.Any() == true)
            {
                productToEdit.Tags.Clear();
                foreach (Tag tag in _context.Tags.Where(t => tags.Contains(t.Id)))
                {
                    productToEdit.Tags.Add(tag);
                }
            }

            if (img != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    img.CopyTo(ms);
                    var imageToSave = Convert.ToBase64String(ms.ToArray(), 0, ms.ToArray().Length);
                    productToEdit.ImageSrc = "data:image/png;base64," + imageToSave;
                }
            }

            _context.Update(productToEdit);
            await _context.SaveChangesAsync();
            return Redirect("/Admin/Products");
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

        [HttpPost]
        public IActionResult Customers(string customerName)
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

            List<CustomerViewModel> wantedCustomerViews = new List<CustomerViewModel>();

            if (customerName != null)
            {
                wantedCustomerViews = customerView.Where((customer) => customer.FullName == customerName).ToList();
            }
            else
            {
                wantedCustomerViews = customerView;
            }
            
            ViewBag.CustomersView = wantedCustomerViews;

            return View();
        }

        #endregion

        #region Tags

        public IActionResult Tags()
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }
            List<Tag> tags = _context.Tags.Where(cat => !cat.IsDeleted).ToList();
            ViewBag.Tags = tags;
            return View();
        }

        [HttpPost]
        public IActionResult Tags(string tagName)
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }
            List<Tag> tags;

            if (tagName != null)
            {
                tags = _context.Tags.Where((tag) => tag.Name == tagName).ToList();
            }
            else
            {
                tags = _context.Tags.ToList();
            }
            ViewBag.Tags = tags;

            return View();
        }

        public IActionResult RemoveTag(int id)
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }

            if (_context.Tags.Include(tag => tag.Products).FirstOrDefault(tag => tag.Id == id)?.Products?.Any() == false)
            {
                _context.Tags.Single(p => p.Id == id).IsDeleted = true;
                _context.SaveChanges();
            }
            else
                TempData["TagRemovalFailed"] = true;

            return Redirect("/Admin/Tags");
        }

        public IActionResult AddTag(string name)
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }

            if (name == null)
            {
                return Redirect("/Admin/Tags");
            }

            Category newTag = new Category() { Name = name };

            _context.Add(newTag);
            _context.SaveChanges();
            return Redirect("/Admin/Tags");
        }

        public IActionResult EditTag(int id, string name)
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }
            Tag tagToEdit = _context.Tags.Single(c => c.Id == id);
            tagToEdit.Name = name;

            _context.Update(tagToEdit);
            _context.SaveChanges();
            return Redirect("/Admin/Tags");
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

        [HttpPost]
        public IActionResult Admins(string adminName)
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }
            List<Admin> admins;

            if (adminName != null)
            {
                admins = _context.Admins.Where((admin) => admin.UserName == adminName).ToList();
            }
            else
            {
                admins = _context.Admins.ToList();
            }
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

        #region Categories

        public IActionResult Categories()
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }
            List<Category> categories = _context.Categories.Where(cat => !cat.IsDeleted).ToList();
            ViewBag.Categories = categories;
            return View();
        }

        [HttpPost]
        public IActionResult Categories(string categoryName)
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }
            List<Category> categories;

            if (categoryName != null)
            {
                categories = _context.Categories.Where((category) => category.Name== categoryName).ToList();
            }
            else
            {
                categories = _context.Categories.ToList();
            }
            ViewBag.Categories = categories;

            return View();
        }

        public IActionResult RemoveCategory(int id)
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }

            if (_context.Products.Where(product => !product.IsDeleted && product.Category.Id == id).Count() == 0)
            {
                _context.Categories.Single(p => p.Id == id).IsDeleted = true;
                _context.SaveChanges();
            }
            else
                TempData["CategoryRemovalFailed"] = true;

            return Redirect("/Admin/Categories");
        }

        public IActionResult AddCategory(string name)
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }

            if (name == null)
            {
                return Redirect("/Admin/Categories");
            }

            Category newCategory = new Category() { Name = name };

            _context.Add(newCategory);
            _context.SaveChanges();
            return Redirect("/Admin/Categories");
        }

        public IActionResult EditCategory(int id, string name)
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }
            Category categoryToEdit = _context.Categories.Single(c => c.Id == id);
            categoryToEdit.Name = name;

            _context.Update(categoryToEdit);
            _context.SaveChanges();
            return Redirect("/Admin/Categories");
        }
        #endregion

        #region Branches

        public IActionResult Branches()
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }
            List<Branch> branches = _context.Branches.ToList();
            ViewBag.Branches = branches;
            return View();
        }

        [HttpPost]
        public IActionResult Branches(string branchName)
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }
            List<Branch> branches;

            if (branchName != null)
            {
                branches = _context.Branches.Where((branch) => branch.BranchName == branchName).ToList();
            }
            else
            {
                branches = _context.Branches.ToList();
            }
            ViewBag.Branches = branches;

            return View();
        }

        public IActionResult RemoveBranch(int id)
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }
            _context.Remove(_context.Branches.Single(b => b.ID == id));
            _context.SaveChanges();
            return Redirect("/Admin/Branches");
        }

        public IActionResult EditBranch(int id, string name, string address, float x, float y)
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }
            Branch branchToEdit = _context.Branches.Single(b => b.ID == id);

            branchToEdit.BranchName = name;
            branchToEdit.AddressInfo = address;
            branchToEdit.LocationX = x;
            branchToEdit.LocationY = y;

            _context.Update(branchToEdit);
            _context.SaveChanges();
            return Redirect("/Admin/Branches");
        }

        public IActionResult AddBranch(int id, string name, string address, float x, float y)
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }
            Branch branchToEdit = new Branch()
            {
                BranchName = name,
                AddressInfo = address,
                LocationX = x,
                LocationY = y
            };

            _context.Add(branchToEdit);
            _context.SaveChanges();
            return Redirect("/Admin/Branches");
        }

        #endregion

        public IActionResult Statistics()
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }

            return View();
        }

        public IActionResult FacebookPost()
        {
            if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }

            return View();
        }
    }
}
