using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Security {

    /// <summary>
    /// Gets all roles prefixed by client_id, as well as
    /// all associated role claims and requested user claims.
    /// 
    /// NOTE: this implementation depends upon maintenance of a 
    /// UserClientClaims table, which associates a User.ID (GUID) and
    /// ClientId (varchar) with a set of claims (JSON column).
    /// After updating ApiResources, ApiClaims, ClientScopes,
    /// AspNetUser, AspNetUserRoles, AspNetUserClaims, or AspNetRoles
    /// execute stored procedure di.UpdateUserClientClaims
    /// 
    /// NOTE: when configuring an ApiResource, add UserClaims -- all
    /// user claims that should be added to access token
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

            var userId = Guid.Parse(context.Subject.GetSubjectId());
            var clientId = context.Client.ClientId;


            //add roles
            var roles = await _dbContext.UserClientApplicationRoles
                .Where(x => x.UserId == userId && x.ClientId == clientId)
                .ToListAsync();

            context.IssuedClaims.AddRange(roles.Select(r => new Claim(r.ApplicationName, r.RoleName)));


            //add requested user claims
            var userClaims = await _dbContext.UserClaims
                                .Where(uc => uc.UserId == userId
                                    && context.RequestedClaimTypes.Any(rct => rct == uc.ClaimType))
                                .Select(uc=>new Claim(uc.ClaimType, uc.ClaimValue))
                                .ToListAsync();

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
