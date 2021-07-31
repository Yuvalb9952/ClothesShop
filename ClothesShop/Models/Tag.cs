using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClothesShop.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }
        public List<Product> Products { get; set; }
    }
}
