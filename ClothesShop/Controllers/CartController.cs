using Accord.MachineLearning.Rules;
using ClothesShop.Data;
using ClothesShop.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var keys = HttpContext.Session.Keys.Where(key => key != "adminId" && key != "fullName");
            Dictionary<int, int> quantites = new Dictionary<int, int>();
            List<Product> productsInBag = _context.Products.Where(x => keys.Contains(x.Id.ToString())).Include(product => product.Category).ToList();
            ViewBag.productsInBag = productsInBag;

            foreach (var key in keys)
            {
                quantites.Add(int.Parse(key), int.Parse(HttpContext.Session.GetString(key)));
            }

            ViewBag.statistics = GetRecommendedProducts();
            ViewBag.quantites = quantites;
            ViewBag.productSizesList = new List<Size>() { Size.Small, Size.Medium, Size.Large };

            return View();
        }

        [HttpPost]
        public IActionResult AddToCart(int id, int quantity)
        {
            if (HttpContext.Session.GetString(id.ToString()) == null)
            {
                HttpContext.Session.SetString(id.ToString(), quantity.ToString());
            }
            else
            {
                var newQuantity = int.Parse(HttpContext.Session.GetString(id.ToString())) + quantity;
                HttpContext.Session.SetString(id.ToString(), newQuantity.ToString());
            }
            return RedirectToAction("Shop", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> AddRecommendationToCart(int id, Size size, int quantity)
        {
            if (HttpContext.Session.GetString(id.ToString()) == null)
            {
                HttpContext.Session.SetString(id.ToString(), quantity.ToString());
            }
            else
            {
                var newQuantity = int.Parse(HttpContext.Session.GetString(id.ToString())) + quantity;
                HttpContext.Session.SetString(id.ToString(), newQuantity.ToString());
            }

            Product productToEdit = _context.Products.Single(p => p.Id == id);
            productToEdit.Size = size;
            _context.Update(productToEdit);
            await _context.SaveChangesAsync();

            return RedirectToAction("Cart", "Cart");
        }

        public IActionResult DeleteProduct(int id)
        {
            HttpContext.Session.Remove(id.ToString());
            return RedirectToAction("Cart", "Cart");
        }

        public IEnumerable<Product> GetCartFormSession()
        {
            var cartIDs = HttpContext.Session.Keys.Where(id => int.TryParse(id, out var num)).Select(int.Parse);
            return _context.Products.Include(prod => prod.Category).Where(product => cartIDs.Contains(product.Id));
        }

        public List<Product> GetRecommendedProducts()
        {
            Product[] productToReturn = GetCartFormSession().ToArray();
            Apriori<Product> apriori = new Apriori<Product>(0, 0);

            // Get the Models
            List<Order> orders = _context.Orders.ToList();
            List<Product> products = _context.Products.Include(product => product.Category).ToList();
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
                return _context.Products.Where(prod => !prod.IsDeleted).Include(product => product.Category).Take(3).ToList();

            List<Product> recommended = decideProd[0].Where(prod => !prod.IsDeleted).ToList();
            if (recommended.Count > 0)
                // Return the first row of the suggestion - the most fit suggesion (can return the whole suggestions instead)
                return decideProd[0].Where(prod => !prod.IsDeleted).ToList();
            else
                return _context.Products.Where(prod => !prod.IsDeleted).Include(product => product.Category).Take(3).ToList();
        }
    }
}
