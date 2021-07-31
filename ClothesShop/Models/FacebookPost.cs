using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;


namespace ClothesShop.Models
{
    public class FacebookPost
    {
        [Key]
        public string message { get; set; }
        public string link { get; set; }
        public IFormFile image { get; set; }

    }
}
