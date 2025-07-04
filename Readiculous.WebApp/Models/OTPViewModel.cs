using System.ComponentModel.DataAnnotations;

namespace Readiculous.WebApp.Models
{
    public class OTPViewModel
    {
        [Required(ErrorMessage = "OTP Code is required.")]
        [StringLength(6, ErrorMessage = "OTP must be 6 digits.")]
        public string Code { get; set; }
    }
}
