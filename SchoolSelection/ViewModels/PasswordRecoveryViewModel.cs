using System.ComponentModel.DataAnnotations;

namespace SchoolSelection.ViewModel;

public class PasswordRecoveryViewModel
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
}