using ClothesShop.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClothesShop.Data
{
    public class ClothesShopContext : DbContext
    {
        public ClothesShopContext(DbContextOptions<ClothesShopContext> options) : base(options)
        {
        }

        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<PrevBranch> Branches { get; set; } // TODO: Remove "Prev"
        public DbSet<Admin> Admins { get; set; }
        public DbSet<PrevCategory> Categories { get; set; } // TODO: Remove "Prev"
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Costumers { get; set; }

        public void InitializeProducts()
        {
            // TODO: Create objects for DB

            //SaveChanges();
        }
    }
}
