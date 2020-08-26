using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Implementation of IUserClaimsPrincipalFactory, which is used to generate a new
    /// ClaimsPrincipal.  The options for the factory allow including or excluding
    /// claims from various sources (user claims, roles as claims, role claims) and
    /// optionally filtering claims by application name.  
    /// 
    /// This version of the class assumes that the user class is DomainUser and the
    /// role class is DomainRole.
    /// </summary>
    public class DomainUserClaimsPrincipalFactory : DomainUserClaimsPrincipalFactory<DomainUser, DomainRole> {
        public DomainUserClaimsPrincipalFactory(DomainIdentityDbContext context, IAppClaimEncoder encoder,
            IOptionsMonitor<ClaimsPrincipalFactoryOptions> options, IHostEnvironment env)
            : base(context, encoder, options, env) {
        }
    }


    /// <summary>
    /// Implementation of IUserClaimsPrincipalFactory, which is used to generate a new
    /// ClaimsPrincipal.  The options for the factory allow including or excluding
    /// claims from various sources (user claims, roles as claims, role claims) and
    /// optionally filtering claims by application name.  
    /// 
    /// This version of the class allows specifying the user class and role class, 
    /// which must extend DomainUser and DomainRole, respectively.
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    /// <typeparam name="TRole"></typeparam>
    public class DomainUserClaimsPrincipalFactory<TUser, TRole> : IUserClaimsPrincipalFactory<TUser>
        where TUser : DomainUser
        where TRole : DomainRole {

        private readonly DomainIdentityDbContext _dbContext;
        private readonly IAppClaimEncoder _encoder;
        private readonly ClaimsPrincipalFactoryOptions _options;
        private readonly IHostEnvironment _env;

        public DomainUserClaimsPrincipalFactory(DomainIdentityDbContext dbContext,
            IAppClaimEncoder encoder, IOptionsMonitor<ClaimsPrincipalFactoryOptions> options,
            IHostEnvironment env) {
            _dbContext = dbContext;
            _encoder = encoder;
            _options = options.CurrentValue;
            _env = env;
        }

        public async Task<ClaimsPrincipal> CreateAsync(TUser user) {
            var principal = new ClaimsPrincipal();
            var identity = new ClaimsIdentity();

            //Add user properties as claims
            identity.AddClaims(user.ToClaims());

            identity.AddClaim(new Claim(JwtClaimTypes.Subject, user.Id.ToString()));
            identity.AddClaim(new Claim(JwtClaimTypes.Name, user.UserName));


            //Add user claims as claims
            if (_options.IncludeUserClaims) {
                var userClaims = (await _dbContext.Set<IdentityUserClaim<int>>()
                    .Where(uc => uc.UserId == user.Id)
                    .ToListAsync())
                    .Select(uc => new Claim(uc.ClaimType, uc.ClaimValue));
                //filter by application
                if (_options.FilterUserClaimsByCurrentApplicationName) {
                    userClaims = userClaims
                        .Select(uc => _encoder.Decode(new Claim(uc.Type, uc.Value)))
                        .Where(uc => uc.Application == _env.ApplicationName)
                        .Select(uc => new Claim(uc.ClaimType, uc.ClaimValue));
                }
                identity.AddClaims(userClaims);
            }

            //add roles and/or role claims
            if (_options.IncludeRoles || _options.IncludeRoleClaims ) {
                var query =
                        from r in _dbContext.Set<TRole>()
                        join ur in _dbContext.UserRoles
                                on r.Id equals ur.RoleId
                        where ur.UserId == user.Id
                        select new { RoleId = r.Id, AppClaim = new AppClaim { Application = r.Application, ClaimType = JwtClaimTypes.Role, ClaimValue = r.Name } };

                if (_options.FilterRolesByCurrentApplicationName)
                    query = query.Where(q => q.AppClaim.Application == _env.ApplicationName);

                var appClaims = await query.ToListAsync();

                //add roles as claims
                if (_options.IncludeRoles) {
                    foreach (var appClaim in appClaims)
                        identity.AddClaim(_encoder.Encode(appClaim.AppClaim));
                }

                //add role claims as claims
                if (_options.IncludeRoleClaims) {
                    var roleIds = appClaims.Select(x => x.RoleId);
                    var rQuery = _dbContext.Set<IdentityRoleClaim<int>>()
                            .Where(rc => roleIds.Contains(rc.RoleId));
                    var roleClaims = await rQuery.ToListAsync();

                    var roleClaimsView = from rc in roleClaims
                                         join ac in appClaims
                                            on rc.RoleId equals ac.RoleId
                                         select new AppClaim {
                                             Application = ac.AppClaim.Application,
                                             ClaimType = rc.ClaimType,
                                             ClaimValue = rc.ClaimValue
                                         };

                    foreach (var roleClaim in roleClaimsView)
                        identity.AddClaim(_encoder.Encode(roleClaim));
                }

            }

            //add the identity to the claims principal
            principal.AddIdentity(identity);

            //return the claims principal
            return principal;
        }
    }
}
