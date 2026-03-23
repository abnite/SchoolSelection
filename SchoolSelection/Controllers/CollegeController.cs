using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolSelection.ApplicationClass;
using SchoolSelection.Data;
using SchoolSelection.Interfaces;
using SchoolSelection.Models;
using SchoolSelection.Services;
using SchoolSelection.ViewModels;

namespace SchoolSelection.Controllers;

[Authorize]
public class CollegeController : Controller
{
    private readonly CollegeDbContext _context;
    private readonly IOpenAIService _openAIService;
    private readonly DataHelper _myHelper;
    private readonly SubscriptionService _subscriptionService;
    private readonly CollegeService _collegeService;

    public CollegeController(SubscriptionService subscriptionService, DataHelper myHelper, IOpenAIService openAIService,
        CollegeDbContext context, CollegeService collegeService)
    {
        _context = context;
        _openAIService = openAIService;
        _myHelper = myHelper;
        _subscriptionService = subscriptionService;
        _collegeService = collegeService;
    }

    private bool IsSessionExpired()
    {
        var collegeSelectionId = HttpContext.Session.GetString("CollegeSelectionId");
        return string.IsNullOrEmpty(collegeSelectionId);
    }

    private IActionResult HandleSessionExpiration()
    {
        return Unauthorized("Session expired. Please log in again.");
    }

    [HttpGet]
    public async Task<IActionResult> GetRecentJourneys()
    {
        try
        {
            var userName = User.Identity.Name;
            var recentJourneys = await _context.CollegeSelections
                .Where(c => c.AddedBy.UserName == userName && c.IsDeleted == false)
                .OrderByDescending(c => c.DateAdded)
                .Take(3)
                .Select(c => new
                {
                    id = c.Id,
                    name = c.CollegeSelectionName,
                    date = c.DateAdded.HasValue ? c.DateAdded.Value.ToString("MMM dd, yyyy") : "No date",
                    status = _context.SelectionResults.Any(r => r.CollegeSelectionId == c.Id)
                        ? "Completed"
                        : "In Progress",
                    link = _context.SelectionResults.Any(r => r.CollegeSelectionId == c.Id)
                        ? Url.Action("GetEvaluationDetails", "College", new { id = c.Id })
                        : Url.Action("ResumeEvaluation", "College", new { collegeSelectionId = c.Id })
                })
                .ToListAsync();

            return Json(recentJourneys);
        }
        catch (Exception ex)
        {
            return Json(new { error = ex.Message });
        }
    }

    // GET
    public async Task<IActionResult> Index()
    {
        var collegeSelectionId = HttpContext.Session.GetString("CollegeSelectionId");
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var plan = await _subscriptionService.GetUserSubscriptionPlan(userId);

        var resultsCount = 0;
        if (plan.Type == "Free")
        {
            resultsCount = _context.CollegeSelections.Count(s => s.AddedBy.Id == userId && s.IsDeleted == false);
        }
        else
        {
            var getUserSubscription = await _subscriptionService.GetActiveSubscription(userId);
            if (getUserSubscription != null)
            {
                resultsCount = getUserSubscription.UsedSelections;
            }
            else
            {
                resultsCount = 0;
            }
        }

        ViewData["RemainingSelections"] = plan.MaxSelections - resultsCount;
        ViewData["PlanType"] = plan.Type;


        ViewData["CollegeSelectionId"] = collegeSelectionId;
        return View();
    }




    public async Task<IActionResult> Index1234()
    {
        // 1. existing plan logic
        var collegeSelectionId = HttpContext.Session.GetString("CollegeSelectionId");
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var plan = await _subscriptionService.GetUserSubscriptionPlan(userId);

        int resultsCount;
        if (plan.Type == "Free")
        {
            resultsCount = _context.CollegeSelections
                .Count(s => s.AddedBy.Id == userId && s.IsDeleted == false);
        }
        else
        {
            var sub = await _subscriptionService.GetActiveSubscription(userId);
            resultsCount = sub?.UsedSelections ?? 0;
        }

        // 2. fetch the most-recent 5 journeys
        var savedSelections = await _context.CollegeSelections
            .Where(s => s.AddedBy.Id == userId && s.IsDeleted == false)
            .OrderByDescending(s => s.DateAdded)
            .Take(5)
            .Select(s => new SavedSelectionViewModel
            {
                Id = s.Id,
                CollegeSelectionName = s.CollegeSelectionName,
                DateAdded = s.DateAdded ?? DateTime.MinValue,
                IsCompleted = _context.SelectionResults
                    .Any(r => r.CollegeSelectionId == s.Id)
            })
            .ToListAsync();

        // 3. build and return ViewModel
        var vm = new HomeViewModel
        {
            PlanType = plan.Type,
            RemainingSelections = Math.Max(0, plan.MaxSelections.Value - resultsCount),
            CollegeSelectionId = collegeSelectionId,
            SavedSelections = savedSelections
        };

        return View(vm);
    }


