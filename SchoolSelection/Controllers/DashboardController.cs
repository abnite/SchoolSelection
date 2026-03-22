using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolSelection.Data;

namespace SchoolSelection.Controllers;
[Authorize (Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly CollegeDbContext _context;

    public DashboardController(CollegeDbContext context)
    {
        _context = context;
    }
    // GET
    public IActionResult Index()
    {
        return View();
    }
    
    [HttpGet]
    public async Task<IActionResult> GetDashboardData(int year)
    {
        try
        {
            var currentYear =year > 0 ? year :  DateTime.Now.Year;

            // Total Registered Users
            var totalUsers = await _context.Users.CountAsync();

            // Total Created College Selections
            var totalCollegeSelections = await _context.CollegeSelections.CountAsync();

            // Total Completed Selections
            var totalCompletedSelections = await _context.SelectionResults.CountAsync();

            // Created College Selections by Month
            var monthlySelections = await _context.CollegeSelections
                .Where(cs => cs.DateAdded.HasValue && cs.DateAdded.Value.Year == currentYear)
                .GroupBy(cs => cs.DateAdded.Value.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            // Ensure all months are included
            var monthlyData = Enumerable.Range(1, 12).Select(month => new
            {
                Month = month,
                Count = monthlySelections.FirstOrDefault(m => m.Month == month)?.Count ?? 0
            }).ToList();

            return Json(new
            {
                totalUsers,
                totalCollegeSelections,
                totalCompletedSelections,
                monthlyData
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


}