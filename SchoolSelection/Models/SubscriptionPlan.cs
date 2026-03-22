using System.ComponentModel.DataAnnotations;
using SchoolSelection.CommonEntities;

namespace SchoolSelection.Models;

public class SubscriptionPlan:EntityHelper
{
    public string SubscriptionPlanName { get; set; }
    [Required]
    public string Type { get; set; } // e.g., "Monthly", "Yearly"
    [Required]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; } // Subscription price
    
    // Feature Limits Based on Subscription Type
    public int? MaxSelections { get; set; } // Number of College Selection Apps they can create
    public int? MaxColleges { get; set; } // Number of colleges they can enter
    public int? MaxCriteria { get; set; } // Number of criteria they can select
    public int? MaxSubmissions { get; set; } // Number of times they can trigger "View Your College Fit"

    public string Description { get; set; } // Optional
    public DateTime UpdatedAt { get; set; }
}