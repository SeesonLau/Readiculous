using System.ComponentModel.DataAnnotations;

namespace Readiculous.Services.ServiceModels
{
    public class EmailRequestModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }
    }
} 