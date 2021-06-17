using System.ComponentModel.DataAnnotations;

namespace ClothesShop.Models
{
    public class PrevBranch
    {
        [Key]
        public int ID { get; set; }
        public string BranchName { get; set; }
        public string AddressInfo { get; set; }
        public float LocationX { get; set; }
        public float LocationY { get; set; }
    }
}
