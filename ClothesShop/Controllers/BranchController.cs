using ClothesShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ClothesShop.Data;

namespace ClothesShop.Controllers
{
    public class BranchController : Controller
    {
        private readonly ClothesShopContext _context;
        public BranchController(ClothesShopContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult getBranches()
        {
            List<Branch> branches = _context.Branches.ToList();
            ViewBag.Branches = branches;

            return Json(branches);
        }
    }
}
