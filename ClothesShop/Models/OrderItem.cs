using System.ComponentModel.DataAnnotations;

namespace ClothesShop.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        public Product Product{ get; set; }

        public int Quantity{ get; set; }

        public int OrderId { get; set; }
     }
}
