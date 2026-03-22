using System.Security.Claims;
using System.Text;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using SchoolSelection.Data;
using SchoolSelection.Interfaces;
using SchoolSelection.Models;
using SchoolSelection.ViewModel;

namespace CollegeController.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> SignInManager;
    public readonly RoleManager<ApplicationRole> RoleManager;
    public readonly CollegeDbContext Db;
    private readonly IConfiguration _configuration;
    private readonly IEmailSender _emailService;
    private readonly CollegeDbContext _context;
    
    public AccountController(CollegeDbContext context,IEmailSender emailService,UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> SignInManager,
        RoleManager<ApplicationRole> RoleManager, CollegeDbContext Db,IConfiguration configuration)
    {
        this.SignInManager = SignInManager;
        this.RoleManager = RoleManager;
        this.userManager = userManager;
        this.Db = Db;
        _configuration = configuration;
        _emailService = emailService;
        _context = context;

    }
    [Authorize (Roles = "Admin")]
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Terms_Conditions()
    {
        return View();
    }
    public IActionResult Privacy_Policy()
    {
        return View();
    }

    public IActionResult Support()
    {
        return View();
    }
  

    public IActionResult AddUser()
    {
        return View("AddUser");
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
            if (result.Succeeded)
            {
                var checkConfirm = await Db.Users.Where(i => i.UserName == model.Email).FirstOrDefaultAsync();
                if (checkConfirm?.IsConfirmed == false || checkConfirm.EmailConfirmed==false || checkConfirm.IsConfirmed==null)
                {
                    TempData["Message"] = "Please check your email to confirm your account before logging in.";
                    return View("Login");
                }
                else
                {
                    // Record login time
                    checkConfirm.LastLoginTime = DateTime.UtcNow;
                    await userManager.UpdateAsync(checkConfirm);
                    
                    // Save a login record to the database (optional)
                    var loginRecord = new UserActivityLog
                    {
                        UserId = checkConfirm.Id,
                        ActivityType = "Login",
                        Timestamp = DateTime.UtcNow
                    };
                    _context.UserActivityLogs.Add(loginRecord);
                    await _context.SaveChangesAsync();
                     return RedirectToAction("index", "College");
                }
               
            }
            else
            {
                TempData["ErrorMessage"] ="Invalid Login Details";
            }

            ModelState.AddModelError(string.Empty, " Invalid Login Details");

        }
        return View(model);
    }
    
    public IActionResult GoogleLogin()
    {
        return Challenge(new AuthenticationProperties { RedirectUri = "Account/GoogleLoginCallback" }, GoogleDefaults.AuthenticationScheme);
    }
    
    public async Task<IActionResult> GoogleLoginCallback()
    {
        var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        if (!authenticateResult.Succeeded)
        return RedirectToAction("Login");
        
        var email = authenticateResult.Principal.FindFirst(ClaimTypes.Email)?.Value;
        var name = authenticateResult.Principal.FindFirst(ClaimTypes.Name)?.Value;
        var FirstName = authenticateResult.Principal.FindFirst(ClaimTypes.GivenName)?.Value;
        var Lastname = authenticateResult.Principal.FindFirst(ClaimTypes.Surname)?.Value;
        var Country = authenticateResult.Principal.FindFirst(ClaimTypes.Country)?.Value;
        var PhoneNo = authenticateResult.Principal.FindFirst(ClaimTypes.MobilePhone)?.Value;
        
        //var info = await SignInManager.GetExternalLoginInfoAsync();
        var info = new ExternalLoginInfo(authenticateResult.Principal, 
            GoogleDefaults.AuthenticationScheme, 
            email, 
            "Google");
        if (info == null)
        {
            // Handle error scenario
            return RedirectToAction(nameof(Login));
        }
        
        // Sign in the user with this external login provider if the user already has a login.
        var result = await SignInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

        if (result.Succeeded)
        {
            // User successfully signed in with their Google account
            return RedirectToAction("index", "GeneratePaper");
        }
        else
        {
            
                // User does not exist in your system, create a new user account or link the Google account
              //  var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var user = await userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    // User does not exist, create a new user
                   // user = new ApplicationUser { UserName = email, Email = email };
                    user = new ApplicationUser
                   {
                       UserName = email, Email = email, FirstName = FirstName, LastName = Lastname,
                       Country = Country, PhoneNumber = PhoneNo, IsConfirmed = true,EmailConfirmed = true,
                   };
                    var createUserResult = await userManager.CreateAsync(user);
                    if (!createUserResult.Succeeded)
                    {
                        // Handle user creation failure
                        return RedirectToAction(nameof(Login));
                    }
                    else
                    {
                        var addRole = await userManager.AddToRoleAsync(user, "user");

                        ViewData["Email"] = user.Email;
                    }
                }

                // Link the Google account to the user account
                var addLoginResult = await userManager.AddLoginAsync(user, info);
                if (!addLoginResult.Succeeded)
                {
                    // Handle link failure
                    return RedirectToAction(nameof(Login));
                }

                // Sign in the user
                await SignInManager.SignInAsync(user, isPersistent: false);
                var checkUserDetails = await _context.Users.Where(i => i.Email == email).FirstOrDefaultAsync();

                if (checkUserDetails.Country == null || checkUserDetails.PhoneNumber == null)
                {
                    ViewData["Email"] = email;
                    return RedirectToAction("CompleteFromGoogle", "GeneratePaper",new{Email=email});

                }
                return RedirectToAction("index", "GeneratePaper");
            
            
            
            // If the user does not have an account, then you may create one here and then sign them in
          //  var email = info.Principal.FindFirstValue(ClaimTypes.Email);
         /*   var user = new ApplicationUser {   UserName = email, Email = email, FirstName = FirstName, LastName = Lastname,
                Country = Country, PhoneNumber = PhoneNo, EmailConfirmed = true,
                IsConfirmed = true,
            };
            var createUserResult = await userManager.CreateAsync(user,"hdsfjdsfbdsfsdbksdjdfg67885564");
            if (createUserResult.Succeeded)
            {
                var addRole = await userManager.AddToRoleAsync(user, "user");
                await userManager.AddLoginAsync(user, info);
                await SignInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("index", "GeneratePaper");
            }*/
        }
        

        // Now sign in the user with your application logic

        return RedirectToAction("Index", "GeneratePaper");
    }

    public async Task<IActionResult> Logout()
    {
        var user = await userManager.GetUserAsync(User);

        if (user != null)
        {
            // Record logout time
            var logoutRecord = new UserActivityLog
            {
                UserId = user.Id,
                ActivityType = "Logout",
                Timestamp = DateTime.UtcNow
            };
            _context.UserActivityLogs.Add(logoutRecord);
            await _context.SaveChangesAsync();
        }
        await SignInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Create a new user
            var user = new ApplicationUser
            {
                UserName = model.Email, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName,
                Country = model.Country, PhoneNumber = model.PhoneNo, ReferralCode = model.ReferralCode
            };
            var result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                //Assign Right
                var addRole = await userManager.AddToRoleAsync(user, "user");
                // Generate an email confirmation token
                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token },
                    Request.Scheme);

                // Send confirmation email
                await SendConfirmationEmailAsync(user.Email, confirmationLink);
                
                TempData["Message"] =" Confirmation Email Sent. Please Check you Email and Confirm Account";
                return View("Login");
            }
            else
            {
                string errorMessage = "";
                // Add errors to ModelState for display in the view
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    errorMessage += error.Description +"\n";
                }

                TempData["ErrorMessage"] = errorMessage;
            }
        }
        return View(model);
    }
    
    /*private async Task SendConfirmationEmailAsync(string email, string link)
    {
        try
        {
            string to = email;
            string subject = "Email Confirmation";
            string message = $"Please confirm your email by clicking this link: {link}";
           // _emailService.SendEmailAsync(to, subject, message);
            _emailService.SendGmailEmailAsync(to, subject, message);
            
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
           
        }
    }*/
    
    private async Task SendConfirmationEmailAsync(string email, string link)
    {
        try
        {
            string to = email;
            string subject = "Confirm Your Email Address";
            string message = $@"
                <html>
                <head>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            line-height: 1.6;
                        }}
                        .email-container {{
                            max-width: 600px;
                            margin: auto;
                            padding: 20px;
                            border: 1px solid #e0e0e0;
                            border-radius: 8px;
                            background-color: #f9f9f9;
                        }}
                        .btn {{
                            display: inline-block;
                            padding: 15px 25px; /* Adjusted padding for better appearance */
                            font-size: 18px; /* Increased font size for readability */
                            color: #ffffff; /* Ensures white text */
                            background-color: #007bff; /* Button background */
                            text-decoration: none;
                            border-radius: 5px;
                            font-weight: bold; /* Makes text bolder */
                            text-align: center; /* Ensures text is centered */
                            transition: background-color 0.3s ease; /* Smooth hover effect */
                        }}
                        .btn:hover {{
                            background-color: #0056b3;
                        }}
                    </style>
                </head>
                <body>
                    <div class='email-container'>
                        <h2>Email Confirmation</h2>
                        <p>Dear User,</p>
                        <p>Thank you for registering with us. Please confirm your email address by clicking the button below:</p>
                        <p>
                            <a href='{link}' class='btn'>Confirm Email</a>
                        </p>
                        <p>If you did not register for this account, please disregard this email.</p>
                        <p>Best regards,</p>
                        <p><strong>Frooot Pizza</strong></p>
                    </div>
                </body>
                </html>";

            await _emailService.SendGmailEmailAsync(to, subject, message);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "An error occurred while sending the confirmation email: " + ex.Message;
        }
}

    
    //Confirm Email
    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (userId == null || token == null)
        {
            // Handle invalid token
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            // Handle user not found
        }

        var result = await userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            var getUser = await Db.Users.Where(i => i.Email == user.Email).FirstOrDefaultAsync();
            getUser.IsConfirmed = true;
            getUser.EmailConfirmed = true;

            await Db.SaveChangesAsync();
            TempData["Message"] ="Email Confirmed. Please Login";
            return View("Login");
        }
        else
        {
            // Email confirmation failed
            TempData["ErrorMessage"] ="Email was not Confirmed. Please Click on the Link again";
            //TempData["Message"] ="Email Confirmed. Please Login";
        }
        return View("Login");
    }

    public IActionResult Profile()
    {
        return View();
    }
       
    [Authorize]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] ="Error. Check your input";
            return View("Profile");
        }

        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var changePasswordResult = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            foreach (var error in changePasswordResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            TempData["ErrorMessage"] ="Wrong password input.";
            return View("Profile");
        }

        await SignInManager.RefreshSignInAsync(user);
        TempData["Message"] = "Password Changed";
        return View("Profile");
    }
    
    [HttpGet]
    public IActionResult passwordRecovery()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> passwordRecovery(PasswordRecoveryViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                var resetLink = Url.Action("ResetPassword", "Account", new { email = user.Email, token=encodedToken }, Request.Scheme);
                
                await SendPasswordResetTokenAsync(user.Email, resetLink);
                TempData["Message"] =" A password reset link has been sent to your email address. Please check your inbox.";
                return View("Login");
            }
            else
            {
                TempData["ErrorMessage"] ="Email Address does not exist";
                
            }
        }
        return View();
    }
    //Password Reset Email Token 
   /* private async Task SendPasswordResetTokenAsync(string email, string link)
    {
        try
        {
            string to = email;
            string subject = "Password Reset Confirmation";
            string message = $"Please Reset your Password by clicking this link. The link will expire in 24 hours: {link}";
            //_emailService.SendEmailAsync(to, subject, message);
            _emailService.SendGmailEmailAsync(to, subject, message);

        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
           
        }
    }
    */
   private async Task SendPasswordResetTokenAsync(string email, string link)
    {
        try
        {
            string to = email;
            string subject = "Password Reset Request";
            string message = $@"
                <html>
                <head>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            line-height: 1.6;
                        }}
                        .email-container {{
                            max-width: 600px;
                            margin: auto;
                            padding: 20px;
                            border: 1px solid #e0e0e0;
                            border-radius: 8px;
                            background-color: #f9f9f9;
                        }}
                        .btn {{
                            display: inline-block;
                            padding: 15px 25px;
                            font-size: 16px;
                            color: #ffffff;
                            background-color: #007bff;
                            text-decoration: none;
                            border-radius: 5px;
                            font-weight: bold;
                            text-align: center;
                        }}
                        .btn:hover {{
                            background-color: #0056b3;
                        }}
                    </style>
                </head>
                <body>
                    <div class='email-container'>
                        <h2>Password Reset Request</h2>
                        <p>Dear User,</p>
                        <p>We received a request to reset your password. You can reset your password by clicking the button below:</p>
                        <p>
                            <a href='{link}' class='btn'>Reset Password</a>
                        </p>
                        <p>Please note that this link will expire in 24 hours. If you did not request a password reset, you can safely ignore this email.</p>
                        <p>Best regards,</p>
                        <p><strong>Frooot Pizza</strong></p>
                    </div>
                </body>
                </html>";

            await _emailService.SendGmailEmailAsync(to, subject, message);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "An error occurred while sending the password reset email: " + ex.Message;
        }
    }

    [HttpGet]
    public IActionResult ResetPassword(string token, string email)
    {
        var model = new ResetPasswordViewModel { Token = token, Email = email };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
                var result = await userManager.ResetPasswordAsync(user, decodedToken, model.Password);
                if (result.Succeeded)
                {
                    // Password reset successful, redirect to login or confirmation page
                    TempData["Message"] =" Password Changed";
                    return RedirectToAction("Login");
                }
                else
                {
                    var errorMsg = "";
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                        errorMsg = error.Description;
                    }

                    TempData["ErrorMessage"] = "Password Expired or " + errorMsg;
                }
            }
        }
        else
        {
            TempData["ErrorMessage"] = "Please check your inbox and click on the link again";
        }

        return View(model);
    }
    
    
   /* [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendEmail(ContactViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                string fromemail = model.fromEmail;
                string subject = $"{model.subject}. From Scholar WriteAI Website";
                string message = $"Email From. Name : {model.Name},\n\n" +
                                 $"<br></br>" +
                                 $" Email Address: {model.fromEmail},\n\n" +
                                 $"<br></br>" +
                                 $"Message : {model.Message},\n\n";
                                   
                _emailService.ContactSendEmailAsync(fromemail, subject, message);
                 _emailService.ContactSendGmailEmailAsync(fromemail, subject, message);
                TempData["Message"] = "Email Sent";
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = e.Message;
            }
        }
        else
        {
            TempData["ErrorMessage"] = "Error. Check your inputs";
        }

        return View("Support");
    }*/
   [Authorize (Roles = "Admin")]
   [HttpGet]
   public async Task<IActionResult> GetPaginatedAllUsers(int start = 0, int length = 10, string searchValue = "")
   {
       try
       {
           IQueryable<ApplicationUser> query = _context.Users;

           // Apply search filter if a search value exists
           if (!string.IsNullOrEmpty(searchValue))
           {
               query = query.Where(s => 
                   s.UserName.Contains(searchValue) || 
                   s.Email.Contains(searchValue) || 
                   s.Country.Contains(searchValue) || 
                   s.FirstName.Contains(searchValue) || 
                   s.LastName.Contains(searchValue));
           }

           var totalRecords = await _context.Users.CountAsync(); // Total unfiltered records
           var filteredRecords = await query.CountAsync(); // Total filtered records

           // Fetch paginated data
           var paginatedData = await query
               .OrderBy(s => s.FirstName)
               .Skip(start)
               .Take(length)
               .ToListAsync();

           // Map data to the expected structure
           var data = paginatedData.Select(s => new
           {
               s.Id,
               Email = s.Email,
               Name = $"{s.FirstName} {s.LastName}",
               PhoneNo = s.PhoneNumber ?? "N/A",
               Country = s.Country ?? "N/A",
               Status = s.EmailConfirmed ? "Confirmed" : "Not Confirmed",
               LastLogin=s.LastLoginTime.HasValue ? s.LastLoginTime.Value.ToString("dd/MMM/yyyy HH:mm:ss") : "N/A"
           });

           // Return the JSON response for DataTables
           return Json(new
           {
               draw = HttpContext.Request.Query["draw"].FirstOrDefault(),
               recordsTotal = totalRecords,
               recordsFiltered = filteredRecords,
               data = data
           });
       }
       catch (Exception ex)
       {
           return BadRequest(new { message = ex.Message });
       }
   }
   
   
   [Authorize (Roles = "Admin")]
   [HttpGet]
   public async Task<IActionResult> ActivityLogs(int start = 0, int length = 10, string searchValue = "")
   {
       try
       {
           IQueryable<UserActivityLog> query = _context.UserActivityLogs.Include(i=>i.User);

           // Apply search filter if a search value exists
           if (!string.IsNullOrEmpty(searchValue))
           {
               query = query.Where(s => 
                   s.ActivityType.Contains(searchValue) || 
                   s.User.FirstName.Contains(searchValue) || 
                   s.User.Country.Contains(searchValue) || 
                   s.User.Email.Contains(searchValue) || 
                   s.User.LastName.Contains(searchValue));
           }

           var totalRecords = await _context.UserActivityLogs.CountAsync(); // Total unfiltered records
           var filteredRecords = await query.CountAsync(); // Total filtered records

           // Fetch paginated data
           var paginatedData = await query
               .OrderByDescending(s => s.Timestamp)
               .Skip(start)
               .Take(length)
               .ToListAsync();

           // Map data to the expected structure
           var data = paginatedData.Select(s => new
           {
               s.Id,
               Email = s.User.Email,
               Name = $"{s.User.FirstName} {s.User.LastName}",
               Country = s.User.Country ?? "N/A",
               ActivityType = s.ActivityType?? "N/A",
               ActivityDate =s.Timestamp.ToString("dd/MMM/yyyy h:mm:ss tt"),
           });

           // Return the JSON response for DataTables
           return Json(new
           {
               draw = HttpContext.Request.Query["draw"].FirstOrDefault(),
               recordsTotal = totalRecords,
               recordsFiltered = filteredRecords,
               data = data
           });
       }
       catch (Exception ex)
       {
           return BadRequest(new { message = ex.Message });
       }
   }

   [Authorize (Roles = "Admin")]
   [HttpGet]
   public async Task<IActionResult> ActivityLog()
   {
       return View();
   }

   public IActionResult community()
   {
       return View();
   }
   
   public IActionResult Events()
   {
       return View();
   }
   
   
   public IActionResult About_Us()
   {
       return View();
   }


}