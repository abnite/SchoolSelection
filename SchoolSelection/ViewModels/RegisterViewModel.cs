using System.ComponentModel.DataAnnotations;

namespace SchoolSelection.ViewModel;

public class RegisterViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string? Email { get; set; }
    [EmailAddress]
    [Display(Name = "Confirm Email")]
    [Compare("Email", ErrorMessage = "The Email Address and the Confirm Email do not match.")]
    public string? ConfirmEmail { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string? ConfirmPassword { get; set; }
    
    [Required]
    public string? FirstName { get;set; }
    [Required]
    public string? LastName { get;set; }
    [Required]
    public string? Country { get; set; }
    public string? ReferralCode { get; set; }
   // [Required]
   //[Required]
    [RegularExpression(@"^\+\d{1,4}(\s?\d{6,14})?$", ErrorMessage = "Invalid phone number format. Please enter a valid phone number with country code.")]
    //[RegularExpression(@"^\d{10}$", ErrorMessage = "Invalid phone number format. Please enter a 10-digit number.")]
    public string? PhoneNo { get; set; }
}