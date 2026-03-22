using SchoolSelection.Data;

namespace SchoolSelection.Models;

public class UserActivityLog
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public string ActivityType { get; set; } // Login or Logout
    public DateTime Timestamp { get; set; }

    public ApplicationUser User { get; set; }
}