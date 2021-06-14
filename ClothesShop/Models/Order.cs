using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace ClothesShop.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public Customer Customer { get; set; }

        public OrderStatus Status { get; set; }

        public string Address { get; set; }

        public string Zip { get; set; }

        public string CreditCardName { get; set; }

        public string CreditCardNumber { get; set; }

        public string CreditCardExpiration { get; set; }

        public string CreditCardCVV{ get; set; }

        public DateTime OrderDate { get; set; }

        public List<OrderItem> OrderItems { get; set; }

    }
}
