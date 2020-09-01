using EDennis.NetStandard.Base;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace EDennis.AspNetIdentityServer.Areas.Identity.Pages.Account.Admin {
    public partial class UserModel : PageModel {

        private readonly DomainIdentityDbContext _dbContext;

        public UserModel(DomainIdentityDbContext dbContext) {
            _dbContext = dbContext;
        }


        [TempData]
        public string StatusMessage { get; set; }

        /*
            Note: All boolean properties bound to disable-able checkboxes 
                  use nullable booleans in order to differentiate between
                  submitted false values and unsubmitted default values 
                  (which would also be false if using non-nullable booleans).  
                  Disabled checkboxes don't send their values during a form 
                  submit.
        */

        [BindProperty] public int Id { get; set; }
        [BindProperty] public string UserName { get; set; }
        [BindProperty] public string Email { get; set; }
        [BindProperty] public bool EmailConfirmed { get; set; } 
        [BindProperty] public string PhoneNumber { get; set; }
        [BindProperty] public bool PhoneNumberConfirmed { get; set; }
        [BindProperty] public string Organization { get; set; }
        [BindProperty] public bool OrganizationConfirmed { get; set; }
        [BindProperty] public bool OrganizationAdmin { get; set; }

        [DataType(DataType.DateTime)]
        [BindProperty] public DateTimeOffset? LockoutBegin { get; set; }
        [DataType(DataType.DateTime)]
        [BindProperty] public DateTimeOffset? LockoutEnd { get; set; }




        //claim type = "app:role"
        [BindProperty] public Dictionary<string, bool> AppRoleClaims { get; set; }



        //used to differentiate between unsubmitted form elements (e.g., having
        //disabled controls) and purposely submitted default values (null or false)

        private bool emailSubmitted = false;
        private bool phoneNumberSubmitted = false;
        private bool organizationSubmitted = false;

        private bool lockoutBeginSubmitted = false;
        private bool lockoutEndSubmitted = false;

        private readonly Dictionary<string, bool> submittedAppRoleClaims
            = new Dictionary<string, bool>();



        [BindProperty] public string[] OrgApps { get; set; }

        public string[] AppRoles(string appName) {
            return AppRoleClaims.Keys
                .Where(k => k.StartsWith($"{appName}:"))
                .Select(k => k.Substring($"{appName}:".Length))
                .ToArray();
        }


        //lookups
        [BindProperty] public IEnumerable<SelectListItem> Organizations { get; set; }


        [BindProperty] public bool IsSuperAdmin { get; set; }
        [BindProperty] public bool IsOrgAdmin { get; set; }
        [BindProperty] public bool IsSelf { get; set; }

        private string[] appAdminFor;
        private (string App, string Role)[] orgAdminable;

        public bool IsAppAdminFor(string appName) {
            return appAdminFor.Any(a => a == appName);
        }

        public bool OrgAdminable(string appName, string roleName) {
            return orgAdminable.Any(a => a.App == appName && a.Role == roleName);
        }

        public bool IsAdmin() => IsSuperAdmin || IsOrgAdmin || appAdminFor.Length > 0;


        private async Task LoadLookupsAsync() {
            Organizations = await _dbContext.Organizations
                .Select(x => 
                    new SelectListItem { 
                        Value = x.Name, 
                        Text = x.Name, 
                        Selected = x.Name == Organization })
                    .ToArrayAsync();
        }

        private async Task LoadAdminUserDataAsync() {


            IsSuperAdmin = User.HasClaim(c => c.Type == DomainClaimTypes.SuperAdmin);
            IsOrgAdmin = User.HasClaim(c=>c.Type == DomainClaimTypes.OrganizationAdminFor && c.Value == Organization);
            IsOrgAdmin = User.HasClaim(c => c.Type == DomainClaimTypes.OrganizationAdminFor && c.Value == Organization);


            OrgApps = await _dbContext.OrganizationApplications
                        .Where(x => x.Organization == Organization)
                        .Select(x => x.Application)
                        .ToArrayAsync();

            //get applications where the current admin user is an app admin
            //  (which applies across all organizations) 
            appAdminFor =
                User.Claims
                .Where(c => c.Type == DomainClaimTypes.ApplicationRole && c.Value.EndsWith(":admin"))
                .Select(c => c.Value.Split(":")[0])
                .ToArray();


            //if current admin user is an org admin for the target user's organization...
            if (IsOrgAdmin) {
                //add to Apps
                appAdminFor = appAdminFor.Union(OrgApps).ToArray();
            }

        }

        private async Task LoadTargetUserDataAsync() {

            //now, get the target user claims
            var userClaims = await _dbContext.UserClaims
                .Where(uc => uc.UserId == Id)
                .ToListAsync();

            // get all app role claims for relevant Apps 
            var appRoleClaims = await _dbContext.ApplicationClaims
                .Where(ac => OrgApps.Contains(ac.Application) && ac.ClaimType == DomainClaimTypes.ApplicationRole)
                .ToListAsync();

            AppRoleClaims =  appRoleClaims.ToDictionary(ac => $"{ac.Application}:{ac.ClaimValue}", ac => false);

            //store a tuple of appRoleClaims that are OrgAdminable
            orgAdminable = appRoleClaims
                .Where(ac => ac.OrgAdminable )
                .Select(ac => (ac.Application,ac.ClaimValue))
                .ToArray();

            //update the app role claims to true when the target user has that claim
            var appRoleClaimKeys = AppRoleClaims.Keys.ToArray();
            for (int i = 0; i < appRoleClaimKeys.Length; i++) {
                if (userClaims.Any(uc => uc.ClaimType == DomainClaimTypes.ApplicationRole && uc.ClaimValue == appRoleClaimKeys[i]))
                    AppRoleClaims[appRoleClaimKeys[i]] = true;
            }


        }


        public async Task<IActionResult> OnGetAsync(int id) {

            //on get, the bound columns need to be populated with current data for target user

            var user = await _dbContext.Users.FindAsync(id);
            
            if (user == null) {
                return NotFound($"Unable to load user with Id '{Id}'.");
            }

            Id = id;
            UserName = user.UserName;
            Email = user.Email;
            EmailConfirmed = user.EmailConfirmed;
            PhoneNumber = user.PhoneNumber;
            PhoneNumberConfirmed = user.PhoneNumberConfirmed;
            Organization = user.Organization;
            OrganizationConfirmed = user.OrganizationConfirmed;
            OrganizationAdmin = user.OrganizationAdmin;
            LockoutBegin = user.LockoutBegin;
            LockoutEnd = user.LockoutEnd;

            IsSelf = User.Claims.Any(c =>
                (c.Type == ClaimTypes.Name && c.Value == UserName) ||
                (c.Type == JwtClaimTypes.Name && c.Value == UserName) ||
                (c.Type == ClaimTypes.Email && c.Value == Email) ||
                (c.Type == JwtClaimTypes.Email && c.Value == Email)
                );

            await LoadAdminUserDataAsync();
            await LoadTargetUserDataAsync();
            await LoadLookupsAsync();

            return Page();
        }


        public async Task<IActionResult> OnPostAsync() {

            await LoadAdminUserDataAsync();

            ProcessFormData();

            var user = await _dbContext.Users
                .FindAsync(Id);

            if (user == null) {
                return NotFound($"Unable to load user with Id '{Id}'.");
            }

            var userClaims = await _dbContext.UserClaims
                .Where(uc => uc.UserId == Id)
                .ToListAsync();

            //save current user and user claims data to history.

            var userHistory = new DomainUserHistory {
                Id = user.Id,
                DateReplaced = DateTime.Now,
                ReplacedBy = User.Identity.Name,
                UserName = user.UserName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                Organization = user.Organization,
                OrganizationConfirmed = user.OrganizationConfirmed,
                OrganizationAdmin = user.OrganizationAdmin,
                LockoutBegin = user.LockoutBegin,
                LockoutEnd = user.LockoutEnd,
                UserClaims = JsonSerializer.Serialize(userClaims.Select(x=>$"({x.ClaimType},{x.ClaimValue})"))
            };

            _dbContext.UserHistories.Add(userHistory);
            await _dbContext.SaveChangesAsync();


            //update the user
            user.Email = emailSubmitted ? Email : user.Email;
            user.EmailConfirmed = EmailConfirmed;
            user.PhoneNumber = phoneNumberSubmitted ? PhoneNumber : user.PhoneNumber;
            user.PhoneNumberConfirmed = PhoneNumberConfirmed;
            user.Organization = organizationSubmitted ? Organization : user.Organization;
            user.OrganizationConfirmed = OrganizationConfirmed;
            user.OrganizationAdmin = OrganizationAdmin;
            user.LockoutBegin = lockoutBeginSubmitted ? LockoutBegin : user.LockoutBegin;
            user.LockoutEnd = lockoutEndSubmitted ? LockoutEnd : user.LockoutEnd;

            _dbContext.Update(user);
            await _dbContext.SaveChangesAsync();

            var appRoleClaimsKeys = submittedAppRoleClaims.Keys.ToArray();
            for (int i = 0; i < appRoleClaimsKeys.Length; i++) {
                var claimValue = appRoleClaimsKeys[i];
                var shouldHaveClaim = submittedAppRoleClaims[claimValue];
                var matchingUserClaim = userClaims.FirstOrDefault(uc => uc.ClaimType == DomainClaimTypes.ApplicationRole && uc.ClaimValue == claimValue);
                if (shouldHaveClaim && matchingUserClaim == null)
                    _dbContext.UserClaims.Add(new IdentityUserClaim<int> { UserId = Id, ClaimType = DomainClaimTypes.ApplicationRole, ClaimValue = claimValue });
                else if (!shouldHaveClaim && matchingUserClaim != null)
                    _dbContext.UserClaims.Remove(matchingUserClaim);
            }
            await _dbContext.SaveChangesAsync();

            return RedirectToPage();
        }

        /// <summary>
        /// Submitted form data is processes explicitly in order to differentiate between unsubmitted
        /// form data (which happens when controls are disabled) and purposely submitted default
        /// values (e.g., false values for checkboxes and null values for lockout dates)
        /// </summary>
        /// <returns></returns>
        private void ProcessFormData() {

            phoneNumberSubmitted = HttpContext.Request.Form.ContainsKey("PhoneNumber");
            emailSubmitted = HttpContext.Request.Form.ContainsKey("Email");
            organizationSubmitted = HttpContext.Request.Form.ContainsKey("Organization");
            lockoutBeginSubmitted = HttpContext.Request.Form.ContainsKey("LockoutBegin");
            lockoutEndSubmitted = HttpContext.Request.Form.ContainsKey("LockoutEnd");

            foreach (var entry in HttpContext.Request.Form) {
                if (entry.Key.StartsWith("AppRoleClaims[")) {
                    var propKey = entry.Key.Substring(entry.Key.IndexOf('[') + 1, entry.Key.IndexOf(']') - entry.Key.IndexOf('[') - 1);
                    var propValue = entry.Value.Contains("true", StringComparer.OrdinalIgnoreCase);
                    submittedAppRoleClaims.Add(propKey, propValue);
                }
            }
        }

    }
}
