using System.ComponentModel.DataAnnotations;

namespace ClothesShop.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        // OrderItem shouldn't have the Order in it (Order should have list of orderitems).
        // For now putting in comment.
        // public Order Order { get; set; }

        public Product Product{ get; set; }

        // OrderItem shouldnt have orderId in it. For now putting in comment.
        // public int OrderId { get; set; }

        public int Quantity{ get; set; }
     }
}
