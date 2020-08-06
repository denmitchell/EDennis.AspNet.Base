using EDennis.NetStandard.Base;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Mappers;

namespace EDennis.AspNetIdentityServer {

    [ApiController]
    [Authorize (Policy ="AdministerIDP")]
    public class AppInitController {

        private readonly ConfigurationDbContext _configDbContext;
        private readonly DomainIdentityDbContext _identityDbContext;
        public AppInitController(ConfigurationDbContext configDbContext,
            DomainIdentityDbContext identityDbContext) {
            _configDbContext = configDbContext;
            _identityDbContext = identityDbContext;
        }

        /// <summary>
        /// Loads an ApiResource model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("/api")]
        public async Task LoadApiResource(ApiResourceOptions options) {
            var model = options.ToModel();
            var apiResource = _configDbContext.ApiResources.FirstOrDefault(a => a.Name == model.Name);
            if (apiResource == null) {
                apiResource = model.ToEntity();
                _configDbContext.ApiResources.Add(apiResource);
                await _configDbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Loads a Client from ClientCredentialsOptions
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        [HttpPost("/client/cc")]
        public async Task LoadClientAsync(ClientCredentialsOptions options) {
            var entity = options.ToModel().ToEntity();
            await LoadClientAsync(entity);
        }


        /// <summary>
        /// Loads a Client from OidcOptions
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        [HttpPost("/client/oidc")]
        public async Task LoadClientAsync(AuthorizationCodeOptions options) {
            var entity = options.ToModel().ToEntity();
            await LoadClientAsync(entity);
        }


        [NonAction]
        private async Task LoadClientAsync(Client entity) {

            int clientId;
            var client = _configDbContext.Clients.FirstOrDefault(c => c.ClientId == entity.ClientId);
            if (client != null)
                clientId = client.Id;
            else {
                _configDbContext.Add(entity);
                await _configDbContext.SaveChangesAsync();
                clientId = entity.Id;
            }
        }


        /// <summary>
        /// Loads a user from a list of TestUser objects
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="testUsers"></param>
        /// <returns></returns>
        [HttpPost("/users")]
        public async Task LoadTestUsersAsync([FromRoute] string appName, [FromBody] List<TestUser> testUsers) {

            Guid appId;
            var app = _identityDbContext.Applications.FirstOrDefault(a => a.Name == appName);
            if (app != null)
                appId = app.Id;
            else {
                appId = CombGuid.Create();
                _identityDbContext.Applications.Add(
                    new DomainApplication {
                        Id = appId,
                        Name = appName,
                        SysUser = Environment.UserName
                    });
                await _identityDbContext.SaveChangesAsync();
            }



            var roles = testUsers.SelectMany(u => u.Roles)
                .ToDictionary(r => r, r => default(Guid));

            foreach (var entry in roles) {
                var role = _identityDbContext.Roles.FirstOrDefault(a => 
                    a.Name == entry.Key && a.ApplicationId == appId);
                if (role != null)
                    roles[entry.Key] = role.Id;
                else {
                    roles[entry.Key] = CombGuid.Create();
                    _identityDbContext.Roles.Add(new DomainRole {
                        Id = roles[entry.Key],
                        ApplicationId = appId,
                        Name = entry.Key,
                        NormalizedName = entry.Key.ToUpper(),
                        SysUser = Environment.UserName
                    });
                    await _identityDbContext.SaveChangesAsync();
                }
            }

            foreach (var entry in testUsers) {

                Guid orgId;
                var org = _identityDbContext.Organizations.FirstOrDefault(a => a.Name == entry.OrganizationName);
                if (org != null)
                    orgId = org.Id;
                else {
                    orgId = CombGuid.Create();
                    _identityDbContext.Organizations.Add(
                        new DomainOrganization {
                            Id = orgId,
                            Name = entry.OrganizationName,
                            SysUser = Environment.UserName
                        });
                    await _identityDbContext.SaveChangesAsync();
                }



                Guid userId;
                var user = _identityDbContext.Users.FirstOrDefault(a => a.Email == entry.Email);
                if (user != null)
                    userId = user.Id;
                else {
                    userId = CombGuid.Create();
                    _identityDbContext.Users.Add(new DomainUser {
                        Id = userId,
                        Email = entry.Email,
                        NormalizedEmail = entry.Email.ToUpper(),
                        UserName = entry.Email,
                        NormalizedUserName = entry.Email.ToUpper(),
                        EmailConfirmed = true,
                        SysUser = Environment.UserName
                    });
                    await _identityDbContext.SaveChangesAsync();
                }

                foreach (var role in entry.Roles) {
                    if (!_identityDbContext.UserRoles.Any(ur => ur.UserId == userId && ur.RoleId == roles[role])) {
                        _identityDbContext.UserRoles.Add(new DomainUserRole {
                            UserId = userId,
                            RoleId = roles[role],
                            SysUser = Environment.UserName
                        });
                        await _identityDbContext.SaveChangesAsync();
                    }
                }

                foreach (var claim in entry.Claims)
                    foreach (var value in claim.Value) {
                        if (!_identityDbContext.UserClaims.Any(ur => ur.UserId == userId && ur.ClaimType == claim.Key && ur.ClaimValue == value)) {
                            _identityDbContext.UserClaims.Add(new DomainUserClaim {
                                UserId = userId,
                                ClaimType = claim.Key,
                                ClaimValue = value,
                                SysUser = Environment.UserName
                            });
                            await _identityDbContext.SaveChangesAsync();
                        }
                    }
            }

        }



    }


}

