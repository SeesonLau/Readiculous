// Models/UserFormModel.cs
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.WebApp.Models
{
    public class UserFormModel
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
