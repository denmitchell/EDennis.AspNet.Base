using IdentityModel;
using Microsoft.AspNetCore.Identity;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;

namespace EDennis.NetStandard.Base {

    public class DomainUserClaimsPrincipalFactory : IUserClaimsPrincipalFactory<DomainUser> {
        
        private readonly DomainIdentityDbContext _context;
        private readonly IAppClaimComposer _composer;
        private readonly ClaimsPrincipalFactoryOptions _options;
        private readonly IHostEnvironment _env;

        public DomainUserClaimsPrincipalFactory(DomainIdentityDbContext context, 
            IAppClaimComposer composer, IOptionsMonitor<ClaimsPrincipalFactoryOptions> options,
            IHostEnvironment env) {
            _context = context;
            _composer = composer;
            _options = options.CurrentValue;
            _env = env;
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
                    from r in _context.Set<DomainRole>()
                    join a in _context.Set<DomainApplication>()
                        on r.ApplicationId equals a.Id
                    join ur in _context.UserRoles
                            on r.Id equals ur.RoleId
                    where ur.UserId == user.Id
                    select new { RoleId = r.Id, AppClaim = new AppClaim { ApplicationName = a.Name, ClaimType = JwtClaimTypes.Role, ClaimValue = r.Name } };

            if (_options.FilterByCurrentApplicationName)
                query = query.Where(q => q.AppClaim.ApplicationName == _env.ApplicationName);

            var appClaims = await query.ToListAsync();

            foreach(var appClaim in appClaims)
                identity.AddClaim(_composer.Compose(appClaim.AppClaim));
            

            if (_options.IncludeRoleClaims) {
                var roleIds = appClaims.Select(x => x.RoleId);
                var rQuery = _context.Set<DomainRoleClaim>()
                        .Where(rc => roleIds.Contains(rc.RoleId));
                var roleClaims = await rQuery.ToListAsync();

                var roleClaimsView = from rc in roleClaims
                                     join ac in appClaims
                                        on rc.RoleId equals ac.RoleId
                                     select new AppClaim { ApplicationName = ac.AppClaim.ApplicationName,
                                         ClaimType = rc.ClaimType,
                                         ClaimValue = rc.ClaimValue };

                foreach (var roleClaim in roleClaimsView)
                    identity.AddClaim(_composer.Compose(roleClaim));
            }


            principal.AddIdentity(identity);

            return principal;
        }
    }
}
