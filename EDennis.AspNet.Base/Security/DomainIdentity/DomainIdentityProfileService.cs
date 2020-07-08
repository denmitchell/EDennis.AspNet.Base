using EDennis.AspNet.Base.Security;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Security {

    /// <summary>
    /// Gets all roles prefixed by client_id, as well as
    /// all associated role claims and requested user claims.
    /// </summary>
    public class DomainIdentityProfileService<TUser,TRole> : IProfileService 
        where TUser: DomainUser, new()
        where TRole: DomainRole{

        private readonly DomainUserManager<TUser> _domainUserManager;
        private readonly DomainRoleManager<TRole> _domainRoleManager;
        private readonly ILogger<DomainIdentityProfileService<TUser, TRole>> _logger;

        public DomainIdentityProfileService(DomainUserManager<TUser> domainUserManager,
            DomainRoleManager<TRole> domainRoleManager, ILogger<DomainIdentityProfileService<TUser,TRole>> logger) {
            _domainUserManager = domainUserManager;
            _domainRoleManager = domainRoleManager;
            _logger = logger;
        }


        public async Task GetProfileDataAsync(ProfileDataRequestContext context) {

            //table-valued parameter @RequestedResourceApiScopes
            var requestedResourceApiScopes =
                context.RequestedResources.Resources.ApiResources.Select(a => a.Name);

            //table-valued parameter @RequestedUserClaimTypes
            var requestedUserClaimTypes = context.RequestedClaimTypes;

            //scalar-valued parameter @UserId
            var userId = context.Subject.GetSubjectId();

            //scalar-valued parameter @ClientId
            var clientId = context.Client.ClientId;



            var apps =
                context.RequestedResources.Resources.ApiResources
                .Where(a => context.Client.AllowedScopes.Any(c => a.Scopes.Contains(c)))
                .Select(a=>a.Properties.FirstOrDefault(a=>a.Key=="ApplicationName").Value
                  ?? a.Name)
                .ToArray();

            var user = await _domainUserManager.GetUserAsync(context.Subject);
            var userClaims = await _domainUserManager.GetClaimsAsync(user);
            var roles = await _domainUserManager.GetRolesAsync<TRole>(user, apps);
            var roleClaims = await _domainRoleManager.GetClaimsAsync(roles);

            context.IssuedClaims.AddRange(roles.Select(r=>new Claim(ClaimTypes.Role,r.RoleName)));
            context.IssuedClaims.AddRange(roleClaims);

            var requestedClaims = userClaims.Where(x => context.RequestedClaimTypes.Contains(x.Type,StringComparer.OrdinalIgnoreCase)).ToList();
                
            context.IssuedClaims.AddRange(requestedClaims);

            return;

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

            await IsActiveAsync(context, sub);
        }

        /// <summary>
        /// Determines if the subject is active.
        /// from https://github.com/IdentityServer/IdentityServer4/blob/main/src/AspNetIdentity/src/ProfileService.cs
        /// </summary>
        /// <param name="context"></param>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        protected virtual async Task IsActiveAsync(IsActiveContext context, string subjectId) {
            var user = await FindUserAsync(subjectId);
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
        protected virtual async Task IsActiveAsync(IsActiveContext context, TUser user) {
            context.IsActive = await IsUserActiveAsync(user);
        }

        /// <summary>
        /// Returns if the user is active.
        /// from https://github.com/IdentityServer/IdentityServer4/blob/main/src/AspNetIdentity/src/ProfileService.cs
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual Task<bool> IsUserActiveAsync(TUser user) {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Loads the user by the subject id.
        /// from https://github.com/IdentityServer/IdentityServer4/blob/main/src/AspNetIdentity/src/ProfileService.cs
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        protected virtual async Task<TUser> FindUserAsync(string subjectId) {
            var user = await _domainUserManager.FindByIdAsync(subjectId);
            if (user == null) {
                _logger?.LogWarning("No user found matching subject Id: {subjectId}", subjectId);
            }

            return user;
        }
    }
}
