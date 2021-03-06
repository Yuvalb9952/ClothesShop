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
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }

        public void InitializeProducts()
        {
            var tlvBranch = new Branch
            {
                BranchName = "ClothesShop - TLV",
                AddressInfo = "Tel Aviv, TLV Mall",
                LocationX = (float)32.069500,
                LocationY = (float)34.786509
            };

            var jerusalemBranch = new Branch
            {
                BranchName = "ClothesShop - HQ",
                AddressInfo = "Jerusalem, Hommos 1 St",
                LocationX = (float)31.777820,
                LocationY = (float)35.209204
            };

            var ashdodBranch = new Branch
            {
                BranchName = "ClothesShop - HQ",
                AddressInfo = "Ashdod, Big fashion",
                LocationX = (float)31.776842,
                LocationY = (float)34.663914
            };

            var HerzliyaBranch = new Branch
            {
                BranchName = "ClothesShop - HQ",
                AddressInfo = "Herzliya, Arena mall",
                LocationX = (float)32.163753,
                LocationY = (float)34.796861
            };

            Branches.AddRange(tlvBranch, jerusalemBranch, HerzliyaBranch, ashdodBranch);

            var admin1 = new Admin
            {
                Email = "bla@gmail.com",
                UserName = "bla",
                Gender = Gender.Female,
                Password = "Aa123456"
            };

            var admin2 = new Admin
            {
                Email = "bla2@gmail.com",
                UserName = "bla2",
                Gender = Gender.Male,
                Password = "Aa123456"
            };

            Admins.AddRange(admin1, admin2);

            var tShirts = new Category
            {
                Name = "T-Shirts"
            };

            var pants = new Category
            {
                Name = "Pants"
            };

            var dresses = new Category
            {
                Name = "Dresses"
            };

            var shoes = new Category
            {
                Name = "Shoes"
            };

            var jackets = new Category
            {
                Name = "Jackets"
            };

            Categories.AddRange(tShirts, pants, dresses, shoes, jackets);

            var elegant = new Tag
            {
                Name = "Elegant"
            };

            var casual = new Tag
            {
                Name = "Casual"
            };

            var sport = new Tag
            {
                Name = "Sport"
            };

            var party = new Tag
            {
                Name = "Party"
            };

            var summer = new Tag
            {
                Name = "Summer"
            };

            var spring = new Tag
            {
                Name = "Spring"
            };

            var colorful = new Tag
            {
                Name = "Colorful"
            };

            Tags.AddRange(elegant, casual, sport, party, summer, spring, colorful);

            var printedCrop = new Product
            {
                Name = "Printed Crop",
                Category = tShirts,
                IsDeleted = false,
                Price = 50,
                Gender = Gender.Female,
                ImageSrc = "https://s7d2.scene7.com/is/image/aeo/0352_3818_900_l1?$pdp-md-opt$&fmt=webp",
                Tags = new List<Tag>() { party, colorful }
            };

            var eagleTshirt = new Product
            {
                Name = "Eagle T-shirt",
                Category = tShirts,
                IsDeleted = false,
                Price = 80,
                Gender = Gender.Male,
                ImageSrc = "https://s7d2.scene7.com/is/image/aeo/0181_5372_109_f?$pdp-md-opt$&fmt=webp",
                Tags = new List<Tag>() { casual, summer }
            };

            var babydollDress = new Product
            {
                Name = "Babydoll dress",
                Category = dresses,
                IsDeleted = false,
                Price = 100,
                Gender = Gender.Female,
                ImageSrc = "https://s7d2.scene7.com/is/image/aeo/0395_5742_704_l1?$pdp-md-opt$&fmt=webp",
                Tags = new List<Tag>() { elegant, spring }
            };

            var pocketPants = new Product
            {
                Name = "Pocket Pants",
                Category = pants,
                IsDeleted = false,
                Price = 120,
                Gender = Gender.Male,
                ImageSrc = "https://s7d2.scene7.com/is/image/aeo/0121_4549_204_f?$pdp-mdg-opt$&fmt=webp",
                Tags = new List<Tag>() { party, casual }
            };

            var flipflops = new Product
            {
                Name = "Flip-Flop",
                Category = shoes,
                IsDeleted = false,
                Price = 70,
                Gender = Gender.Female,
                ImageSrc = "https://s7d2.scene7.com/is/image/aeo/0417_5167_109_f?$pdp-md-opt$&fmt=webp",
                Tags = new List<Tag>() { summer }
            };

            var denimJacket = new Product
            {
                Name = "Denim Jacket",
                Category = jackets,
                IsDeleted = false,
                Price = 100,
                Gender = Gender.Male,
                ImageSrc = "https://s7d2.scene7.com/is/image/aeo/0106_1391_400_f?$pdp-md-opt$&fmt=webp",
                Tags = new List<Tag>() { party }
            };

            Products.AddRange(
                printedCrop,
                eagleTshirt,
                babydollDress,
                pocketPants,
                flipflops,
                denimJacket
            );

            var statusNew = new OrderStatus
            {
                Name = "New"
            };

            var statusSent = new OrderStatus
            {
                Name = "Delivered"
            };

            var statusAccepted = new OrderStatus
            {
                Name = "Accepted"
            };

            var Adi = new Customer
            {
                Email = "adi@gmail.com",
                FirstName = "Adi",
                LastName = "Papismadov"
            };

            var Eden = new Customer
            {
                Email = "eden@gmail.com",
                FirstName = "Eden",
                LastName = "Avretz"
            };

            var Tal = new Customer
            {
                Email = "tal@gmail.com",
                FirstName = "Tal",
                LastName = "Shemesh"
            };

            var Yuval = new Customer
            {
                Email = "yuval@gmail.com",
                FirstName = "Yuval",
                LastName = "Ben Amram"
            };

            OrderStatuses.AddRange(statusNew, statusAccepted, statusSent);
            Customers.AddRange(Adi, Eden, Tal, Yuval);

            Order talOrder = new Order
            {
                Address = "nav 1",
                CreditCardCVV = "143",
                CreditCardExpiration = "10/20",
                CreditCardName = "Tal Shemesh",
                CreditCardNumber = "2345234523452345",
                Customer = Tal,
                OrderDate = new System.DateTime(2021, 7, 1),
                Status = statusSent,
                Zip = "1232344",
                OrderItems = new System.Collections.Generic.List<OrderItem>()
            };

            OrderItem talItem1 = new OrderItem
            {
                Product = printedCrop,
                Quantity = 2,
                Size = Size.Small
            };

            OrderItem talItem2 = new OrderItem
            {
                Product = flipflops,
                Quantity = 1,
                Size = Size.Medium
            };

            talOrder.OrderItems.Add(talItem1);
            talOrder.OrderItems.Add(talItem2);

            Order yuvalOrder = new Order
            {
                Address = "gav 1",
                CreditCardCVV = "144",
                CreditCardExpiration = "11/21",
                CreditCardName = "Yuval Ben Amram",
                CreditCardNumber = "6789678967896789",
                Customer = Yuval,
                OrderDate = new System.DateTime(2021, 5, 10),
                Status = statusAccepted,
                Zip = "1232345",
                OrderItems = new System.Collections.Generic.List<OrderItem>()
            };

            OrderItem yuvalItem1 = new OrderItem
            {
                Product = denimJacket,
                Quantity = 1,
                Size = Size.Large
            };

            yuvalOrder.OrderItems.Add(yuvalItem1);

            Orders.AddRange(talOrder, yuvalOrder);

            SaveChanges();
        }
    }
}
