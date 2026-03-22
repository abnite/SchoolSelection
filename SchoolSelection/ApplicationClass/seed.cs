using System;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SchoolSelection.Data;
using SchoolSelection.Models;

namespace Project_Articles.ApplicationClass
{
    public class seed
    {
        private CollegeDbContext _dbContext;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public seed(CollegeDbContext _dbContext, RoleManager<ApplicationRole> _roleManager, UserManager<ApplicationUser> _userManager)
        {
            this._dbContext = _dbContext;
            this._roleManager = _roleManager;
            this._userManager = _userManager;
        }

        public void  InsertDB()
        {
            string[] Roles = new string[] { "Admin", "user" };

            foreach (var Role in Roles)
            {
                var roleStore = new RoleStore<IdentityRole>(_dbContext);
                if (!_dbContext.Roles.Any(r=>r.Name==Role))
                {
                    ApplicationRole applicationRole = new ApplicationRole();
                    applicationRole.Name = Role;
                    applicationRole.NormalizedName = Role.ToUpper();
                    IdentityResult identityResult = roleStore.CreateAsync(applicationRole).Result;
                }
            }

            if (!_dbContext.Users.Any(u => u.UserName == "admin@admin.com"))
            {
                ApplicationUser newUser = new ApplicationUser
                {
                    UserName = "admin@admin.com",
                    LastName = "Administrator",
                    FirstName = "Administrator",
                    Email = "abnite@gmail.com",
                    IsConfirmed = true
                };
                IdentityResult results = _userManager.CreateAsync(newUser, "P@$$Code1").Result;
                IdentityResult addRole = _userManager.AddToRoleAsync(newUser, "Admin").Result;

            }

            if (!_dbContext.SystemSettings.Any(s => s.Key == "EnableSubscriptions"))
            {
                SystemSetting newSystemSetting = new SystemSetting
                {
                    Id = Guid.NewGuid(),
                    Key = "EnableSubscriptions",
                    Value = "false",
                    UpdatedAt = DateTime.Now

                };
                _dbContext.SystemSettings.Add(newSystemSetting);
                _dbContext.SaveChanges();
            }

            if (!_dbContext.SubscriptionPlans.Any(u => u.Type == "Free"))
            {
                var freePlan = new SubscriptionPlan
                {
                    Id = Guid.NewGuid(),
                    Type = "Free",
                    Description = "Free",
                    Price = 0.00m,
                    MaxSelections = 1,
                    MaxColleges = 3,
                    MaxCriteria = 3,
                    MaxSubmissions = 2,
                    UpdatedAt = DateTime.UtcNow
                };
                _dbContext.SubscriptionPlans.Add(freePlan);
                _dbContext.SaveChanges();
            }

            
        }

    }
}