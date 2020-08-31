using EDennis.NetStandard.Base;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EDennis.AspNetIdentityServer {

    /// <summary>
    /// Gets all app roles associated with the ClientId, as well as 
    /// any requested UserClaims.
    /// 
    /// NOTE: this implementation requires each Client to store each
    /// relevant application name as a record in ClientProperties,
    /// where the Key column holds the application name.  By convention, 
    /// the Value column should be "ResourceApplication" (or 
    /// something else that conveys the meaning of the record); 
    /// however, there are no real constraints on the Value column.
    /// 
    /// NOTE: when configuring an ApiResource, add UserClaims -- all
    /// user claims that should be added to access token.  These
    /// claims are stored as entries in the ApiResourceClaims table.
    /// </summary>
    public class DomainIdentityProfileService : IProfileService {

        private readonly DomainIdentityDbContext _dbContext;
        private readonly ILogger<DomainIdentityProfileService> _logger;

        public DomainIdentityProfileService(DomainIdentityDbContext dbContext,
            ILogger<DomainIdentityProfileService> logger) {
            _dbContext = dbContext;
            _logger = logger;
        }


        public async Task GetProfileDataAsync(ProfileDataRequestContext context) {

            var userId = int.Parse(context.Subject.GetSubjectId());
            var clientId = context.Client.ClientId;

            //retrieve client Application Names stored as property keys 
            var scopes = context.Client.Properties.Keys;

            //retrieve all user claims from database
            var userClaims = await _dbContext.UserClaims
                                .Where(uc => uc.UserId == userId)
                                .Select(uc => new Claim(uc.ClaimType, uc.ClaimValue))
                                .ToListAsync();

            //limit the list of claims to those that are either
            //  (a) requested claims, as configured in ApiResourceClaims OR
            //  (b) app role claims for applications registered in ClientProperties
            userClaims = userClaims
                .Where(uc => context.RequestedClaimTypes.Any(rct => rct == uc.Type)
                    || (uc.Type.StartsWith("app:") && scopes.Any(s => uc.Value.StartsWith($"{s}:"))))
                .ToList();

            //updated the IssuedClaims property
            context.IssuedClaims.AddRange(userClaims);
        }



        /// <summary>
        /// This method gets called whenever identity server needs to determine if the user is valid or active (e.g. if the user's account has been deactivated since they logged in).
        /// (e.g. during token issuance or validation).
        /// from https://github.com/IdentityServer/IdentityServer4/blob/main/src/AspNetIdentity/src/ProfileService.cs
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public virtual async Task IsActiveAsync(IsActiveContext context) {
            var sub = context.Subject?.GetSubjectId();
            if (sub == null) throw new Exception("No subject Id claim present");

            var guid = Guid.Parse(sub);
            await IsActiveAsync(context, guid);
        }


        /// <summary>
        /// Determines if the subject is active.
        /// from https://github.com/IdentityServer/IdentityServer4/blob/main/src/AspNetIdentity/src/ProfileService.cs
        /// </summary>
        /// <param name="context"></param>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        protected virtual async Task IsActiveAsync(IsActiveContext context, Guid userId) {
            var user = await FindUserAsync(userId);
            if (user != null) {
                await IsActiveAsync(context, user);
            } else {
                context.IsActive = false;
            }
        }

        /// <summary>
        /// Determines if the user is active.
        /// from https://github.com/IdentityServer/IdentityServer4/blob/main/src/AspNetIdentity/src/ProfileService.cs
        /// </summary>
        /// <param name="context"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        protected virtual async Task IsActiveAsync(IsActiveContext context, DomainUser user) {
            context.IsActive = await IsUserActiveAsync(user);
        }

        /// <summary>
        /// Returns if the user is active.
        /// from https://github.com/IdentityServer/IdentityServer4/blob/main/src/AspNetIdentity/src/ProfileService.cs
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual Task<bool> IsUserActiveAsync(DomainUser user) {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Loads the user by the subject id.
        /// from https://github.com/IdentityServer/IdentityServer4/blob/main/src/AspNetIdentity/src/ProfileService.cs
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        protected virtual async Task<DomainUser> FindUserAsync(Guid userId) {
            var user = await _dbContext.Set<DomainUser>().FindAsync(userId);
            if (user == null) {
                _logger?.LogWarning("No user found matching subject Id: {subjectId}", userId);
            }

            return user;
        }
    }
}
