using System;
using System.ComponentModel.DataAnnotations;


namespace ClothesShop.Models
{
    public class Product : IComparable<Product>
    {
        [Key]
        public int Id { get; set; }
        public PrevCategory Category { get; set; } // TODO: Remove "Prev"
        public string Name { get; set; }
        public int Price { get; set; }
        public string ImageSrc { get; set; }
        public bool IsDeleted { get; set; }

        // TODO: Not sure why it's here, if we're not using it delete
        public int CompareTo(Product other)
        {
            return this.Id.CompareTo(other.Id);
        }
    }
}
