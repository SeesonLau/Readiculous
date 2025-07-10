using System.ComponentModel.DataAnnotations;

namespace Readiculous.Services.ServiceModels
{
    public class OtpVerificationModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "OTP is required.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 digits.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must contain only 6 digits.")]
        public string Otp { get; set; }
    }
} 