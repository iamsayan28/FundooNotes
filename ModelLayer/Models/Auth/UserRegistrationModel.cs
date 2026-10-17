using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLayer.Models.Auth
{
    public class UserRegistrationModel
    {
        [Required(ErrorMessage = ("First Name is required"))]
        [RegularExpression("^[A-Z][a-zA-Z]{2,}$")]
        public string FirstName { get; set; } = string.Empty;
        [Required(ErrorMessage = ("Last Name is required"))]
        [RegularExpression("^[A-Z][a-zA-Z]{2,}$")]
        public string LastName { get; set; } = string.Empty;
        [EmailAddress]
        [StringLength(255)]
        [Required(ErrorMessage = ("Email is required"))]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = ("Phone Number is required"))]
        [RegularExpression("^[0-9]{10}$")]
        public string ContactNo { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
