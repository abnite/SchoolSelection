using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolSelection.Data;
using SchoolSelection.Models;

namespace SchoolSelection.Controllers;

[Authorize]
public class ContactController : Controller
{
    private CollegeDbContext _context;

    public ContactController(CollegeDbContext context)
    {
        _context = context;
    }
    
    // GET
    [Authorize(Roles = "Admin")]
    public IActionResult Index()
    {
        return View();
    }
    // GET
    public IActionResult Contact()
    {
        return View("Contact");
    }
    
    [HttpPost]
    public async Task<IActionResult> SubmitInquiry(ContactInquiry model)
    {
        if (ModelState.IsValid)
        {
            // Save the inquiry to the database
            _context.ContactInquiries.Add(model);
            await _context.SaveChangesAsync();

            // You can also send an email here if required

            return Ok(new { success = true });
        }
        return BadRequest(new { success = false, message = "Invalid data." });
    }
    
    // Get all inquiries based on status
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult GetInquiries(string status, int page = 1, int itemsPerPage = 10)
    {
        var inquiries = _context.ContactInquiries.AsQueryable();

        if (status == "unread")
        {
            inquiries = inquiries.Where(i => i.Status == "Unread");
        }
        else if (status == "read")
        {
            inquiries = inquiries.Where(i => i.Status == "Read");
        }
        else if (status == "resolved")
        {
            inquiries = inquiries.Where(i => i.Status == "Resolved");
        }

        var totalItems = inquiries.Count();
        var data = inquiries
            .OrderByDescending(i => i.SubmittedAt)
            .Skip((page - 1) * itemsPerPage)
            .Take(itemsPerPage)
            .Select(i => new
            {
                i.Id,
                i.Name,
                i.Email,
                i.Subject,
                SubmittedAt = i.SubmittedAt.ToString("dd/MM/yyyy HH:mm"),
                i.Status
            })
            .ToList();

        return Json(new { data, totalItems });
    }


    // Mark an inquiry as read
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var inquiry = await _context.ContactInquiries.FindAsync(id);
        if (inquiry == null) return NotFound();

        inquiry.Status = "Read";
        await _context.SaveChangesAsync();
        return Ok();
    }

    // Mark an inquiry as resolved
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> MarkAsResolved(int id)
    {
        var inquiry = await _context.ContactInquiries.FindAsync(id);
        if (inquiry == null) return NotFound();

        inquiry.Status = "Resolved";
        await _context.SaveChangesAsync();
        return Ok();
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetInquiryDetails(int id)
    {
        var inquiry = await _context.ContactInquiries.FindAsync(id);
        if (inquiry == null) return NotFound();

        // Mark as read if the current status is "Unread"
        if (inquiry.Status == "Unread")
        {
            inquiry.Status = "Read";
            await _context.SaveChangesAsync();
        }

        var result = new
        {
            inquiry.Name,
            inquiry.Email,
            inquiry.Subject,
            SubmittedAt = inquiry.SubmittedAt.ToString("dd/MM/yyyy HH:mm"),
            inquiry.Message,
            inquiry.Status
        };

        return Json(result);
    }

}