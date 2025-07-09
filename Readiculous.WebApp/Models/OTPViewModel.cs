using System.ComponentModel.DataAnnotations;

namespace Readiculous.WebApp.Models
{
    public class OTPViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Verification Code")]
        public string OTP { get; set; }
    }
}
