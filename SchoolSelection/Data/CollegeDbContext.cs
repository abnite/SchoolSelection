using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolSelection.Models;
using Results = Microsoft.AspNetCore.Http.Results;

namespace SchoolSelection.Data;

public class CollegeDbContext:IdentityDbContext<ApplicationUser,ApplicationRole,string>
{
    private readonly IHttpContextAccessor accessor;
    public UserManager<ApplicationUser> userManager;

    public CollegeDbContext(DbContextOptions<CollegeDbContext> o, IHttpContextAccessor accessor):base(o)
    {
        this.accessor = accessor;
    }
    
    public DbSet<CollegeSelection> CollegeSelections { get; set; }
    public DbSet<School> Schools { get; set; }
    public DbSet<Criteria>Criteria { get; set; }
    public DbSet<Evaluation>Evaluations { get; set; }
    public DbSet<SelectionResults> SelectionResults { get; set; }
    public DbSet<UserActivityLog> UserActivityLogs { get; set; }
    public DbSet<ContactInquiry> ContactInquiries { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; } 
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<SystemSetting> SystemSettings { get; set; }
    public DbSet<CollegeFitHistory> CollegeFitHistories { get; set; }
    public DbSet<CollegeCriteriaSummary> CollegeCriteriaSummaries { get; set; }

}