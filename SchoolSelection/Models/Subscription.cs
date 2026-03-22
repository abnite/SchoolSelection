using SchoolSelection.CommonEntities;

namespace SchoolSelection.Models;

public class Subscription:EntityHelper
{
    //public string UserId { get; set; }
    
    public Guid SubscriptionPlanId { get; set; }
    public SubscriptionPlan SubscriptionPlan { get; set; } 
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive {get; set; }
    
    // Payment Fields
    public string PaymentMethod { get; set; } // e.g., PayPal, Skrill
    public string PaymentStatus { get; set; } // Pending, Completed, Failed
    public string TransactionId { get; set; } // Payment Gateway Transaction ID
    
    // Usage Tracking (NEW)
    public int UsedSelections { get; set; } // How many college selections user has made
    public int UsedColleges { get; set; } // How many colleges user has added
    public int UsedCriteria { get; set; } // How many criteria user has added
    public int UsedResults { get; set; } // How many times user has generated a result
    
    //Free Usage Tracking 
    public int FreeUsedSelections { get; set; } // How many college selections user has made
    public int FreeUsedColleges { get; set; } // How many colleges user has added
    public int FreeUsedCriteria { get; set; } // How many criteria user has added
    public int FreeUsedResults { get; set; } // How many times user has generated a result
}