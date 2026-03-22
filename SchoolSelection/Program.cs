using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Project_Articles.ApplicationClass;
using Project_Articles.Services;
using SchoolSelection.ApplicationClass;
using SchoolSelection.Data;
using SchoolSelection.Interfaces;
using SchoolSelection.Services;
using SchoolSelection.ViewModels;

var builder = WebApplication.CreateBuilder(args);
// Configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json")
    .Build();

// Add services to the container.
builder.Configuration.AddJsonFile("appsettings.json");
var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();
string openAIApiKey =appSettings.OpenAIApiKey;

/*builder.Services.AddAuthentication(option=>
{
    option.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;

}).AddCookie().AddGoogle(googleOptions =>
{
    googleOptions.ClientId = configuration["GoogleSettings:ClientID"];
    googleOptions.ClientSecret = configuration["GoogleSettings:ClientSecretKey"];
} );*/

builder.Services.AddTransient<IOpenAIService>(provider => new OpenAIService(openAIApiKey));

// Add services to the container.
builder.Services.AddControllersWithViews().AddSessionStateTempDataProvider();
builder.Services.AddDbContext<CollegeDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("CollegeDefault")), ServiceLifetime.Scoped);
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(o =>
{
    o.Password.RequireDigit = false;
    o.Password.RequireNonAlphanumeric = false;
    o.Password.RequireUppercase = false;
    o.Password.RequireLowercase = false;
    o.Lockout.MaxFailedAccessAttempts = 6;
    o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(60);
}).AddDefaultTokenProviders().AddEntityFrameworkStores<CollegeDbContext>();

// Configure session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
  
});

builder.Services.Configure<CookiePolicyOptions>(o =>
{
    o.CheckConsentNeeded = context => true;
    o.MinimumSameSitePolicy = SameSiteMode.None;
});
builder.Services.ConfigureApplicationCookie(a =>
{
    a.AccessDeniedPath = "";
    a.Cookie.Name = "CollegeSelectionApp";
    a.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    a.LoginPath = "/Account/Login";
    a.LogoutPath = "";
    a.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
    a.SlidingExpiration = true;
    a.Cookie.SameSite = SameSiteMode.None;
    a.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    a.Cookie.HttpOnly = true;
});
//builder.Services.AddScoped<IGoogleScholar, GoogleScholarService>();
builder.Services.AddScoped<seed>();
builder.Services.AddTransient<DataHelper>();
builder.Services.AddScoped<IEmailSender,EmailSenderService>();
builder.Services.AddScoped<SystemSettingsService>();
builder.Services.AddScoped<CollegeService>();
builder.Services.AddScoped<SubscriptionService>();
builder.Services.AddHostedService<SubscriptionStatusUpdater>();
builder.Services.Configure<PayPalSettings>(builder.Configuration.GetSection("PayPal"));
// Register PayPalService with HttpClient
builder.Services.AddHttpClient<PayPalService>();
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(24); // Adjust the duration as needed
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder.WithOrigins("https://your-wix-domain.com") // Replace with actual domain
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});




var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<CollegeDbContext>();
    var seed = services.GetRequiredService<seed>();
    seed.InsertDB();
}

app.UseCors("AllowSpecificOrigin");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();
app.UseSession();

// Add this middleware to modify the Content-Security-Policy header and remove X-Frame-Options
/*app.Use(async (context, next) =>
{
    // Remove X-Frame-Options header to avoid conflicts
    context.Response.Headers.Remove("X-Frame-Options");

    // Add Content-Security-Policy with frame-ancestors directive
    context.Response.Headers.Add("Content-Security-Policy", "frame-ancestors *;");

    await next();
});*/

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=College}/{action=Index}/{id?}");

app.Run();
