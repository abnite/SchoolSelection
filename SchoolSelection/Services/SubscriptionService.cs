using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolSelection.Data;
using SchoolSelection.Models;

namespace SchoolSelection.Services;

public class SubscriptionService
{
    private readonly CollegeDbContext _context;
    private readonly SystemSettingsService _settingsService;
    private readonly UserManager<ApplicationUser> _userManager;

    public SubscriptionService(UserManager<ApplicationUser> userManager,SystemSettingsService settingsService,CollegeDbContext context)
    {
        _context = context;
        _settingsService = settingsService;
        _userManager = userManager;
    }
    
    public async Task<bool> IsAdmin(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        return await _userManager.IsInRoleAsync(user, "Admin");
    }
    
    // Get the user's active subscription OR apply Free limits automatically
    public async Task<SubscriptionPlan> GetUserSubscriptionPlan(string userId)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.AddedBy.Id == userId && s.EndDate > DateTime.Now)
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefaultAsync();

        if (subscription != null)
        {
            return subscription.SubscriptionPlan; // Use the active paid plan
        }

        // If no active subscription → Apply Free plan limits automatically
        return await _context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Type == "Free");
    }
    
    
    public async Task<Subscription> GetActiveSubscription(string userId)
    {
        return await _context.Subscriptions.Include(s => s.SubscriptionPlan)
            .Where(s => s.AddedBy.Id == userId && s.EndDate > DateTime.UtcNow && s.IsActive)
            .OrderByDescending(s => s.StartDate).FirstOrDefaultAsync();
    }

    public async Task<bool> HasFullAccess(string userId)
    {
        if (await IsAdmin(userId)) return true; // Admins always have full access.

        var isSubscriptionEnabled = _settingsService.IsSubscriptionEnabled();
        if (!isSubscriptionEnabled) return true; // Subscription system is disabled.

      //  var subscription = _context.Subscriptions.FirstOrDefault(s => s.Id.ToString() == userId && s.EndDate > DateTime.Now);

        //return subscription != null;
      // return await GetUserSubscriptionPlan(userId)!=null;
      return false;
    }

    public async Task<bool> CanAddSchool(string userId)
    {
        if (await HasFullAccess(userId)) return true;
        
        var plan = await GetUserSubscriptionPlan(userId);

        var schoolCount = 0;
        if (plan.Type == "Free")
        { 
            schoolCount = _context.Schools.Count(s => s.AddedBy.Id == userId && s.IsDeleted==false);
        }
        else
        {
          var getUserSubscription = await GetActiveSubscription(userId);
          schoolCount = getUserSubscription.UsedColleges;
        }

        
        return schoolCount < plan.MaxColleges;
    }

    public async Task<bool> CanAddCriteria(string userId)
    {
        if (await HasFullAccess(userId)) return true;
        
        var plan = await GetUserSubscriptionPlan(userId);

        var criteriaCount = 0;
        if (plan.Type == "Free")
        { 
            criteriaCount = _context.Criteria.Count(s => s.AddedBy.Id == userId && s.IsDeleted==false);
        }
        else
        {
            var getUserSubscription = await GetActiveSubscription(userId);
            criteriaCount = getUserSubscription.UsedCriteria;
        }
       // return criteriaCount < 2;
       return criteriaCount < plan.MaxCriteria;
    }

    public async Task<bool>  CanGenerateResult(string userId)
    {
        if (await HasFullAccess(userId)) return true;
        
        var plan = await GetUserSubscriptionPlan(userId);
        var resultsCount  = 0;
        if (plan.Type == "Free")
        { 
             resultsCount = _context.SelectionResults.Count(r => r.AddedBy.Id == userId);
        }
        else
        {
            var getUserSubscription = await GetActiveSubscription(userId);
            resultsCount = getUserSubscription.UsedResults;
        }
      
        // return resultsCount == 0;
        return resultsCount < plan.MaxSubmissions;
    }
    
    public async Task<bool>  CanCreateSelection(string userId)
    {
        if (await HasFullAccess(userId)) return true;
        
        var plan = await GetUserSubscriptionPlan(userId);
        var resultsCount  = 0;
        if (plan.Type == "Free")
        { 
            resultsCount = _context.CollegeSelections.Count(r => r.AddedBy.Id == userId);
        }
        else
        {
            var getUserSubscription = await GetActiveSubscription(userId);
            resultsCount = getUserSubscription.UsedSelections;
        }

      
      
        // return resultsCount == 0;
        return resultsCount < plan.MaxSelections;
    }
    
    // Track usage (Paid subscriptions only)
    public async Task IncrementUsage(string userId, string feature)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.AddedBy.Id == userId && s.EndDate > DateTime.Now)
            .FirstOrDefaultAsync();

        if (subscription != null)
        {
            switch (feature)
            {
                case "selection":
                    subscription.UsedSelections++;
                    break;
                case "college":
                    subscription.UsedColleges++;
                    break;
                case "criteria":
                    subscription.UsedCriteria++;
                    break;
                case "result":
                    subscription.UsedResults++;
                    break;
            }
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task DecrementUsage(string userId, string feature)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.AddedBy.Id == userId && s.EndDate > DateTime.Now)
            .FirstOrDefaultAsync();

        if (subscription != null)
        {
            switch (feature)
            {
                case "selection":
                    subscription.UsedSelections--;
                    break;
                case "college":
                    subscription.UsedColleges--;
                    break;
                case "criteria":
                    subscription.UsedCriteria--;
                    break;
                case "result":
                    subscription.UsedResults--;
                    break;
            }
            await _context.SaveChangesAsync();
        }
    }

    // Reset only Paid Subscription usage when renewed
    public async Task ResetUsageOnSubscription(string userId)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.AddedBy.Id == userId && s.EndDate > DateTime.Now)
            .FirstOrDefaultAsync();

        if (subscription != null)
        {
            subscription.UsedSelections = 0;
            subscription.UsedColleges = 0;
            subscription.UsedCriteria = 0;
            subscription.UsedResults = 0;
            await _context.SaveChangesAsync();
        }
    }
}