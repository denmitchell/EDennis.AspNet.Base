using IdentityModel;
using Microsoft.AspNetCore.Identity;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace EDennis.NetStandard.Base {
    public class DomainUserClaimsPrincipalFactory : IUserClaimsPrincipalFactory<DomainUser> {
        
        private readonly DomainIdentityDbContext _context;


        public DomainUserClaimsPrincipalFactory(DomainIdentityDbContext context) {
            _context = context;
        }

        public async Task<ClaimsPrincipal> CreateAsync(DomainUser user) {
            var principal = new ClaimsPrincipal();
            var identity = new ClaimsIdentity();

            identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id.ToString()));
            identity.AddClaim(new Claim(JwtClaimTypes.Name, user.UserName ?? user.Email ?? user.Id.ToString()));

            if (!string.IsNullOrWhiteSpace(user.Email)) {
                identity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
                identity.AddClaim(new Claim(JwtClaimTypes.Email, user.Email));
            }

            if (!string.IsNullOrWhiteSpace(user.PhoneNumber)) {
                identity.AddClaim(new Claim(JwtClaimTypes.PhoneNumber, user.PhoneNumber));
            }

            if (user.OrganizationId != default) {
                var organizationName = (await _context.Organizations.FirstOrDefaultAsync(o => o.Id == user.OrganizationId))?.Name;
                if (organizationName != null)
                    identity.AddClaim(new Claim("organization", organizationName));
            }

            var query =
                    from r in _context.Roles
                    join a in _context.Applications
                        on r.ApplicationId equals a.Id
                    join ur in _context.UserRoles
                            on r.Id equals ur.RoleId
                    where ur.UserId == user.Id
                    select new { ApplicationName = a.Name, RoleName = r.Name };

            var roles = await query.ToListAsync();

            foreach(var role in roles) {
                identity.AddClaim(new Claim(role.ApplicationName, role.RoleName));
            }

            principal.AddIdentity(identity);

            return principal;
        }
    }
}
