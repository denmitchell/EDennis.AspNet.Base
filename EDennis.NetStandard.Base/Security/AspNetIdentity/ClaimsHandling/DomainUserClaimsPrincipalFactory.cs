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

    //TODO: Check impact of Role.Nomen
    public class DomainUserClaimsPrincipalFactory : DomainUserClaimsPrincipalFactory<DomainUser, DomainRole> {
        public DomainUserClaimsPrincipalFactory(DomainIdentityDbContext context, IAppClaimEncoder encoder, 
            IOptionsMonitor<ClaimsPrincipalFactoryOptions> options, IHostEnvironment env) 
            : base(context, encoder, options, env) {
        }
    }


    /// <summary>
    /// TODO: Determine if materialized views can make this more efficient
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    /// <typeparam name="TRole"></typeparam>
    public class DomainUserClaimsPrincipalFactory<TUser,TRole> : IUserClaimsPrincipalFactory<TUser> 
        where TUser: DomainUser
        where TRole: DomainRole {
        
        private readonly DomainIdentityDbContext _context;
        private readonly IAppClaimEncoder _encoder;
        private readonly ClaimsPrincipalFactoryOptions _options;
        private readonly IHostEnvironment _env;

        public DomainUserClaimsPrincipalFactory(DomainIdentityDbContext context, 
            IAppClaimEncoder encoder, IOptionsMonitor<ClaimsPrincipalFactoryOptions> options,
            IHostEnvironment env) {
            _context = context;
            _encoder = encoder;
            _options = options.CurrentValue;
            _env = env;
        }

        public async Task<ClaimsPrincipal> CreateAsync(TUser user) {
            var principal = new ClaimsPrincipal();
            var identity = new ClaimsIdentity();

            var userClaims = user.ToClaims();
            foreach (var claim in userClaims)
                identity.AddClaim(claim);

            var query =
                    from r in _context.Set<TRole>()
                    join ur in _context.UserRoles
                            on r.Id equals ur.RoleId
                    where ur.UserId == user.Id
                    select new { RoleId = r.Id, AppClaim = new AppClaim { Application = r.Application, ClaimType = JwtClaimTypes.Role, ClaimValue = r.Nomen } };

            if (_options.FilterByCurrentApplicationName)
                query = query.Where(q => q.AppClaim.Application == _env.ApplicationName);

            var appClaims = await query.ToListAsync();

            foreach(var appClaim in appClaims)
                identity.AddClaim(_encoder.Encode(appClaim.AppClaim));
            

            if (_options.IncludeRoleClaims) {
                var roleIds = appClaims.Select(x => x.RoleId);
                var rQuery = _context.Set<IdentityRoleClaim<int>>()
                        .Where(rc => roleIds.Contains(rc.RoleId));
                var roleClaims = await rQuery.ToListAsync();

                var roleClaimsView = from rc in roleClaims
                                     join ac in appClaims
                                        on rc.RoleId equals ac.RoleId
                                     select new AppClaim { Application = ac.AppClaim.Application,
                                         ClaimType = rc.ClaimType,
                                         ClaimValue = rc.ClaimValue };

                foreach (var roleClaim in roleClaimsView)
                    identity.AddClaim(_encoder.Encode(roleClaim));
            }


            principal.AddIdentity(identity);

            return principal;
        }
    }
}
