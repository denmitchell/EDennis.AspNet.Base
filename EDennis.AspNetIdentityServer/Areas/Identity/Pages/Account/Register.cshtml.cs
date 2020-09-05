using EDennis.NetStandard.Base;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace EDennis.AspNetIdentityServer.Areas.Identity.Pages.Account {
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<DomainUser> _signInManager;
        private readonly UserManager<DomainUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly DomainIdentityDbContext _dbContext;
        private readonly CentralAdmin _centralAdmin;

        public RegisterModel(
            DomainIdentityDbContext dbContext,
            UserManager<DomainUser> userManager,
            SignInManager<DomainUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            CentralAdmin centralAdmin)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _centralAdmin = centralAdmin;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [BindProperty]
        public IEnumerable<SelectListItem> Organizations { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }


            [Required]
            [Display(Name = "Organization")]
            public string Organization { get; set; }


        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            Organizations = _dbContext.Organizations.Select(o => new SelectListItem { Value = o.Name, Text = o.Name });
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new DomainUser { UserName = Input.Email, Email = Input.Email };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");


                    //handle email to organization admin to confirm registration

                    var userMgtUrl = Url.Page(
                        "/Account/Admin/User",
                        pageHandler: null,
                        values: new { area = "Identity", id = user.Id},
                        protocol: Request.Scheme);

                    //first, try to get shared mailbox for organization
                    var admins = await _dbContext.Organizations.Where(o =>
                        o.Name == Input.Organization && o.SharedEmail != null)
                        .Select(u => u.SharedEmail)
                        .ToListAsync();

                    //failing that, try to get email addresses for relevant organization admins
                    if (admins.Count() == 0)
                        admins = await _dbContext.Users.Where(u =>
                        u.Organization == Input.Organization
                            && u.OrganizationAdmin && u.Email != Input.Email)
                        .Select(u=>u.Email)
                        .ToListAsync();

                    //failing that, try to get email addresses for the super admins
                    if(admins.Count() == 0)
                        admins = await _dbContext.Users.Where(u => u.SuperAdmin)
                            .Select(u=>u.Email)
                            .ToListAsync();

                    //failing that, get the central admin email
                    if (admins.Count() == 0)
                        admins = new List<string> { _centralAdmin.Email };

                    //build email list
                    var to = new string[] { admins.First() };
                    var cc = admins.Except(to).First().Select(e=>e + "*");
                    var addrs = string.Join(';', to.Union(cc));

                    await _emailSender.SendEmailAsync(addrs, "Registering New Organization Member",
                        $"{Input.Email} has registered with us using organization = {Input.Organization}.  Please <a href='{HtmlEncoder.Default.Encode(userMgtUrl)}'>confirm his/her organization.</a>.");


                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
