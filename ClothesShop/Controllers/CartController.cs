using Accord.MachineLearning.Rules;
using ClothesShop.Data;
using ClothesShop.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClothesShop.Controllers
{
    public class CartController : Controller
    {
        private readonly ClothesShopContext _context;

        public CartController(ClothesShopContext context)
        {
            _context = context;
        }

        // GET: /<controller>/
        public IActionResult Cart()
        {
            var keys = HttpContext.Session.Keys.Where(key => key != "adminId" && key != "UserName");
            Dictionary<int, List<ProductMetaData>> productsMD = new Dictionary<int, List<ProductMetaData>>();
            List<Product> productsInBag = _context.Products.Where(x => keys.Contains(x.Id.ToString())).Include(product => product.Category).Include(product => product.Tags).ToList();
            ViewBag.productsInBag = productsInBag;

            foreach (var key in keys)
            {
                productsMD.Add(int.Parse(key), JsonConvert.DeserializeObject<List<ProductMetaData>>(HttpContext.Session.GetString(key)));
            }

            ViewBag.statistics = GetRecommendedProducts();
            ViewBag.productsMD = productsMD;
            ViewBag.productSizesList = new List<Size>() { Size.Small, Size.Medium, Size.Large };
            ViewBag.CartSize = getAmountOfProductsInCart();

            return View();
        }

        [HttpPost]
        public IActionResult AddToCart(int id, Size size, int quantity)
        {
            if (HttpContext.Session.GetString(id.ToString()) == null)
            {
                var mdList = new List<ProductMetaData>() { new ProductMetaData() { Quantity = quantity, Size = size } };
                HttpContext.Session.SetString(id.ToString(), JsonConvert.SerializeObject(mdList));
            }
            else
            {
                var mdList = JsonConvert.DeserializeObject<List<ProductMetaData>>(HttpContext.Session.GetString(id.ToString()));

                if (mdList.Exists(md => md.Size == size))
                {
                    mdList.Find(md => md.Size == size).Quantity += quantity;
                } 
                else
                {
                    mdList.Add(new ProductMetaData() { Quantity = quantity, Size = size });
                }

                HttpContext.Session.SetString(id.ToString(), JsonConvert.SerializeObject(mdList));
            }
            return RedirectToAction("Shop", "Home");
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

        [HttpPost]
        public async Task<IActionResult> AddRecommendationToCart(int id, Size size, int quantity)
        {
            if (HttpContext.Session.GetString(id.ToString()) == null)
            {
                var mdList = new List<ProductMetaData>() { new ProductMetaData() { Quantity = quantity, Size = size } };
                HttpContext.Session.SetString(id.ToString(), JsonConvert.SerializeObject(mdList));
            }
            else
            {
                var mdList = JsonConvert.DeserializeObject<List<ProductMetaData>>(HttpContext.Session.GetString(id.ToString()));

                if (mdList.Exists(md => md.Size == size))
                {
                    mdList.Find(md => md.Size == size).Quantity += quantity;
                }
                else
                {
                    mdList.Add(new ProductMetaData() { Quantity = quantity, Size = size });
                }

                HttpContext.Session.SetString(id.ToString(), JsonConvert.SerializeObject(mdList));
            }
            return RedirectToAction("Cart", "Cart");
        }

        public IActionResult DeleteProduct(int id, Size size)
        {
            var mdList = JsonConvert.DeserializeObject<List<ProductMetaData>>(HttpContext.Session.GetString(id.ToString()));

            if (mdList?.Count > 1)
            {
                mdList = mdList.Where(md => md.Size != size).ToList();
                HttpContext.Session.SetString(id.ToString(), JsonConvert.SerializeObject(mdList));
            } 
            else
            {
                HttpContext.Session.Remove(id.ToString());
            }
            return RedirectToAction("Cart", "Cart");
        }

        public IEnumerable<Product> GetCartFormSession()
        {
            var cartIDs = HttpContext.Session.Keys.Where(id => int.TryParse(id, out var num)).Select(int.Parse);
            return _context.Products.Include(prod => prod.Category).Include(prod => prod.Tags).Where(product => cartIDs.Contains(product.Id));
        }

        public List<Product> GetRecommendedProducts()
        {
            Product[] productToReturn = GetCartFormSession().ToArray();
            Apriori<Product> apriori = new Apriori<Product>(0, 0);

            // Get the Models
            List<Order> orders = _context.Orders.ToList();
            List<Product> products = _context.Products.Include(product => product.Category).Include(product => product.Tags).ToList();
            List<OrderItem> orderItems = _context.OrderItems.ToList();
            List<Category> categories = _context.Categories.ToList();

            // Group the oredered products by orders
            List<IGrouping<int, OrderItem>> groupItems = orderItems.GroupBy(o => o.OrderId).ToList();

            // Define the sorted set for the learning algorithm
            SortedSet<Product>[] productSets = new SortedSet<Product>[groupItems.Count];

            int i = 0;

            // Initialize the sorted set for the learning algorithm
            foreach (IGrouping<int, OrderItem> group in groupItems)
            {
                productSets[i] = new SortedSet<Product>();

                foreach (OrderItem item in group)
                {
                    productSets[i].Add(item.Product);
                }

                i++;
            }

            // Execute the learning algorithm and get the rules' object to get the sugguestions from
            AssociationRuleMatcher<Product> productsRules = apriori.Learn(productSets);

            // Execute the suggustion function with the products chosen by the client => 'Decide'
            Product[][] decideProd = productsRules.Decide(productToReturn);

            if (decideProd.Length == 0)
                return _context.Products.Where(prod => !prod.IsDeleted).Include(product => product.Category).Include(product => product.Tags).Take(3).ToList();

            List<Product> recommended = decideProd[0].Where(prod => !prod.IsDeleted).ToList();
            if (recommended.Count > 0)
                // Return the first row of the suggestion - the most fit suggesion (can return the whole suggestions instead)
                return decideProd[0].Where(prod => !prod.IsDeleted).ToList();
            else
                return _context.Products.Where(prod => !prod.IsDeleted).Include(product => product.Category).Include(product => product.Tags).Take(3).ToList();
        }
    }
}
