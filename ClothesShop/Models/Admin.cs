using System.ComponentModel.DataAnnotations;

namespace ClothesShop.Models
{
    public class Admin
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        public string UserName { get; set; }
        public Gender Gender { get; set; }
        [Required]
        public string Email { get; set; }
    }
}
