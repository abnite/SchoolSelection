using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PaypalServerSdk.Standard.Models;
using SchoolSelection.ApplicationClass;
using SchoolSelection.Data;
using SchoolSelection.Models;
using SchoolSelection.Services;

namespace SchoolSelection.Controllers;

[Authorize]
public class SubscriptionPlanController : Controller
{
    private readonly CollegeDbContext _context;
    private readonly DataHelper _myHelper;
    private readonly PayPalService _payPalService;
    private readonly PayPalEnvironmentConfig _currentConfig;
    private readonly SubscriptionService _subscriptionService;

    public SubscriptionPlanController(PayPalService payPalService,DataHelper myHelper,CollegeDbContext context, IOptions<PayPalSettings> payPalSettings, SubscriptionService subscriptionService)
    {
        _context = context;
        _myHelper = myHelper;
        _payPalService = payPalService;
        var settings = payPalSettings.Value;
        _subscriptionService = subscriptionService;
        _currentConfig = settings.GetCurrentEnvironmentConfig();
    }
    // GET
    public async Task<IActionResult> Index()
    {
        var plans = await _context.SubscriptionPlans.ToListAsync();
        return View(plans);
        return View();
    }
    
    // Add or Edit a subscription plan
    public async Task<IActionResult> AddOrEdit(Guid? id)
    {
        if (id == null) return View(new SubscriptionPlan());
        
        var plan = await _context.SubscriptionPlans.FindAsync(id);
        if (plan == null) return NotFound();
        
        return View(plan);
    }

    [HttpPost]
    public async Task<IActionResult> AddOrEdit(SubscriptionPlan plan)
    {
        if (ModelState.IsValid)
        {
            plan.UpdatedAt = DateTime.Now;

            if (plan.Id == Guid.Empty)
            {
                plan.Id = Guid.NewGuid();
                _context.SubscriptionPlans.Add(plan);
            }
            else
            {
                _context.SubscriptionPlans.Update(plan);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Subscription plan saved successfully!";
            return RedirectToAction("Index");
        }

        return View(plan);
    }

    // Delete a subscription plan
    public async Task<IActionResult> Delete(Guid id)
    {
        var plan = await _context.SubscriptionPlans.FindAsync(id);
        if (plan == null) return NotFound();

        _context.SubscriptionPlans.Remove(plan);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Subscription plan deleted successfully!";
        return RedirectToAction("Index");
    }
    
    public async Task<IActionResult> SubscriptionOptions()
    {
        var plans = await _context.SubscriptionPlans/*.Where(i=>i.Type!="Free")*/.ToListAsync();
       
        return View(plans);
    }
    
     // Subscribe to a Plan
    [HttpPost]
    public async Task<IActionResult> Subscribe(Guid planId, string paymentMethod)
    {
        var plan = await _context.SubscriptionPlans.FindAsync(planId);
        if (plan == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

        // Check if the user is already subscribed to the plan
        var existingSubscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.AddedBy.Id == userId && s.SubscriptionPlanId == planId && s.EndDate>=DateTime.Now);
        
        var userPlan = await _subscriptionService.GetUserSubscriptionPlan(userId);
        var getUserSubscription = await _subscriptionService.GetActiveSubscription(userId);

        var resultsCount = 0;
        if (getUserSubscription != null)
        {
            resultsCount = getUserSubscription.UsedSelections;
        }
        else
        {
            resultsCount = 0;
        }
        
        
        var remainingColleges=userPlan.MaxSelections - resultsCount;
        remainingColleges = remainingColleges < 0 ? 0 : remainingColleges;

        
      if (existingSubscription != null)
        {
            if (remainingColleges > 0)
            {
                TempData["ErrorMessage"] = $"You still have {remainingColleges} remaining selections. You cannot subscribe again until your current plan is closer to expiration.";
                return RedirectToAction("SubscriptionOptions");
            }
            TempData["ErrorMessage"] = "You already have an active subscription for this plan.";
            return RedirectToAction("SubscriptionOptions");
        }
       

        // Create a new subscription
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            SubscriptionPlanId = planId,
            StartDate = DateTime.Now,
            EndDate = plan.Type.ToLower() switch
            {
                "monthly" => DateTime.Now.AddMonths(1),
                "yearly" => DateTime.Now.AddYears(1),
                _ => DateTime.Now.AddMonths(1) // Default to monthly if type is unknown
            },
            PaymentMethod = paymentMethod,
            PaymentStatus = "Pending",
            AddedBy = _myHelper.GetLoggedInUser(),
            TransactionId = Guid.NewGuid().ToString(),
        };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();
        
        
        // Redirect to payment processing based on the method
        switch (paymentMethod)
        {
            case "PayPal":
                return RedirectToAction("SubscribeWithPayPal", new { subscriptionId = subscription.Id });
            case "Skrill":
                return RedirectToAction("PayWithSkrill", new { subscriptionId = subscription.Id });
            default:
                TempData["ErrorMessage"] = "Invalid payment method selected.";
                return RedirectToAction("SubscriptionOptions");
        }

       /* TempData["SuccessMessage"] = "You have successfully subscribed to the plan!";
        return RedirectToAction("MySubscriptions");*/
    }
    
