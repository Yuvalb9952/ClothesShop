using System.ComponentModel.DataAnnotations;

namespace ClothesShop.Models
{
    public class PrevCategory
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }
    }
}
