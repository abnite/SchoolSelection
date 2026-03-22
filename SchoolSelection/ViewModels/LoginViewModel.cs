using System.ComponentModel.DataAnnotations;

namespace SchoolSelection.ViewModel;

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name ="User Name")]
    public string? Email { get; set; }

    [Required(ErrorMessage ="Enter Password"),DataType(DataType.Password)]
    public string? Password { get; set; }
    [Display(Name ="Remember me")]
    public bool RememberMe { get; set; }
}