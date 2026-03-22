using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolSelection.Data;

namespace SchoolSelection.ApplicationClass
{
    public class DataHelper
    {
        private readonly CollegeDbContext _Context;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHttpContextAccessor Accessor;
        private readonly IWebHostEnvironment Env;
        const string AllChars = "ABCDEFGHJKLMNPQRSTUVWXYZ123456789";
        const string NumberChars = "0123456789"; 
        const string Alphabets = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        private static readonly Random _random = new Random();

        public DataHelper(UserManager<ApplicationUser> _userManager, RoleManager<ApplicationRole> _roleManager,
            SignInManager<ApplicationUser> _signInManager, CollegeDbContext _Context,
            IHttpContextAccessor Accessor, IWebHostEnvironment Env)
        {
            this._Context = _Context;
            this._roleManager = _roleManager;
            this._userManager = _userManager;
            this._signInManager = _signInManager;
            this.Accessor = Accessor;
            this.Env = Env;
        }

        public ApplicationUser GetLoggedInUser()
        {
            var User = Accessor.HttpContext.User.Identity.Name;
            return _userManager.FindByNameAsync(User).Result;
        }

        public async Task<string>  GenerateDbToken()
        {
            var token = new string(Enumerable.Repeat(AllChars, 12).Select(n => n[_random.Next(n.Length)]).ToArray());
            if (string.IsNullOrEmpty(token))
            {
                await GenerateDbToken();
            }

            return token;
        }
        
        //Convert String t Capital Letter words
        public static string CapitalizeWords(string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (value.Length == 0)
                return value;
            StringBuilder result = new StringBuilder(value);
            result[0] = char.ToUpper(result[0]);
            for (int i = 1; i < result.Length; ++i)
            {
                if (char.IsWhiteSpace(result[i - 1]))
                    result[i] = char.ToUpper(result[i]);
            }

            return result.ToString();
        }
        
        public static int Generate()
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            return rand.Next(100000000, 999999999);
        }
    }
}