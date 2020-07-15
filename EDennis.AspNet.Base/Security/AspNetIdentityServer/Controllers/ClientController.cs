using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Mappers;
using M = IdentityServer4.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EDennis.AspNet.Base.Security.PatchExtensions;

namespace EDennis.AspNet.Base.Security {

    [Authorize(Policy = "AdministerIDP")]
    [Route("api/[controller]")]
    [ApiController]
    public abstract class ClientController<TContext> : ControllerBase
        where TContext : ConfigurationDbContext {

        private readonly TContext _dbContext;

        public ClientController(TContext dbContext) {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Returns an instance of IdentityServer4.Models.Client, whose ClientId
        /// matches the clientId route parameter
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        [HttpGet("{clientId}")]
        public async Task<IActionResult> GetAsync([FromRoute] string clientId) {

            var result = await _dbContext.Clients.FirstOrDefaultAsync(a => a.ClientId == clientId);
            if (result == null)
                return NotFound();
            else
                return Ok(result.ToModel());
        }


        /// <summary>
        /// Deletes a Client, whose ClientId
        /// matches the clientId route parameter
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        [HttpGet("{clientId}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] string clientId) {
            var result = await _dbContext.Clients.FirstOrDefaultAsync(c => c.ClientId == clientId);
            if (result == null)
                return NotFound();
            else {
                _dbContext.Remove(result);
                await _dbContext.SaveChangesAsync();
                return NoContent();
            }
        }

        /// <summary>
        /// Creates a new Client record from the 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] M.Client model) {

            var client = model.ToEntity();

            try {
                _dbContext.Add(client);
                await _dbContext.SaveChangesAsync();
            } catch (DbUpdateException ex) {
                ModelState.AddModelError("", ex.Message);
                return BadRequest(ModelState);
            }

            return Ok();
        }


        [HttpPatch("{clientId}")]
        public async Task<IActionResult> PatchAsync([FromBody] JsonElement partialModel, [FromRoute] string clientId) {

            var existing = _dbContext.Clients.FirstOrDefault(a => a.ClientId == clientId);
            if (existing == null)
                return NotFound();


            foreach (var prop in partialModel.EnumerateObject()) {
                try {
                    switch (prop.Name) {
                        case "AbsoluteRefreshTokenLifetime":
                        case "absoluteRefreshTokenLifetime":
                            existing.AbsoluteRefreshTokenLifetime = prop.Value.GetInt32();
                            break;
                        case "AccessTokenLifetime":
                        case "accessTokenLifetime":
                            existing.AccessTokenLifetime = prop.Value.GetInt32();
                            break;
                        case "AccessTokenType":
                        case "accessTokenType":
                            existing.AccessTokenType = prop.Value.GetInt32();
                            break;
                        case "AllowAccessTokensViaBrowser":
                        case "allowAccessTokensViaBrowser":
                            existing.AllowAccessTokensViaBrowser = prop.Value.GetBoolean();
                            break;
                        case "AllowedCorsOrigins":
                        case "allowedCorsOrigins":
                            existing.AllowedCorsOrigins = prop.Value.EnumerateArray().Select(e =>
                                new ClientCorsOrigin {
                                    Origin = e.GetString()
                                }).ToList();
                            break;
                        case "AllowedGrantTypes":
                        case "allowedGrantTypes":
                            existing.AllowedGrantTypes = prop.Value.EnumerateArray().Select(e =>
                                new ClientGrantType {
                                    GrantType = e.GetString()
                                }).ToList();
                            break;
                        case "AllowedScopes":
                        case "allowedScopes":
                            existing.AllowedScopes = prop.Value.EnumerateArray().Select(e =>
                                new ClientScope {
                                    Scope = e.GetString()
                                }).ToList();
                            break;
                        case "AllowOfflineAccess":
                        case "allowOfflineAccess":
                            existing.AllowOfflineAccess = prop.Value.GetBoolean();
                            break;
                        case "AllowPlainTextPkce":
                        case "allowPlainTextPkce":
                            existing.AllowPlainTextPkce = prop.Value.GetBoolean();
                            break;
                        case "AllowRememberConsent":
                        case "allowRememberConsent":
                            existing.AllowRememberConsent = prop.Value.GetBoolean();
                            break;
                        case "AlwaysIncludeUserClaimsInIdToken":
                        case "alwaysIncludeUserClaimsInIdToken":
                            existing.AlwaysIncludeUserClaimsInIdToken = prop.Value.GetBoolean();
                            break;
                        case "AlwaysSendClientClaims":
                        case "alwaysSendClientClaims":
                            existing.AlwaysSendClientClaims = prop.Value.GetBoolean();
                            break;
                        default:
                            break;
                    }
                } catch (InvalidOperationException ex) {
                    ModelState.AddModelError(prop.Name, ex.Message);
                }
            }
/*

            try {
                if (partialModel.TryGetProperty("ClientSecrets", out JsonElement clientSecrets))
                    existing.ClientSecrets = clientSecrets.EnumerateArray().Select(e =>
                        new ClientSecret {
                            Value = e.GetProperty("Value").GetString(),
                            Expiration = e.GetProperty("Expiration").GetDateTime()
                        }).ToList();
            } catch (InvalidOperationException ex) {
                ModelState.AddModelError("ClientSecrets", ex.Message);
                return BadRequest(ModelState);
            }

            try {
                if (partialModel.TryGetProperty("RedirectUris", out JsonElement redirectUris))
                    existing.RedirectUris = redirectUris.EnumerateArray().Select(e =>
                        new ClientRedirectUri {
                            RedirectUri = e.GetString()
                        }).ToList();
            } catch (InvalidOperationException ex) {
                ModelState.AddModelError("RedirectUris", ex.Message);
                return BadRequest(ModelState);
            }

            try {
                if (partialModel.TryGetProperty("PostLogoutRedirectUris", out JsonElement postLogoutRedirectUris))
                    existing.PostLogoutRedirectUris = postLogoutRedirectUris.EnumerateArray().Select(e =>
                        new ClientPostLogoutRedirectUri {
                            PostLogoutRedirectUri = e.GetString()
                        }).ToList();
            } catch (InvalidOperationException ex) {
                ModelState.AddModelError("PostLogoutRedirectUris", ex.Message);
                return BadRequest(ModelState);
            }

            try {
                if (partialModel.TryGetProperty("Claims", out JsonElement claims))
                    existing.Claims = claims.EnumerateArray().Select(e =>
                        new ClientClaim {
                            Type = e.GetProperty("ClaimType").GetString(),
                            Value = e.GetProperty("ClaimValue").GetString(),
                        }).ToList();
            } catch (InvalidOperationException ex) {
                ModelState.AddModelError("Claims", ex.Message);
                return BadRequest(ModelState);
            }

*/

            if (ModelState.ErrorCount > 0)
                return BadRequest(ModelState);


            _dbContext.Entry(existing).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            return Ok();

        }

    }
}
