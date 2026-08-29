//TODO:Ver que es foolproof
//using Foolproof;

using System;
using System.ComponentModel.DataAnnotations;

namespace Ray.FrontendApi.Models
{
    // Models used as parameters to AccountController actions.

    public class ChangePasswordBindingModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class RegisterBindingModel
    {
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [StringLength(100, ErrorMessage = "{0} must be a string with a maximum length of {1} characters.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [StringLength(100, ErrorMessage = "{0} must be a string with a maximum length of {1} characters.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [StringLength(100, ErrorMessage = "{0} must be a string with a maximum length of {1} characters.")]
        [Display(Name = "Country")]
        public string Country { get; set; }

        [StringLength(100, ErrorMessage = "{0} must be a string with a maximum length of {1} characters.")]
        [Display(Name = "State")]
        public string State { get; set; }

        [StringLength(100, ErrorMessage = "{0} must be a string with a maximum length of {1} characters.")]
        [Display(Name = "City")]
        public string City { get; set; }

        [StringLength(100, ErrorMessage = "{0} must be a string with a maximum length of {1} characters.")]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [StringLength(100, ErrorMessage = "{0} must be a string with a maximum length of {1} characters.")]
        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; }

        [StringLength(100, ErrorMessage = "{0} must be a string with a maximum length of {1} characters.")]
        [Display(Name = "Cellphone")]
        public string Cellphone { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Language")]
        public int LanguageId { get; set; }

        [Required]
        [Display(Name = "Time Zone")]
        public int TimeZoneId { get; set; }

        [Display(Name = "Facebook Id")]
        public string FacebookId { get; set; }

        [Display(Name = "Twitter Id")]
        public string TwitterId { get; set; }

        [StringLength(50, ErrorMessage = "{0} must be a string with a maximum length of {1} characters.")]
        public string IdentificationNumber { get; set; }

        [Range(0, 4, ErrorMessage = "Gender")]
        public byte? Gender { get; set; }

        public DateTime? BirthDate { get; set; }

        [StringLength(300, ErrorMessage = "{0} must be a string with a maximum length of {1} characters.")]
        public string Neighborhood { get; set; }

        public string Reference { get; set; }

        [StringLength(50, ErrorMessage = "{0} must be a string with a maximum length of {1} characters.")]
        public string IMEI { get; set; }

        [StringLength(300, ErrorMessage = "{0} must be a string with a maximum length of {1} characters.")]
        public string ProfileImagePath { get; set; }
    }

    public class ForgotPasswordBindingModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ResetPasswordBindingModel : ForgotPasswordBindingModel
    {
        [Required]
        [Display(Name = "Token")]
        public string Token { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class UpdatePasswordBindingModel
    {
        [Required]
        [Display(Name = "Current password")]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Token")]
        public string Token { get; set; }
    }

    public class RemoveLoginBindingModel
    {
        [Required]
        [Display(Name = "Login provider")]
        public string LoginProvider { get; set; }

        [Required]
        [Display(Name = "Provider key")]
        public string ProviderKey { get; set; }
    }

    public class SetPasswordBindingModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ExternalLogInBindingModel
    {
        [Display(Name = "Email")]
        public string Email { get; set; }

        //[RequiredIfEmpty("TwitterId")]
        [Display(Name = "Facebook Id")]
        public string FacebookId { get; set; }

        //[RequiredIfEmpty("FacebookId")]
        [Display(Name = "Twitter Id")]
        public string TwitterId { get; set; }
    }

    public class UserConfirmationBindingModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Token")]
        public string Token { get; set; }
    }
}
