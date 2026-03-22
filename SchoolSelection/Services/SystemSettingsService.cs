using SchoolSelection.Data;

namespace SchoolSelection.Services;

public class SystemSettingsService
{
    private readonly CollegeDbContext _context;

    public SystemSettingsService(CollegeDbContext context)
    {
        _context = context;
    }

    public bool IsSubscriptionEnabled()
    {
        var setting = _context.SystemSettings.FirstOrDefault(s => s.Key == "EnableSubscriptions");
        return setting != null && bool.TryParse(setting.Value, out var isEnabled) && isEnabled;
    }

    public async Task UpdateSubscriptionToggle(bool isEnabled)
    {
        var setting = _context.SystemSettings.FirstOrDefault(s => s.Key == "EnableSubscriptions");
        if (setting != null)
        {
            setting.Value = isEnabled.ToString();
            setting.UpdatedAt = DateTime.Now;
            _context.SystemSettings.Update(setting);
            await _context.SaveChangesAsync();
        }
    }
}