    public async Task<IActionResult> CollegeSelectionName()
    {
        var collegeSelectionId = HttpContext.Session.GetString("CollegeSelectionId");
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var plan = await _subscriptionService.GetUserSubscriptionPlan(userId);

        var resultsCount = 0;
        if (plan.Type == "Free")
        {
            resultsCount = _context.CollegeSelections.Count(s => s.AddedBy.Id == userId && s.IsDeleted == false);
        }
        else
        {
            var getUserSubscription = await _subscriptionService.GetActiveSubscription(userId);
            if (getUserSubscription != null)
            {
                resultsCount = getUserSubscription.UsedSelections;
            }
            else
            {
                resultsCount = 0;
            }

        }

        ViewData["RemainingSelections"] = plan.MaxSelections - resultsCount;
        ViewData["PlanType"] = plan.Type;


        ViewData["CollegeSelectionId"] = collegeSelectionId;
        return View();
    }

    //submitting the college Selection Name
    [HttpPost]
    public async Task<IActionResult> CollegeSelection(CollegeSelection model)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var collegeSelectionId = HttpContext.Session.GetString("CollegeSelectionId");
            ViewData["CollegeSelectionId"] = collegeSelectionId;

            if (!await _subscriptionService.CanCreateSelection(userId))
            {
                TempData["ErrorMessage"] =
                    "You have reached the limit of College Selections. Please subscribe for more access.";
                return RedirectToAction("Index");
            }

            if (!string.IsNullOrEmpty(collegeSelectionId))
            {
                return View("SchoolSelection");
            }

