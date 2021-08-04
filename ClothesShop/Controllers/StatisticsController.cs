using ClothesShop.Data;
using ClothesShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace ClothesShop.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly ClothesShopContext _context;

        public StatisticsController(ClothesShopContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<List<GraphData>> ProductOrdersCount()
        {
            var productOrdersCountList = _context.OrderItems.Include(orderItem => orderItem.Product).ToList()
                                                            .GroupBy(i => i.Product)
                                                            .Select(o => new GraphData() { Name = o.Key.Name, Value = o.Sum(x => x.Quantity) })
                                                            .OrderBy(data => data.Value).ToList();
            return Ok(JsonConvert.SerializeObject(productOrdersCountList));
        }

       [HttpGet]
        public ActionResult<List<GraphData>> CategoriesOrdersCount()
        {
            var categoryOrdersCountList = _context.OrderItems
                .Join(_context.Products,
                order => order.Product.Id,
                product => product.Id,
                (order, product) => new
                {
                    product = product,
                    order = order
                }).Join(_context.Categories,
                order => order.product.Category.Id,
                category => category.Id,
                (order, category) => new
                {
                    order = order.order,
                    product = order.product,
                    category = category
                }).ToList()
                .GroupBy(x => x.category)
                .Select(o => new GraphData
                {
                    Name = o.Key.Name,
                    Value = o.Sum(order => order.order.Quantity)
                })
                .ToList();

            return Ok(JsonConvert.SerializeObject(categoryOrdersCountList));
        }

        [HttpGet]
        public ActionResult<List<GraphData>> ProfitPerMonth()
        {
            var profitPerMonthList = _context.OrderItems
                .Join(_context.Products,
                order => order.Product.Id,
                product => product.Id,
                (orderItem, product) => new
                {
                    product = product,
                    orderItem = orderItem
                }).Join(_context.Orders,
                orderDetails => orderDetails.orderItem.OrderId,
                order => order.Id,
                (orderDetails, order) => new
                {
                    orderItem = orderDetails.orderItem,
                    product = orderDetails.product,
                    order = order
                }).ToList()
                .OrderBy(x => x.order.OrderDate)
                .GroupBy(x => x.order.OrderDate.ToString("MM-yyyy"))
                .Select(o => new GraphData
                {
                    Name = o.Key.ToString(),
                    Value = o.Sum(order => order.orderItem.Quantity * order.product.Price)
                })
                .ToList();

            return Ok(JsonConvert.SerializeObject(profitPerMonthList));
        }

        [HttpGet]
        public ActionResult<List<GraphData>> OrderStatusCount()
        {
            var ordersStatusCount = _context.Orders
                .Join(_context.OrderStatuses,
                order => order.Status.Id,
                status => status.Id,
                (order, status) => new {
                    order = order,
                    status = status
                }).ToList()
                .GroupBy(x => x.status)
                .Select(o => new GraphData
                {
                    Name = o.Key.Name,
                    Value = o.Count()
                })
                .ToList();

            return Ok(JsonConvert.SerializeObject(ordersStatusCount));
        }
    }
}
