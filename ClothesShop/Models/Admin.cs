using System.ComponentModel.DataAnnotations;

namespace ClothesShop.Models
{
    public class Admin
    {
        [Key]
        public int Id { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public Gender Gender { get; set; }
        public string Email { get; set; }
    }
}
