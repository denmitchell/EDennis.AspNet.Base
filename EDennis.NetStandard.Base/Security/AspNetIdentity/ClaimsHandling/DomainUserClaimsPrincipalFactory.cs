using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Implementation of IUserClaimsPrincipalFactory, which is used 
    /// to generate a new ClaimsPrincipal.  The options for the factory 
    /// allow filtering claims by the current application's name.  
    /// </summary>
    public class DomainUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<DomainUser> {

        private readonly ClaimsPrincipalFactoryOptions _options;
        private readonly IHostEnvironment _env;

        public const string AUTHENTICATION_TYPE = "DomainIdentity";

        public DomainUserClaimsPrincipalFactory(
            IOptionsMonitor<ClaimsPrincipalFactoryOptions> options,
            IOptions<IdentityOptions> optionsAccessor,
            IHostEnvironment env,
            UserManager<DomainUser> userManager) :base (userManager,optionsAccessor){
            _options = options.CurrentValue;
            _env = env;
        }

        public override async Task<ClaimsPrincipal> CreateAsync(DomainUser user) {

            //Use base implementation, which gets the name, nameidentifier, and user claims.
            //This also ensures that the default SignInManager can sign-in the user.
            var principal = await base.CreateAsync(user);

            var identity = (ClaimsIdentity)principal.Identity;
            var claimTypes = identity.Claims.Select(c => c.Type);
            
            //ADD USER PROPERTIES AS CLAIMS
            identity.AddClaims(user.ToClaims().Where(c=>!claimTypes.Contains(c.Type)));
            
            //if configured, filter out any claim ...
            //   - whose Type starts with app: AND 
            //   - whose Value doesn't start with _env.ApplicationName
            if (_options.FilterClaimsByCurrentApplicationName) {
                var claimsToRemove = identity.Claims
                    .Where(uc => !(uc.Type.StartsWith("app:")
                        && !uc.Value.StartsWith($"{_env.ApplicationName}:")))
                    .ToArray();

                for (int i = 0; i < claimsToRemove.Count(); i++) {
                    identity.RemoveClaim(claimsToRemove[i]);
                }
            }

            //return the claims principal
            return principal;
        }
    }
}
