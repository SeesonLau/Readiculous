using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.Services.ServiceModels
{
    public class UserViewModel /*: IValidatableObject*/
    {

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(20, ErrorMessage = "Your Username must not exceed 20 characters!")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [StringLength(100, ErrorMessage = "Your email must not exceed 100 characters!")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }

        //[Required(ErrorMessage = "Password is required.")]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^a-zA-Z\\d\\s]).{8,16}$",
        ErrorMessage = "Password must be 8-16 characters long and contain at least one lowercase letter, one uppercase letter, one digit, and one special character.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string UserId { get; set; }
        public RoleType? Role { get; set; }
        public IFormFile ProfilePicture { get; set; }
        public string ProfilePictureUrl { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedTime { get; set; }
        [Display(Name = "Remove Profile Picture")]
        public bool RemoveProfilePicture { get; set; }
        public AccessStatus AccessStatus { get; set; }

        /*public bool IsAdminCreation { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!IsAdminCreation)
            {
                if (string.IsNullOrWhiteSpace(Password))
                {
                    yield return new ValidationResult("Password is required.", new[] { nameof(Password) });
                }
                else
                {
                    var regex = new System.Text.RegularExpressions.Regex("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^a-zA-Z\\d\\s]).{8,16}$");
                    if (!regex.IsMatch(Password))
                    {
                        yield return new ValidationResult(
                            "Password must be 8-16 characters long and contain at least one lowercase letter, one uppercase letter, one digit, and one special character.",
                            new[] { nameof(Password) });
                    }
                }
            }
        }*/
    }
}
