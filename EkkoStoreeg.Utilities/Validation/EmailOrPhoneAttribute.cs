using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EkkoSoreeg.Utilities.Validation
{
    public class EmailOrPhoneAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                var input = value.ToString();
                var phoneRegex = new Regex(@"^(?:\+20|0)?1[0125]\d{8}$");
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                if (phoneRegex.IsMatch(input))
                {
                    return ValidationResult.Success;
                }
                if (emailRegex.IsMatch(input))
                {
                    return ValidationResult.Success;
                }
                return new ValidationResult("The input must be a valid Egyptian phone number or email address.");
            }
            return new ValidationResult("This field is required.");
        }
    }
}
