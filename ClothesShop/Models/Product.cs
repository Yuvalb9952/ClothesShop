using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace ClothesShop.Models
{
    public class Product : IComparable<Product>
    {
        [Key]
        public int Id { get; set; }
        public Category Category { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public Gender Gender { get; set; }
        public string ImageSrc { get; set; }
        public bool IsDeleted { get; set; }
        public List<Tag> Tags { get; set; }

        public int CompareTo(Product other)
        {
            return this.Id.CompareTo(other.Id);
        }
    }
}
