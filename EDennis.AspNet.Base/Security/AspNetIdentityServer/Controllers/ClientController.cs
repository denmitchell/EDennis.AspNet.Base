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

namespace EDennis.AspNet.Base.Security {

    [Authorize(Policy ="AdministerIDP")]
    [Route("api/[controller]")]
    [ApiController]
    public abstract class ClientController<TContext> : ControllerBase
        where TContext: ConfigurationDbContext {

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

            var client = new Client {
                ClientId = model.ClientId,
                AllowOfflineAccess = model.AllowOfflineAccess,
                RequireConsent = model.RequireConsent,
                RequirePkce = model.RequirePkce,

                AllowedScopes = model.AllowedScopes.Select(s => new ClientScope { Scope = s }).ToList(),
                ClientClaimsPrefix = model.ClientClaimsPrefix,

                AllowedCorsOrigins = model.AllowedCorsOrigins.Select(o => new ClientCorsOrigin { Origin = o }).ToList(),
                AllowedGrantTypes = model.AllowedGrantTypes.Select(g => new ClientGrantType { GrantType = g }).ToList(),
                ClientSecrets = model.ClientSecrets.ToList(),
                RedirectUris = model.RedirectUris.Select(u => new ClientRedirectUri { RedirectUri = u }).ToList(),
                PostLogoutRedirectUris = model.PostLogoutRedirectUris.Select(u => new ClientPostLogoutRedirectUri { PostLogoutRedirectUri = u }).ToList(),
                Claims = model.Claims.Select(c => new ClientClaim { Type = c.ClaimType, Value = c.ClaimValue }).ToList(),
                Enabled = true,
            };

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

            try {
                if (partialModel.TryGetProperty("AllowOfflineAccess", out JsonElement allowOfflineAccess))
                    existing.AllowOfflineAccess = allowOfflineAccess.GetBoolean();
            } catch (InvalidOperationException ex) {
                ModelState.AddModelError("AllowOfflineAccess", ex.Message);
            }

            try {
                if (partialModel.TryGetProperty("RequireConsent", out JsonElement requireConsent))
                    existing.RequireConsent = requireConsent.GetBoolean();
            } catch (InvalidOperationException ex) {
                ModelState.AddModelError("RequireConsent", ex.Message);
            }

            try {
                if (partialModel.TryGetProperty("RequirePkce", out JsonElement requirePkce))
                    existing.RequirePkce = requirePkce.GetBoolean();
            } catch (InvalidOperationException ex) {
                ModelState.AddModelError("RequirePkce", ex.Message);
            }

            try {
                if (partialModel.TryGetProperty("ClientClaimsPrefix", out JsonElement clientClaimsPrefix))
                    existing.ClientClaimsPrefix = clientClaimsPrefix.GetString();
            } catch (InvalidOperationException ex) {
                ModelState.AddModelError("ClientClaimsPrefix", ex.Message);
            }

            try {
                if (partialModel.TryGetProperty("AllowedScopes", out JsonElement allowedScopes))
                    existing.AllowedScopes = allowedScopes.EnumerateArray().Select(e =>
                        new ClientScope {
                            Scope = e.GetString()
                        }).ToList();
            } catch (InvalidOperationException ex) {
                ModelState.AddModelError("AllowedScopes", ex.Message);
                return BadRequest(ModelState);
            }

            try {
                if (partialModel.TryGetProperty("AllowedGrantTypes", out JsonElement allowedGrantTypes))
                    existing.AllowedGrantTypes = allowedGrantTypes.EnumerateArray().Select(e =>
                        new ClientGrantType {
                            GrantType = e.GetString()
                        }).ToList();
            } catch (InvalidOperationException ex) {
                ModelState.AddModelError("AllowedGrantTypes", ex.Message);
                return BadRequest(ModelState);
            }

            try {
                if (partialModel.TryGetProperty("AllowedCorsOrigins", out JsonElement allowedCorsOrigins))
                    existing.AllowedCorsOrigins = allowedCorsOrigins.EnumerateArray().Select(e =>
                        new ClientCorsOrigin {
                            Origin = e.GetString()
                        }).ToList();
            } catch (InvalidOperationException ex) {
                ModelState.AddModelError("AllowedCorsOrigins", ex.Message);
                return BadRequest(ModelState);
            }

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

            if(ModelState.ErrorCount > 0)
                return BadRequest(ModelState);


            _dbContext.Entry(existing).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            return Ok();

        }

    }
}
