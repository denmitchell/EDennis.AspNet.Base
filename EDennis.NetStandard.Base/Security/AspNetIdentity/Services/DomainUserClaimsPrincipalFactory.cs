using IdentityModel;
using Microsoft.AspNetCore.Identity;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EDennis.NetStandard.Base {

    public class DomainUserClaimsPrincipalFactory : IUserClaimsPrincipalFactory<DomainUser> {
        
        private readonly DomainIdentityDbContext _context;
        private readonly IAppClaimComposer _composer;

        public DomainUserClaimsPrincipalFactory(DomainIdentityDbContext context, 
            IAppClaimComposer composer) {
            _context = context;
            _composer = composer;
        }

        public async Task<ClaimsPrincipal> CreateAsync(DomainUser user) {
            var principal = new ClaimsPrincipal();
            var identity = new ClaimsIdentity();

            var userClaims = user.ToClaims();
            foreach (var claim in userClaims)
                identity.AddClaim(claim);

            if (user.OrganizationId != default && !userClaims.Any(c=>c.Type =="organization")) {
                var organizationName = (await _context.Set<DomainOrganization>().FirstOrDefaultAsync(o => o.Id == user.OrganizationId))?.Name;
                if (organizationName != null)
                    identity.AddClaim(new Claim("organization", organizationName));
            }

            var query =
                    from r in _context.Roles
                    join a in _context.Set<DomainApplication>()
                        on r.ApplicationId equals a.Id
                    join ur in _context.UserRoles
                            on r.Id equals ur.RoleId
                    where ur.UserId == user.Id
                    select new AppClaim{ ApplicationName = a.Name, ClaimType = JwtClaimTypes.Role, ClaimValue = r.Name };

            var appClaims = await query.ToListAsync();

            foreach(var appClaim in appClaims) {
                identity.AddClaim(_composer.Compose(appClaim));
            }

            principal.AddIdentity(identity);

            return principal;
        }
    }
}
