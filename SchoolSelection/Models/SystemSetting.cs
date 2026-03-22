using SchoolSelection.CommonEntities;

namespace SchoolSelection.Models;

public class SystemSetting
{
    public Guid Id { get; set; }
    public string Key { get; set; } // e.g., "EnableSubscriptions"
    public string Value { get; set; } // e.g., "true" or "false"
    public DateTime UpdatedAt { get; set; }
}