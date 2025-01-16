using System.ComponentModel.DataAnnotations;

namespace EkkoSoreeg.Entities.Models
{
    public class Region
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(450)]
        public string Name { get; set; }
        [Required]
        public decimal ShippingCost { get; set; }
    }
}
