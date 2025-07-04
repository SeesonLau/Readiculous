using System.ComponentModel.DataAnnotations;

namespace Readiculous.WebApp.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
