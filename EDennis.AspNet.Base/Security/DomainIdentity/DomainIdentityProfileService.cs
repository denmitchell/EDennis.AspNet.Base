using EDennis.AspNet.Base.Extensions;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore.Storage;

namespace EDennis.AspNet.Base.Security {

    /// <summary>
    /// Gets all roles prefixed by client_id, as well as
    /// all associated role claims and requested user claims.
    /// </summary>
    public class DomainIdentityProfileService<TUser,TRole,TContext> : IProfileService 
        where TUser: DomainUser, new()
        where TRole: DomainRole
        where TContext: DomainIdentityDbContext<TUser,TRole> {

        private readonly DomainIdentityDbContext<TUser, TRole> _dbContext;
        private readonly ILogger<DomainIdentityProfileService<TUser, TRole, TContext>> _logger;

        public DomainIdentityProfileService(DomainIdentityDbContext<TUser,TRole> dbContext,
            ILogger<DomainIdentityProfileService<TUser,TRole,TContext>> logger) {
            _dbContext = dbContext;
            _logger = logger;
        }


        public async Task GetProfileDataAsync(ProfileDataRequestContext context) {

            //table-valued parameter @RequestedResourceApiScopes
            var requestedResourceApiScopes =
                context.RequestedResources.Resources.ApiResources.Select(a => a.Name)
                .ToStringTableTypeParameter();

            //table-valued parameter @RequestedUserClaimTypes
            var requestedUserClaimTypes = context.RequestedClaimTypes
                .ToStringTableTypeParameter();

            //scalar-valued parameter @UserId
            var userId = context.Subject.GetSubjectId();

            //scalar-valued parameter @ClientId
            var clientId = context.Client.ClientId;

            var db = _dbContext.Database;

            if (!db.ProviderName.Contains("SqlServer"))
                throw new Exception("Cannot use DomainIdentityProfileService.GetProfileDataAsync without SqlServer provider");

            var cxn = db.GetDbConnection();

            var results = await cxn.QueryAsync<ClaimModel>("exec di.DomainIdentityProfileService_GetProfileData",
                param: new {
                    UserId = userId,
                    ClientId = clientId,
                    RequestedResourceApiScopes = requestedResourceApiScopes,
                    RequestedUserClaimTypes = requestedUserClaimTypes
                },
                transaction: db.CurrentTransaction?.GetDbTransaction()
                );

            context.IssuedClaims.AddRange(results.Select(c => new Claim(c.ClaimType, c.ClaimValue)));

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
        protected virtual async Task<TUser> FindUserAsync(Guid userId) {
            var user = await _dbContext.Set<TUser>().FindAsync(userId);
            if (user == null) {
                _logger?.LogWarning("No user found matching subject Id: {subjectId}", userId);
            }

            return user;
        }
    }
}
