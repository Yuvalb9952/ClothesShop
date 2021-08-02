using System.ComponentModel.DataAnnotations;

namespace ClothesShop.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Gender { get; set; }

        public int Age { get; set; }

        public string Email { get; set; }
    }
}