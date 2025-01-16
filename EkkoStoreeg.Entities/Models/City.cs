using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace EkkoSoreeg.Entities.Models
{
    public class City
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(450, ErrorMessage = "max450")]
        public string Name { get; set; }
        public int RegionId { get; set; }
        [ValidateNever]
        public Region Region { get; set; }
    }
}
