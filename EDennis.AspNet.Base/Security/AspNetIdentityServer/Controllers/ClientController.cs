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
using IdentityModel;
using System.Collections.Generic;

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
        public async Task<IActionResult> PatchAsync([FromBody] JsonElement partialModel, 
            [FromRoute] string clientId, [FromQuery] bool mergeCollections = true) {

            var existing = _dbContext.Clients.FirstOrDefault(a => a.ClientId == clientId);
            if (existing == null)
                return NotFound();

            var model = existing.ToModel();

            foreach (var prop in partialModel.EnumerateObject()) {
                try {
                    switch (prop.Name) {
                        case "AbsoluteRefreshTokenLifetime":
                        case "absoluteRefreshTokenLifetime":
                            model.AbsoluteRefreshTokenLifetime = prop.Value.GetInt32();
                            break;
                        case "AccessTokenLifetime":
                        case "accessTokenLifetime":
                            model.AccessTokenLifetime = prop.Value.GetInt32();
                            break;
                        case "AccessTokenType":
                        case "accessTokenType":
                            model.AccessTokenType = (M.AccessTokenType)Enum.Parse(typeof(M.AccessTokenType),prop.Value.GetString());
                            break;
                        case "AllowAccessTokensViaBrowser":
                        case "allowAccessTokensViaBrowser":
                            model.AllowAccessTokensViaBrowser = prop.Value.GetBoolean();
                            break;
                        case "AllowedCorsOrigins":
                        case "allowedCorsOrigins":
                            var allowedCorsOrigins = prop.Value.EnumerateArray().Select(e => e.GetString()).ToArray();
                            if (mergeCollections && model.AllowedCorsOrigins != null)
                                model.AllowedCorsOrigins = model.AllowedCorsOrigins.Union(allowedCorsOrigins).ToArray();
                            else
                                model.AllowedCorsOrigins = allowedCorsOrigins.ToArray();
                            break;
                        case "AllowedGrantTypes":
                        case "allowedGrantTypes":
                            var allowedGrantTypes = prop.Value.EnumerateArray().Select(e => e.GetString()).ToArray();
                            if (mergeCollections && model.AllowedGrantTypes != null)
                                model.AllowedGrantTypes = model.AllowedGrantTypes.Union(allowedGrantTypes).ToArray();
                            else
                                model.AllowedGrantTypes = allowedGrantTypes.ToArray();
                            break;
                        case "AllowedScopes":
                        case "allowedScopes":
                            var allowedScopes = prop.Value.EnumerateArray().Select(e => e.GetString()).ToArray();
                            if (mergeCollections && model.AllowedScopes != null)
                                model.AllowedScopes = model.AllowedScopes.Union(allowedScopes).ToArray();
                            else
                                model.AllowedScopes = allowedScopes.ToArray();
                            break;
                        case "AllowOfflineAccess":
                        case "allowOfflineAccess":
                            model.AllowOfflineAccess = prop.Value.GetBoolean();
                            break;
                        case "AllowPlainTextPkce":
                        case "allowPlainTextPkce":
                            model.AllowPlainTextPkce = prop.Value.GetBoolean();
                            break;
                        case "AllowRememberConsent":
                        case "allowRememberConsent":
                            model.AllowRememberConsent = prop.Value.GetBoolean();
                            break;
                        case "AlwaysIncludeUserClaimsInIdToken":
                        case "alwaysIncludeUserClaimsInIdToken":
                            model.AlwaysIncludeUserClaimsInIdToken = prop.Value.GetBoolean();
                            break;
                        case "AlwaysSendClientClaims":
                        case "alwaysSendClientClaims":
                            model.AlwaysSendClientClaims = prop.Value.GetBoolean();
                            break;
                        case "AuthorizationCodeLifetime":
                        case "authorizationCodeLifetime":
                            model.AuthorizationCodeLifetime = prop.Value.GetInt32();
                            break;
                        case "BackChannelLogoutSessionRequired":
                        case "backChannelLogoutSessionRequired":
                            model.BackChannelLogoutSessionRequired = prop.Value.GetBoolean();
                            break;
                        case "BackChannelLogoutUri":
                        case "backChannelLogoutUri":
                            model.BackChannelLogoutUri = prop.Value.GetString();
                            break;
                        case "Claims":
                        case "claims":
                            var claims = prop.Value.EnumerateArray().Select(e => {
                                var claim = new M.ClientClaim(); 
                                foreach (var prop2 in e.EnumerateObject()) {
                                    switch (prop2.Name) {
                                        case "Type":
                                        case "type":
                                            claim.Type = prop2.Value.GetString();
                                            break;
                                        case "Value":
                                        case "value":
                                            claim.Value = prop2.Value.GetString();
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                return claim;
                            });
                            if (mergeCollections && model.Claims != null)
                                model.Claims = model.Claims.Union(claims).ToArray();
                            else
                                model.Claims = claims.ToArray();
                            break;
                        case "ClientClaimsPrefix":
                        case "clientClaimsPrefix":
                            model.ClientClaimsPrefix = prop.Value.GetString();
                            break;
                        case "ClientId":
                        case "clientId":
                            model.ClientId = prop.Value.GetString();
                            break;
                        case "ClientName":
                        case "clientName":
                            model.ClientName = prop.Value.GetString();
                            break;
                        case "ClientSecrets":
                        case "clientSecrets":
                            var secrets = prop.Value.EnumerateArray().Select(e => {
                                var secret = new M.Secret {
                                    Type = "SharedSecret",
                                    Expiration = DateTime.MaxValue
                                };
                                foreach (var prop2 in e.EnumerateObject()) {
                                    switch (prop2.Name) {
                                        case "Value":
                                        case "value":
                                            secret.Value = prop2.Value.GetString().ToSha256();
                                            break;
                                        case "Type":
                                        case "type":
                                            secret.Type = prop2.Value.GetString();
                                            break;
                                        case "Description":
                                        case "description":
                                            secret.Description = prop2.Value.GetString();
                                            break;
                                        case "Expiration":
                                        case "expiration":
                                            secret.Expiration = prop2.Value.GetDateTime();
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                return secret;
                            });
                            if (mergeCollections && model.ClientSecrets != null)
                                model.ClientSecrets = model.ClientSecrets.Union(secrets).ToArray();
                            else
                                model.ClientSecrets = secrets.ToArray();
                            break;
                        case "ClientUri":
                        case "clientUri":
                            model.ClientUri = prop.Value.GetString();
                            break;
                        case "ConsentLifetime":
                        case "consentLifetime":
                            model.ConsentLifetime = prop.Value.GetInt32();
                            break;
                        case "Description":
                        case "description":
                            model.Description = prop.Value.GetString();
                            break;
                        case "DeviceCodeLifetime":
                        case "deviceCodeLifetime":
                            model.DeviceCodeLifetime = prop.Value.GetInt32();
                            break;
                        case "Enabled":
                        case "enabled":
                            model.Enabled = prop.Value.GetBoolean();
                            break;
                        case "EnableLocalLogin":
                        case "enableLocalLogin":
                            model.EnableLocalLogin = prop.Value.GetBoolean();
                            break;
                        case "FrontChannelLogoutSessionRequired":
                        case "frontChannelLogoutSessionRequired":
                            model.FrontChannelLogoutSessionRequired = prop.Value.GetBoolean();
                            break;
                        case "FrontChannelLogoutUri":
                        case "frontChannelLogoutUri":
                            model.FrontChannelLogoutUri = prop.Value.GetString();
                            break;
                        case "IdentityProviderRestrictions":
                        case "identityProviderRestrictions":
                            var identityProviderRestrictions = prop.Value.EnumerateArray().Select(e => e.GetString()).ToArray();
                            if (mergeCollections && model.IdentityProviderRestrictions != null)
                                model.IdentityProviderRestrictions = model.IdentityProviderRestrictions.Union(identityProviderRestrictions).ToArray();
                            else
                                model.IdentityProviderRestrictions = identityProviderRestrictions.ToArray();
                            break;
                        case "IdentityTokenLifetime":
                        case "identityTokenLifetime":
                            model.IdentityTokenLifetime = prop.Value.GetInt32();
                            break;
                        case "IncludeJwtId":
                        case "includeJwtId":
                            model.IncludeJwtId = prop.Value.GetBoolean();
                            break;
                        case "LogoUri":
                        case "logoUri":
                            model.LogoUri = prop.Value.GetString();
                            break;
                        case "PairWiseSubjectSalt":
                        case "pairWiseSubjectSalt":
                            model.PairWiseSubjectSalt = prop.Value.GetString();
                            break;
                        case "PostLogoutRedirectUris":
                        case "postLogoutRedirectUris":
                            var postLogoutRedirectUris = prop.Value.EnumerateArray().Select(e => e.GetString()).ToArray();
                            if (mergeCollections && model.PostLogoutRedirectUris != null)
                                model.PostLogoutRedirectUris = model.PostLogoutRedirectUris.Union(postLogoutRedirectUris).ToArray();
                            else
                                model.PostLogoutRedirectUris = postLogoutRedirectUris.ToArray();
                            break;
                        case "Properties":
                        case "properties":
                            var properties = new Dictionary<string, string>();
                            prop.Value.EnumerateObject().ToList().ForEach(e => {
                                properties.Add(e.Name, e.Value.GetString());
                            });
                            if (mergeCollections && model.Properties != null)
                                foreach (var entry in properties)
                                    if (model.Properties.ContainsKey(entry.Key))
                                        model.Properties[entry.Key] = entry.Value;
                                    else
                                        model.Properties.Add(entry.Key, entry.Value);
                            else
                                model.Properties = properties;
                            break;
                        case "ProtocolType":
                        case "protocolType":
                            model.ProtocolType = prop.Value.GetString();
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