            if (ModelState.IsValid)
            {
                var collegeSelection = new CollegeSelection()
                {
                    CollegeSelectionName = model.CollegeSelectionName,
                    AddedBy = _myHelper.GetLoggedInUser()
                };
                _context.CollegeSelections.Add(collegeSelection);
                await _context.SaveChangesAsync();

                await _subscriptionService.IncrementUsage(userId, "selection");

                // Set session values
                HttpContext.Session.SetString("CollegeSelectionName", collegeSelection.CollegeSelectionName);
                HttpContext.Session.SetString("CollegeSelectionId", collegeSelection.Id.ToString());

                return RedirectToAction("SchoolSelection", "College");
                return View("SchoolSelection");
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e);

        }

        return View("index");
    }

    public async Task<IActionResult> SchoolSelection()
    {
        // Assuming you're storing the CollegeSelectionId in the session.
        var collegeSelectionId = HttpContext.Session.GetString("CollegeSelectionId");
        if (!string.IsNullOrEmpty(collegeSelectionId))
        {
            ViewData["CollegeSelectionId"] = collegeSelectionId;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var plan = await _subscriptionService.GetUserSubscriptionPlan(userId);
            var resultsCount = await _context.Schools.CountAsync(s => s.AddedBy.Id == userId && !s.IsDeleted);
            ViewData["RemainingColleges"] = plan.MaxColleges - resultsCount;
            ViewData["PlanType"] = plan.Type;


            // Fetch related schools or initialize a new list if none exist.
            var schools = _context.Schools.Where(s => s.CollegeSelectionId == Guid.Parse(collegeSelectionId)).ToList();
            return View();
        }
        else
        {
            return RedirectToAction("Index");
        }

        return View();
    }

    // [HttpPost]
    [HttpPost]
    /* public async Task<IActionResult> AddOrUpdateSchool(School model, Guid? id)
     {
         var collegeSelectionId = HttpContext.Session.GetString("CollegeSelectionId");
         if (!ModelState.IsValid)
         {
             return View("SchoolSelection", model);
         }

         var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


         if (string.IsNullOrEmpty(collegeSelectionId))
         {
             TempData["ErrorMessage"] = "College selection ID is missing.";
             return RedirectToAction("SchoolSelection");
         }

         model.CollegeSelectionId = Guid.Parse(collegeSelectionId);

         if (id.HasValue && id != Guid.Empty)
         {
             // Update existing school
             var existingSchool = await _context.Schools.FindAsync(id.Value);
             if (existingSchool == null)
             {
                 TempData["ErrorMessage"] = "School not found.";
                 return RedirectToAction("SchoolSelection");
             }

             existingSchool.Name = model.Name;
             existingSchool.Info = model.Info;
             _context.Schools.Update(existingSchool);
             await _context.SaveChangesAsync();

             TempData["Message"] = "School updated successfully.";
             return RedirectToAction("SchoolSelection");
         }
         else
         {
             // Add new school
             if (!await _subscriptionService.CanAddSchool(userId))
             {
                 TempData["ErrorMessage"] = "You have reached the limit of Colleges. Please subscribe for more access.";
                 return RedirectToAction("SchoolSelection");
             }

             model.AddedBy = _myHelper.GetLoggedInUser();
             _context.Schools.Add(model);
             await _subscriptionService.IncrementUsage(userId, "college");
             await _context.SaveChangesAsync();

             TempData["Message"] = "School added successfully.";
             return RedirectToAction("SchoolSelection");
         }
     }*/
    public async Task<IActionResult> AddOrUpdateSchool(School model, Guid? id)
    {
        if (id == null)
        {
            id = Guid.Empty;
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var collegeSelectionId = HttpContext.Session.GetString("CollegeSelectionId");
        ViewData["CollegeSelectionId"] = collegeSelectionId;
        ModelState.Remove("Id");
        if (ModelState.IsValid)
        {
            // var collegeSelectionId = HttpContext.Session.GetString("CollegeSelectionId");
            if (!string.IsNullOrEmpty(collegeSelectionId))
            {


                model.Id = id.Value;
                ViewData["CollegeSelectionId"] = collegeSelectionId;
                model.CollegeSelectionId = Guid.Parse(collegeSelectionId);

                // Check if we're editing an existing criteria
                if (id.HasValue && id != Guid.Empty)
                {
                    // Fetch the existing criteria
                    var existingSchool = await _context.Schools.FindAsync(id.Value);
                    if (existingSchool != null)
                    {
                        existingSchool.Name = model.Name;
                        existingSchool.Info = model.Info;
                        _context.Schools.Update(existingSchool);
                        await _context.SaveChangesAsync();
                        TempData["Message"] = "School updated successfully";
                        return RedirectToAction("SchoolSelection");
                    }
                }

                if (model.Id == null || model.Id == Guid.Empty)
                {
                    if (!await _subscriptionService.CanAddSchool(userId))
                    {
                        TempData["ErrorMessage"] =
                            "You have reached the limit of Colleges. Please subscribe for more access.";
                        return RedirectToAction("SchoolSelection");
                    }

                    model.AddedBy = _myHelper.GetLoggedInUser();
                    _context.Schools.Add(model);
                    await _subscriptionService.IncrementUsage(userId, "college");
                    TempData["Message"] = "School Added successfully";
                }
                else
                {
                    _context.Update(model);
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("SchoolSelection");
        }

        return View("SchoolSelection");
    }

    public async Task<IActionResult> EditSchool(Guid Id)
    {
        var collegeSelectionId = HttpContext.Session.GetString("CollegeSelectionId");
        ViewData["CollegeSelectionId"] = collegeSelectionId;
        try
        {
            if (!string.IsNullOrEmpty(Id.ToString()))
            {
                var getSchool = await _context.Schools.Where(i => i.Id == Id).FirstOrDefaultAsync();
                return View("SchoolSelection", getSchool);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

        }

        return View("SchoolSelection");
    }

    public async Task<IActionResult> DeleteSchool(Guid Id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var collegeSelectionId = HttpContext.Session.GetString("CollegeSelectionId");
        ViewData["CollegeSelectionId"] = collegeSelectionId;
        try
        {
            if (!string.IsNullOrEmpty(Id.ToString()))
            {
                var getSchool = await _context.Schools.Where(i => i.Id == Id).FirstOrDefaultAsync();

                _context.Schools.Remove(getSchool);

                await _context.SaveChangesAsync();
                await _subscriptionService.DecrementUsage(userId, "college");
                TempData["Message"] = "School deleted successfully";
                return RedirectToAction("SchoolSelection");
                //return View("SchoolSelection");

            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

        }

        return View("SchoolSelection");
    }


    //Criteria
    public async Task<IActionResult> CriteriaSelection()
    {
        // Assuming you're storing the CollegeSelectionId in the session.
        var collegeSelectionId = HttpContext.Session.GetString("CollegeSelectionId");
        if (!string.IsNullOrEmpty(collegeSelectionId))
        {
            ViewData["CollegeSelectionId"] = collegeSelectionId;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var plan = await _subscriptionService.GetUserSubscriptionPlan(userId);
            var resultsCount = await _context.Criteria.CountAsync(s => s.AddedBy.Id == userId && !s.IsDeleted);
            ViewData["RemainingCriteria"] = plan.MaxCriteria - resultsCount;
            ViewData["PlanType"] = plan.Type;
            // Fetch related schools or initialize a new list if none exist.
            // var schools = _context.Schools.Where(s => s.CollegeSelectionId == Guid.Parse(collegeSelectionId)).ToList();
            return View();
        }
        else
        {
            return RedirectToAction("Index");
        }

        return View();
    }

    /*  public async Task<IActionResult> AddCriteria(Criteria model, Guid Id)
      {
          var collegeSelectionId = HttpContext.Session.GetString("CollegeSelectionId");
          ViewData["CollegeSelectionId"] = collegeSelectionId;
          if (string.IsNullOrEmpty(collegeSelectionId))
          {
              return View("index");
          }
          if (ModelState.IsValid)
          {
              // var collegeSelectionId = HttpContext.Session.GetString("CollegeSelectionId");
              if (!string.IsNullOrEmpty(collegeSelectionId))
              {
                     if (!string.IsNullOrEmpty(Id.ToString()))
                     {
                         var getSchool = await _context.Schools.Where(i => i.Id == Id).FirstOrDefaultAsync();

                     }

                  model.Id = Id;
                  ViewData["CollegeSelectionId"] = collegeSelectionId;
                  model.CollegeSelectionId = Guid.Parse(collegeSelectionId);
                  if (model.Id == null)
                  {
                      _context.Criteria.Add(model);
                  }
                  else
                  {
                      _context.Update(model);
                  }
                  await _context.SaveChangesAsync();
              }
              return RedirectToAction("CriteriaSelection");
          }

          return View("CriteriaSelection");
      }*/

    [HttpPost]
    public async Task<IActionResult> AddCriteria(string PredefinedCriteria, string CustomCriteria, string Importance,
        Guid? Id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var collegeSelectionId = HttpContext.Session.GetString("CollegeSelectionId");
        if (string.IsNullOrEmpty(collegeSelectionId))
        {
            return RedirectToAction("Index");
        }

        if (string.IsNullOrEmpty(PredefinedCriteria) && string.IsNullOrEmpty(CustomCriteria))
        {
            ModelState.AddModelError(string.Empty, "Please select a predefined criteria or enter a custom criteria.");
            TempData["ErrorMessage"] = "Please select a predefined criteria or enter a custom criteria.";
            return RedirectToAction("CriteriaSelection");
        }

        var criteriaName = !string.IsNullOrEmpty(PredefinedCriteria) ? PredefinedCriteria : CustomCriteria;

        // Check if we're editing an existing criteria
        if (Id.HasValue && Id != Guid.Empty)
        {
            // Fetch the existing criteria
            var existingCriteria = await _context.Criteria.FindAsync(Id.Value);
            if (existingCriteria != null)
            {
                existingCriteria.Name = criteriaName;
                existingCriteria.Importance = Importance;
                _context.Criteria.Update(existingCriteria);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Criteria Updated successfully.";
                return RedirectToAction("CriteriaSelection");
            }
        }

        // Adding a new criteria

        if (!await _subscriptionService.CanAddCriteria(userId))
        {
            TempData["ErrorMessage"] = "You have reached the limit of Criteria. Please subscribe for more access.";
            return RedirectToAction("CriteriaSelection");
        }

        var newCriteria = new Criteria
        {
            CollegeSelectionId = Guid.Parse(collegeSelectionId),
            Name = criteriaName,
            Importance = Importance,
            AddedBy = _myHelper.GetLoggedInUser()
        };

        _context.Criteria.Add(newCriteria);
        await _context.SaveChangesAsync();
        await _subscriptionService.IncrementUsage(userId, "criteria");

        TempData["Message"] = "Criteria added successfully.";


        return RedirectToAction("CriteriaSelection");
    }



    public async Task<IActionResult> EditCriteria(Guid Id)
    {
        var collegeSelectionId = HttpContext.Session.GetString("CollegeSelectionId");
        ViewData["CollegeSelectionId"] = collegeSelectionId;
        try
        {
            if (!string.IsNullOrEmpty(Id.ToString()))
            {
                var getCriteria = await _context.Criteria.Where(i => i.Id == Id).FirstOrDefaultAsync();
                return View("CriteriaSelection", getCriteria);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

        }

        return View("CriteriaSelection");
    }

    public async Task<IActionResult> DeleteCriteria(Guid Id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var collegeSelectionId = HttpContext.Session.GetString("CollegeSelectionId");
        ViewData["CollegeSelectionId"] = collegeSelectionId;
        try
        {
            if (!string.IsNullOrEmpty(Id.ToString()))
            {
                var getCriteria = await _context.Criteria.Where(i => i.Id == Id).FirstOrDefaultAsync();

                _context.Criteria.Remove(getCriteria);

                await _context.SaveChangesAsync();
                _subscriptionService.DecrementUsage(userId, "criteria");
                TempData["Message"] = "Criteria deleted successfully.";
                return RedirectToAction("CriteriaSelection");
                //return View("CriteriaSelection");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

        }

        return View("CriteriaSelection");
    }

    //Review Selection

    public async Task<IActionResult> ReviewSelection(Guid collegeSelectionId)
    {
        var colleges = await _context.Schools
            .Where(c => c.CollegeSelectionId == collegeSelectionId)
            .ToListAsync();

        var criteria = await _context.Criteria
            .Where(c => c.CollegeSelectionId == collegeSelectionId)
            .ToListAsync();

        var collegeSelection = await _context.CollegeSelections
            .FirstOrDefaultAsync(cs => cs.Id == collegeSelectionId);

        ViewBag.Colleges = colleges;
        ViewBag.Criteria = criteria;
        ViewBag.CollegeSelectionName = collegeSelection?.CollegeSelectionName;
        ViewBag.CollegeSelectionId = collegeSelectionId;

        ViewData["CollegeSelectionId"] = collegeSelectionId;

        return View();
    }


    //Evaluation
    public async Task<IActionResult> CollegeEvaluation()
    {
        var collegeSelectionId = HttpContext.Session.GetString("CollegeSelectionId");
        if (!string.IsNullOrEmpty(collegeSelectionId))
        {
            var criteriaSummaries = await _context.CollegeCriteriaSummaries
                .Where(x => x.CollegeSelectionId == Guid.Parse(collegeSelectionId))
                .Include(x => x.School)
                .Include(x => x.Criteria)
                .ToListAsync();

            var model = new EvaluationViewModel();

            model.SubmittedEvaluations = await _context.Evaluations
                .Where(i => i.CollegeSelectionId == Guid.Parse(collegeSelectionId)).ToListAsync();
            model.CollegeSelectionId = Guid.Parse(collegeSelectionId);
            model.SchoolEvaluations = new List<SchoolEvaluationViewModel>();
            model.GeneratedSummaries = criteriaSummaries.Select(x => new SummaryDisplayViewModel
            {
                SchoolName = x.School?.Name ?? "School",
                CriteriaName = x.Criteria?.Name ?? "Unknown",
                Summary = x.Summary
            }).ToList();
            // Example of fetching data for form and evaluations
            var Criteria = await _context.Criteria.Where(c => c.CollegeSelectionId == model.CollegeSelectionId)
                .ToListAsync();
            var Schools = await _context.Schools.Where(s => s.CollegeSelectionId == model.CollegeSelectionId)
                .ToListAsync();
            model.SubmittedEvaluations = await _context.Evaluations
                .Where(e => e.CollegeSelectionId == model.CollegeSelectionId).ToListAsync();

            var allCriteriaNames = Criteria.Select(c => c.Name).Distinct().ToList();
            var evaluations = model.SubmittedEvaluations.GroupBy(e => e.School).ToList();

            foreach (var group in evaluations)
            {
                var schoolViewModel = new SchoolEvaluationViewModel
                {
                    SchoolName = group.Key.Name,
                    CompletedCriteria = group.Select(e => e.Criteria.Name).Distinct().ToList(),
                    IsComplete = allCriteriaNames.All(c => group.Any(e => e.Criteria.Name == c))
                };
                model.SchoolEvaluations.Add(schoolViewModel);
            }


            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var plan = await _subscriptionService.GetUserSubscriptionPlan(userId);

            var resultsCount = 0;
            if (plan.Type == "Free")
            {
                resultsCount = _context.SelectionResults.Count(s => s.AddedBy.Id == userId && s.IsDeleted == false);
            }
            else
            {
                var getUserSubscription = await _subscriptionService.GetActiveSubscription(userId);
                if (getUserSubscription != null)
                {
                    resultsCount = getUserSubscription.UsedColleges;
                }
            }

            ViewData["RemainingSubmissions"] = plan.MaxSubmissions - resultsCount;
            ViewData["PlanType"] = plan.Type;


            ViewData["CollegeSelectionId"] = collegeSelectionId;
            return View(model);
        }
        else
        {
            return RedirectToAction("Index");
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SubmitEvaluation(EvaluationViewModel model)
    {
        var collegeSelectionId = HttpContext.Session.GetString("CollegeSelectionId");
        ViewData["CollegeSelectionId"] = collegeSelectionId;
        var model2 = new EvaluationViewModel();
        if (string.IsNullOrEmpty(collegeSelectionId))
        {
            return View("index");
        }

        ModelState.Remove("SubmittedEvaluations");
        ModelState.Remove("SchoolEvaluations");
        try
        {
            if (ModelState.IsValid)
            {
                var evaluation = new Evaluation
                {
                    SchoolId = model.SchoolId,
                    CriteriaId = model.CriteriaId,
                    Satisfaction = model.Satisfaction,
                    CollegeSelectionId = model.CollegeSelectionId,
                    Notes = model.Notes,
                    AddedBy = _myHelper.GetLoggedInUser()
                };
                _context.Evaluations.Add(evaluation);
                await _context.SaveChangesAsync();

                // Generate summary based on criteria & school
                var summary = await _context.CollegeCriteriaSummaries
                    .Where(x => x.CriteriaId == model.CriteriaId && x.SchoolId == model.SchoolId &&
                                x.CollegeSelectionId == Guid.Parse(collegeSelectionId)).Include(x => x.School)
                    .Include(x => x.Criteria).FirstOrDefaultAsync();


                if (summary == null || (DateTime.UtcNow > summary.LastChecked.AddYears(1) && !summary.ManuallyUpdated))
                {
                    var schoolName = (await _context.Schools.FindAsync(model.SchoolId))?.Name ?? "the school";
                    var criteriaName = (await _context.Criteria.FindAsync(model.CriteriaId))?.Name ?? "this criterion";

                    var prompt =
                        $"Give a short, helpful, real-world summary about {schoolName} in relation to {criteriaName}. Be concise.";
                    var output = await _openAIService.RunChatGPTPromptSummaryAsync(prompt); // Your OpenAI integration

                    if (summary == null)
                    {
                        summary = new CollegeCriteriaSummary
                        {
                            Id = Guid.NewGuid(),
                            SchoolId = model.SchoolId,
                            CriteriaId = model.CriteriaId,
                            CollegeSelectionId = model.CollegeSelectionId,
                            Summary = output,
                            DateAdded = DateTime.UtcNow,
                            LastChecked = DateTime.UtcNow,
                            Version = 1
                        };
                        _context.CollegeCriteriaSummaries.Add(summary);
                    }
                    else
                    {
                        summary.Summary = output;
                        summary.LastChecked = DateTime.UtcNow;
                        summary.Version += 1;
                        _context.CollegeCriteriaSummaries.Update(summary);
                    }

                    await _context.SaveChangesAsync();
                }

                var criteriaSummaries = await _context.CollegeCriteriaSummaries
                    .Where(x =>
                        x.SchoolId == model.SchoolId &&
                        x.CollegeSelectionId == Guid.Parse(collegeSelectionId))
                    .Include(x => x.Criteria).Include(collegeCriteriaSummary => collegeCriteriaSummary.School)
                    .ToListAsync();

                model.GeneratedSummaries = criteriaSummaries.Select(x => new SummaryDisplayViewModel
                    {
                        SchoolName = x.School?.Name ?? "School",
                        CriteriaName = x.Criteria?.Name ?? "Unknown",
                        Summary = x.Summary
                    })
                    .ToList();

                model.SubmittedEvaluations = await _context.Evaluations
                    .Where(i => i.CollegeSelectionId == Guid.Parse(collegeSelectionId)).ToListAsync();
                return RedirectToAction("CollegeEvaluation", model);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

        }

        return View("CollegeEvaluation", model);
    }


    //ReviewEvaluations
    public async Task<IActionResult> ReviewEvaluations()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var collegeSelectionId = HttpContext.Session.GetString("CollegeSelectionId");

            if (!await _subscriptionService.CanCreateSelection(userId))
            {
                TempData["ErrorMessage"] =
                    "You have reached your limit for viewing 'College Fit'. To continue, please upgrade your subscription.";
                return RedirectToAction("CollegeEvaluation");
            }

            if (!string.IsNullOrEmpty(collegeSelectionId))
            {
                ViewData["CollegeSelectionId"] = collegeSelectionId;

                var evaluations = await _context.Evaluations
                    .Where(i => i.CollegeSelectionId == Guid.Parse(collegeSelectionId)).Include(i => i.School)
                    .Include(i => i.Criteria).ToListAsync();
                var groupedEvaluations = evaluations.GroupBy(e => e.School.Name);
                StringBuilder finalData = new StringBuilder();

                foreach (var group in groupedEvaluations)
                {
                    finalData.AppendLine($"College: {group.Key}");
                    foreach (var eval in group)
                    {
                        finalData.AppendLine(
                            $"Criteria: {eval.Criteria.Name}, Criteria Importance: {eval.Criteria.Importance}, Criteria Score: {eval.Satisfaction}");
                    }

                    finalData.AppendLine();
                    /*var getSchool ="College: "+ a.School.Name;
                    var evaluate = "Criteria:" + a.Criteria.Name + ", Criteria Importance: " + a.Criteria.Importance +
                                   ", Criteria Score " + a.Satisfaction;*/

                    // finalData += "\n"+getSchool+" \n"+evaluate;

                }

                var output = finalData.ToString();
                string getWriteup = await _openAIService.RunChatGPTPromptAsync(output);

                ViewBag.Summary = getWriteup;

                //check if CollegeSelectionId is in Selection Results

                var getResults = await _context.SelectionResults
                    .Where(i => i.CollegeSelectionId == Guid.Parse(collegeSelectionId)).FirstOrDefaultAsync();
                if (getResults == null)
                {
                    var result = new SelectionResults()
                    {
                        CollegeSelectionId = Guid.Parse(collegeSelectionId),
                        Result = getWriteup,
                        AddedBy = _myHelper.GetLoggedInUser()

                    };
                    _context.SelectionResults.Add(result);
                    var history = new CollegeFitHistory()
                    {
                        CollegeSelectionId = Guid.Parse(collegeSelectionId),
                        Result = getWriteup,
                        AddedBy = _myHelper.GetLoggedInUser()

                    };

                    _context.CollegeFitHistories.Add(history);
                }
                else
                {
                    getResults.Result = getWriteup;
                    _context.SelectionResults.Update(getResults);

                    var history = new CollegeFitHistory()
                    {
                        CollegeSelectionId = Guid.Parse(collegeSelectionId),
                        Result = getWriteup,
                        AddedBy = _myHelper.GetLoggedInUser()

                    };

                    _context.CollegeFitHistories.Add(history);
                }


                await _context.SaveChangesAsync();

                await _subscriptionService.IncrementUsage(userId, "result");


                Console.WriteLine(getWriteup);
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return View();
    }

    public IActionResult ViewSelection()
    {
        return View();
    }

    public IActionResult ViewSelection1()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetEvaluationDetails(Guid id)
    {
        if (!User.Identity.IsAuthenticated)
        {
            TempData["ErrorMessage"] = "Session Expire. Please Login";
            // Redirect to login page if the user is not authenticated
            return Unauthorized("Session expired. Please log in again.");
        }

        var results = await _context.SelectionResults.FirstOrDefaultAsync(e => e.CollegeSelectionId == id);
        if (results == null)
        {
            return NotFound("Evaluation not found.");
        }

        // Return a partial view with evaluation details
        return PartialView("_EvaluationDetailsPartial", results);
    }


    public async Task<IActionResult> ResumeEvaluation(Guid collegeSelectionId)
    {
        // Store the CollegeSelectionId in the session
        HttpContext.Session.SetString("CollegeSelectionId", collegeSelectionId.ToString());

        // Fetch necessary data for the evaluation
        var collegeSelection = await _context.CollegeSelections
            .Include(cs => cs.AddedBy)
            .FirstOrDefaultAsync(cs => cs.Id == collegeSelectionId);

        if (collegeSelection == null)
        {
            return NotFound("College selection not found.");
        }

        // Redirect to the SchoolSelection or CriteriaSelection page depending on progress
        var hasSchools = await _context.Schools.AnyAsync(s => s.CollegeSelectionId == collegeSelectionId);
        var hasCriteria = await _context.Criteria.AnyAsync(c => c.CollegeSelectionId == collegeSelectionId);

        if (!hasSchools)
        {
            return RedirectToAction("SchoolSelection");
        }
        else if (!hasCriteria)
        {
            return RedirectToAction("CriteriaSelection");
        }

        return RedirectToAction("CollegeEvaluation");
    }

    public IActionResult Reset()
    {
        // Clear session variables
        HttpContext.Session.Clear();

        // Redirect to the Index action to start a new college selection
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> GetCollegeSelections(int page = 1, int pageSize = 10)
    {
        try
        {
            var userName = User.Identity.Name;
            IQueryable<CollegeSelection> query = _context.CollegeSelections.Include(i => i.AddedBy);

            if (!User.IsInRole("Admin"))
            {
                query = query.Where(i => i.AddedBy.UserName == userName);
            }

            var totalRecords = await query.CountAsync();

            var selections = await query
                .OrderByDescending(i => i.DateAdded)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Json(new
            {
                TotalRecords = totalRecords,
                Data = selections.Select(s => new
                {
                    s.Id,
                    s.CollegeSelectionName,
                    DateAdded = s.DateAdded?.ToString("dd MMM, yyyy"),
                    AddedBy = s.AddedBy != null ? $"{s.AddedBy.FirstName} {s.AddedBy.LastName}" : "Null",
                    Email = s.AddedBy?.Email ?? "Null",
                    Country = s.AddedBy?.Country ?? "Null",
                    Status = _context.SelectionResults.Any(r => r.CollegeSelectionId == s.Id)
                        ? "Completed"
                        : "Not Completed"
                })
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetPaginatedCollegeSelections(int start = 0, int length = 10,
        string searchValue = "")
    {
        try
        {
            var userName = User.Identity.Name;
            IQueryable<CollegeSelection> query = _context.CollegeSelections.Include(i => i.AddedBy);

            if (!User.IsInRole("Admin"))
            {
                query = query.Where(i => i.AddedBy.UserName == userName);
            }

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(s =>
                    s.CollegeSelectionName.Contains(searchValue) || s.AddedBy.UserName.Contains(searchValue) ||
                    s.AddedBy.Email.Contains(searchValue) || s.AddedBy.Country.Contains(searchValue));
            }

            var totalRecords = await query.CountAsync();

            var paginatedData = await query
                .OrderByDescending(i => i.DateAdded)
                .Skip(start)
                .Take(length)
                .ToListAsync();

            var data = paginatedData.Select(s => new
            {
                s.Id,
                s.CollegeSelectionName,
                DateAdded = s.DateAdded?.ToString("dd MMM, yyyy"),
                AddedBy = s.AddedBy != null ? $"{s.AddedBy.FirstName} {s.AddedBy.LastName}" : "Null",
                Email = s.AddedBy?.Email ?? "Null",
                Country = s.AddedBy?.Country ?? "Null",
                Status = _context.SelectionResults.Any(r => r.CollegeSelectionId == s.Id)
                    ? "Completed"
                    : "Not Completed"
            });

            return Json(new
            {
                draw = HttpContext.Request.Query["draw"].FirstOrDefault(),
                recordsTotal = totalRecords,
                recordsFiltered = totalRecords,
                data = data
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

//View College fit History
    public async Task<IActionResult> ViewHistorySelection(Guid collegeSelectionId)
    {
        var getResults = await _context.CollegeFitHistories.Where(i => i.CollegeSelectionId == collegeSelectionId)
            .Include(i => i.AddedBy).ToListAsync();


        return View(getResults);
    }

    [HttpGet]
    public async Task<IActionResult> GetHistoryDetails(Guid id)
    {
        if (!User.Identity.IsAuthenticated)
        {
            TempData["ErrorMessage"] = "Session Expire. Please Login";
            // Redirect to login page if the user is not authenticated
            return Unauthorized("Session expired. Please log in again.");
        }

        var results = await _context.CollegeFitHistories.FirstOrDefaultAsync(e => e.Id == id);
        if (results == null)
        {
            return NotFound("Evaluation not found.");
        }

        // Return a partial view with evaluation details
        return PartialView("_CollegeHistoryDetailsPartial", results);
    }



    public async Task<IActionResult> GenerateRecommendation(Guid collegeSelectionId)
    {
        // Call your AI service here to generate recommendations
        // This is a placeholder - implement your actual AI integration

        var recommendation = await _collegeService.GenerateRecommendationAsync(collegeSelectionId);

        // Store the recommendation temporarily
        TempData["Recommendation"] = recommendation;
        ViewBag.Recommendation = recommendation;

        return RedirectToAction("RecommendationResult", new { collegeSelectionId });
    }

    public async Task<IActionResult> RecommendationResult(Guid collegeSelectionId)
    {
        // Retrieve the college selection
        var collegeSelection = await _context.CollegeSelections
            .FirstOrDefaultAsync(cs => cs.Id == collegeSelectionId);

        var recommendation = ViewBag.Recommendation;

        if (collegeSelection == null)
        {
            return NotFound();
        }

        var results = new ResultsViewModel()
        {
            CollegeSelectionId = collegeSelectionId,
        };

        return View(results);
    }

    public async Task<IActionResult> SaveRecommendationResults(ResultsViewModel results)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var collegeSelectionId = HttpContext.Session.GetString("CollegeSelectionId");

            if (!await _subscriptionService.CanCreateSelection(userId))
            {
                TempData["ErrorMessage"] =
                    "You have reached your limit for viewing 'College Fit'. To continue, please upgrade your subscription.";
                return RedirectToAction("CollegeEvaluation");
            }

            if (!string.IsNullOrEmpty(collegeSelectionId))
            {
                ViewData["CollegeSelectionId"] = collegeSelectionId;


                //check if CollegeSelectionId is in Selection Results

                var getResults = await _context.SelectionResults
                    .Where(i => i.CollegeSelectionId == Guid.Parse(collegeSelectionId)).FirstOrDefaultAsync();
                if (getResults == null)
                {
                    var result = new SelectionResults()
                    {
                        CollegeSelectionId = Guid.Parse(collegeSelectionId),
                        Result = results.Result,
                        AddedBy = _myHelper.GetLoggedInUser()

                    };
                    _context.SelectionResults.Add(result);
                    var history = new CollegeFitHistory()
                    {
                        CollegeSelectionId = Guid.Parse(collegeSelectionId),
                        Result = results.Result,
                        AddedBy = _myHelper.GetLoggedInUser()

                    };

                    _context.CollegeFitHistories.Add(history);
                }
                else
                {
                    getResults.Result = results.Result;
                    _context.SelectionResults.Update(getResults);

                    var history = new CollegeFitHistory()
                    {
                        CollegeSelectionId = Guid.Parse(collegeSelectionId),
                        Result = results.Result,
                        AddedBy = _myHelper.GetLoggedInUser()

                    };

                    _context.CollegeFitHistories.Add(history);
                }


                await _context.SaveChangesAsync();

                await _subscriptionService.IncrementUsage(userId, "result");

                TempData["Message"] = "Results have been saved.";
                
                return RedirectToAction("Reset");

                return View("index");
            }
            else
            {
                return RedirectToAction("Index");
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return View();
    }




}
