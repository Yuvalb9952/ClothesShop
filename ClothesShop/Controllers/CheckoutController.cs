using ClothesShop.Data;
using ClothesShop.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ClothesShop.Controllers
{
    public class CheckoutController : Controller
    {
        private const string BaseCurrencyApiURL = "https://api.exchangerate.host/latest?base=ILS&symbols=";
        private readonly ClothesShopContext _context;

        public Currency CurrentCurrency { get; set; }

        public double CurrencyExchangeRate { get; set; }

        public List<OrderItem> Cart { get; set; }

        public CheckoutController(ClothesShopContext context)
        {
            _context = context;
        }

        // GET: /<controller>/{choosenCurrency}
        public async Task<IActionResult> Checkout(List<string> invalidFieldsList, Currency choosenCurrency = Currency.ILS)
        {
            Cart = GetCartFormSession();
            await UpdateCurrency(choosenCurrency);
            ViewBag.CurrentCurrency = CurrentCurrency;
            ViewBag.Cart = Cart;
            ViewBag.ConvertToCurrentCurrency = new Func<double, double>(ConvertToCurrentCurrency);
            ViewBag.CartSum = ConvertToCurrentCurrency(GetCartSum());
            int cartSize = 0;
            foreach (OrderItem orderItem in Cart)
            {
                cartSize += orderItem.Quantity;
            }
            ViewBag.CartSize = cartSize;
            ViewBag.invalidFieldsList = invalidFieldsList;
            ViewBag.GetInputClass = new Func<string, string>(GetInputClass);

            return View();
        }

        [HttpPost]
        public IActionResult AddOrder(Order order)
        {
            Cart = GetCartFormSession();

            var invalidFields = new List<string>();

            if (order.Customer.FirstName == null) invalidFields.Add("FirstName");

            if (order.Customer.LastName == null) invalidFields.Add("LastName");

            if (order.Customer.Email == null || !(new EmailAddressAttribute().IsValid(order.Customer.Email))) invalidFields.Add("Email");

            if (order.Address == null) invalidFields.Add("Address");

            if (order.Zip == null || order.Zip?.Length != 7) invalidFields.Add("Zip");

            if (order.CreditCardName == null) invalidFields.Add("CreditCardName");

            if (order.CreditCardNumber == null || order.CreditCardNumber?.Length != 16) invalidFields.Add("CreditCardNumber");

            if (order.CreditCardExpiration == null || !Regex.IsMatch(order.CreditCardExpiration, @"([0][1-9]|[1][0-2])/\d{2}")) invalidFields.Add("CreditCardExpiration");

            if (order.CreditCardCVV == null || order.CreditCardCVV?.Length != 3) invalidFields.Add("CreditCardCVV");

            if (invalidFields.Count > 0) return RedirectToAction("Checkout", new { invalidFieldsList = invalidFields, choosenCurrency = CurrentCurrency });

            order.OrderDate = DateTime.Now;
            order.Status = _context.OrderStatuses.Single(x => x.Name == "New");
            order.OrderItems = Cart;
            order.Customer.Email = order.Customer.Email.ToLower();

            var existingCustomer = _context.Customers.SingleOrDefault(x => x.Email == order.Customer.Email);

            if (existingCustomer != null)
            {
                order.Customer = existingCustomer;
            }
            else
            {
                _context.Customers.Add(order.Customer);
            }
            try
            {
                _context.Orders.Add(order);
                _context.SaveChanges();

                var keys = new List<string>(HttpContext.Session.Keys);

                foreach (var key in keys)
                {
                    if (int.TryParse(key, out int keyNum))
                    {
                        HttpContext.Session.Remove(key);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return View("OrderComplete");
        }

        public double GetCartSum()
        {
            return Cart.Sum(x => x.Product.Price * x.Quantity);
        }

        public double ConvertToCurrentCurrency(double value)
        {
            return value * CurrencyExchangeRate;
        }

        public async Task<double> GetCurrencyExchangeRate(Currency wantedCurrency)
        {
            var client = new HttpClient();
            var response = await client.GetAsync($"{BaseCurrencyApiURL}{wantedCurrency}");
            response.EnsureSuccessStatusCode();

            var returnVal = JObject.Parse(await response.Content.ReadAsStringAsync());

            return (double)returnVal["rates"][wantedCurrency.ToString()];
        }

        public async Task UpdateCurrency(Currency newCurrency)
        {
            CurrentCurrency = newCurrency;
            CurrencyExchangeRate = await GetCurrencyExchangeRate(newCurrency);
        }

        public string GetInputClass(string inputName)
        {
            var invalidFieldsList = (List<string>)ViewBag.invalidFieldsList;

            if (invalidFieldsList.Count == 0) return "form-control";

            return invalidFieldsList.Contains(inputName) ? "form-control is-invalid" : "form-control is-valid";
        }

        public List<OrderItem> GetCartFormSession()
        {
            var cartIDs = HttpContext.Session.Keys.Where(id => int.TryParse(id, out var num)).Select(int.Parse);
            var products = _context.Products.Where(product => cartIDs.Contains(product.Id)).Include("Category");
            var orderItems = new List<OrderItem>();
            foreach (var product in products)
            {
                foreach(var md in JsonConvert.DeserializeObject<List<ProductMetaData>>(HttpContext.Session.GetString(product.Id.ToString())))
                {
                    orderItems.Add(new OrderItem() { Product = product, Quantity = md.Quantity, Size = md.Size });
                }
            }
            return orderItems;
        }
    }
}