   // [HttpPost]
    public async Task<IActionResult> SubscribeWithPayPal(Guid subscriptionId)
    {
        var subscription = await _context.Subscriptions.FindAsync(subscriptionId);
        var planId = await _context.SubscriptionPlans.FindAsync(subscription.SubscriptionPlanId);
        if (subscription == null) return NotFound();

        var returnUrl = Url.Action("PaymentSuccess", "SubscriptionPlan", new { subscriptionId }, Request.Scheme);
        var cancelUrl = Url.Action("PaymentFailed", "SubscriptionPlan", new { subscriptionId }, Request.Scheme);

        var orderId = await _payPalService.CreateOrderAsync(planId.Price, "USD", returnUrl, cancelUrl);

        
        string baseUrl = _currentConfig.ApiUrl.Contains("sandbox") 
            ? "https://www.sandbox.paypal.com/checkoutnow" 
            : "https://www.paypal.com/checkoutnow";
        
       // var approvalUrl = $"https://www.paypal.com/checkoutnow?token={orderId}";
        var approvalUrl = $"{baseUrl}?token={orderId}";
        return Redirect(approvalUrl);
        return RedirectToAction("SubscriptionOptions");
    }

    public async Task<IActionResult> PaymentSuccess(Guid subscriptionId, string token)
    {
        // Retrieve subscription from the database
        var subscription = await _context.Subscriptions.FindAsync(subscriptionId);

        if (subscription == null) return NotFound();

        try
        {
            // Get order details using the token
            var orderDetails = await _payPalService.GetOrderDetailsAsync(token);

            if (orderDetails == null || orderDetails.Status != "COMPLETED")
            {
                TempData["ErrorMessage"] = "Payment verification failed. Please contact support.";
                return RedirectToAction("SubscriptionOptions");
            }

            // Extract the transactionId
            var transactionId = orderDetails.PurchaseUnits
                .FirstOrDefault()?
                .Payments?.Captures.FirstOrDefault()?.Id;

            // Update subscription details in the database
            subscription.PaymentStatus = "Completed";
            subscription.IsActive = true;
            subscription.TransactionId = transactionId; // Save the transactionId
            subscription.StartDate = DateTime.Now;
            subscription.EndDate = DateTime.Now.AddMonths(1); // Adjust as per the plan
            await _context.SaveChangesAsync();

            TempData["Message"] = "Payment was Successful.";
            return RedirectToAction("SubscriptionOptions");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred while verifying the payment: {ex.Message}";
            return RedirectToAction("SubscriptionOptions");
        }
    }

    public IActionResult PaymentFailed(Guid subscriptionId)
    {
        TempData["ErrorMessage"] = "Payment was cancelled or failed. Please try again.";
        return RedirectToAction("SubscriptionOptions");
    }

    // View User Subscriptions
    public async Task<IActionResult> MySubscriptions()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

        var subscriptions = await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.AddedBy.Id == userId)
            .ToListAsync();

        return View(subscriptions);
    }

    // Cancel Subscription
    public async Task<IActionResult> CancelSubscription(Guid id)
    {
        var subscription = await _context.Subscriptions.FindAsync(id);
        if (subscription == null) return NotFound();

        _context.Subscriptions.Remove(subscription);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Your subscription has been canceled successfully!";
        return RedirectToAction("MySubscriptions");
    }
    

}