using System.ComponentModel.DataAnnotations;

namespace ClothesShop.Models
{
    public enum Currency
    {
        [Display(Name = "$")]
        USD = '$',
        [Display(Name = "€")]
        EUR = '€',
        [Display(Name = "₪")]
        ILS = '₪'
    }
}