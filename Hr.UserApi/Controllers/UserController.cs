using Hr.UserApi.Models;
using EDennis.AspNet.Base;
using EDennis.AspNetIdentityServer.Data;
using EDennis.AspNetIdentityServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;

namespace Hr.UserApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class UserController  {

        private readonly UserManager<AspNetIdentityUser> _userManager;
        private readonly RoleManager<IdentityUser> _roleManager;

        public UserController(UserManager<AspNetIdentityUser> userManager, RoleManager<IdentityUser> roleManager,
            AspNetIdentityDbContext dbContext
            ) {
            _userManager = userManager;
            _roleManager = roleManager;

            if (!initialized) {
                initialized = true;
                loginProviders = dbContext.UserLogins.Select(x => x.LoginProvider)?.ToArray();
            }
        }

        public static string[] loginProviders = new string[] { };
        public static bool initialized = false;

        public static Regex idPattern = new Regex("[0-9a-f]{8}-([0-9a-f]{4}-){3}[0-9a-f]{12}");
        public static Regex lpPattern = new Regex("(?:\\(\\s*loginProvider\\s*=\\s*'?)([^,']+)(?:'?\\s*,\\s*providerKey\\s*=\\s*'?)([^)']+)(?:'?\\s*\\))");


        [HttpGet]
        public async Task<AspNetIdentityUser> FindAsync(string pathParameter) {
            if (pathParameter.Contains("@"))
                return await _userManager.FindByEmailAsync(pathParameter);
            else if (idPattern.IsMatch(pathParameter))
                return await _userManager.FindByIdAsync(pathParameter);
            else
                return await _userManager.FindByNameAsync(pathParameter);
        }


        [HttpPost]
        public async Task<IdentityResult> CreateAsync(string email, string password) {
            var user = new AspNetIdentityUser {
                Id = Guid.NewGuid().ToString(),
                UserName = email,
                NormalizedUserName = email.ToUpper(),
                Email = email,
                NormalizedEmail = email.ToUpper()
            };
            var hashed = _userManager.PasswordHasher.HashPassword(user, password);
            user.PasswordHash = hashed;

            return await _userManager.CreateAsync(user);
        }


        [HttpPut("roles")]
        public async Task<IdentityResult> EditRolesAsync(EditRolesModel editRolesModel) {
            var user = await _userManager.FindByEmailAsync(editRolesModel.Email);
            var currentRoles = await _userManager.GetRolesAsync(user);

            var result1 = await _userManager.RemoveFromRolesAsync(user, currentRoles.Except(editRolesModel.Roles));
            var result2 = await _userManager.AddToRolesAsync(user, editRolesModel.Roles.Except(currentRoles));

            if (!result1.Succeeded && !result2.Succeeded)
                return IdentityResult.Failed(result1.Errors.Union(result2.Errors).ToArray());
            else if (!result1.Succeeded)
                return result1;
            else
                return result2;

        }



    }
}
