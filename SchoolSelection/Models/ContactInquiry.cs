using System.ComponentModel.DataAnnotations;

namespace SchoolSelection.Models;

public class ContactInquiry
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; }

    [Required]
    [MaxLength(200)]
    public string Subject { get; set; }

    [Required]
    public string Message { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Unread";
}