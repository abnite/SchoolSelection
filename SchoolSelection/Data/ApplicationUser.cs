using Microsoft.AspNetCore.Identity;

namespace SchoolSelection.Data;

public class ApplicationUser:IdentityUser
{
    public string? FirstName { get; set; } 
    public string? LastName { get; set; }
    public string? Country { get; set; }
    public Boolean? IsDeleted { get; set; }
    public Boolean? IsConfirmed{ get; set; }
    public Boolean? FreeUsed { get; set; }
    public Boolean? Introduction { get; set; }
    
    public DateTime? LastLoginTime { get; set; }
    
    public string? ReferralCode { get; set; }
    
    public ApplicationUser()
    {
        IsConfirmed = false;
        FreeUsed = false;
    }

}