using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Implementation of IUserClaimsPrincipalFactory, which is used 
    /// to generate a new ClaimsPrincipal.  
    /// </summary>
    public class DomainUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<DomainUser> {

        public DomainUserClaimsPrincipalFactory(
            IOptions<IdentityOptions> optionsAccessor,
            UserManager<DomainUser> userManager) :base (userManager,optionsAccessor){
        }

        public override async Task<ClaimsPrincipal> CreateAsync(DomainUser user) {

            //Use base implementation, which gets the name, nameidentifier, and user claims.
            //This also ensures that the default SignInManager can sign-in the user.
            var principal = await base.CreateAsync(user);

            var identity = (ClaimsIdentity)principal.Identity;
            var claimTypes = identity.Claims.Select(c => c.Type);
            
            //ADD USER PROPERTIES AS CLAIMS
            identity.AddClaims(user.ToClaims().Where(c=>!claimTypes.Contains(c.Type)));
            
            //return the claims principal
            return principal;
        }
    }
}
