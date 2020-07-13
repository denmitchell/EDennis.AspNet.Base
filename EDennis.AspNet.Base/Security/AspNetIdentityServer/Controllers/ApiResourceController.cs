using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
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
    public abstract class ApiResourceController<TContext> : ControllerBase
        where TContext: ConfigurationDbContext {

        private readonly TContext _dbContext;

        public ApiResourceController(TContext dbContext) {
            _dbContext = dbContext;
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetAsync([FromRoute] string name) {
            var result = await _dbContext.ApiResources.FirstOrDefaultAsync(a => a.Name == name);
            if (result == null)
                return NotFound();
            else
                return Ok(new ApiResourceEditModel {
                    Name = name,
                    Scopes = result.Scopes.Select(s => s.Name),
                    UserClaimTypes = result.UserClaims.Select(c => c.Type)
                });
        }


        [HttpGet("{name}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] string name) {
            var result = await _dbContext.ApiResources.FirstOrDefaultAsync(a => a.Name == name);
            if (result == null)
                return NotFound();
            else {
                _dbContext.Remove(result);
                await _dbContext.SaveChangesAsync();
                return NoContent();
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] ApiResourceEditModel model) {

            var apiResource = new ApiResource {
                Name = model.Name,
                DisplayName = model.Name,
                Enabled = true,
                UserClaims = model.UserClaimTypes.Select(c => new ApiResourceClaim { Type = c }).ToList(),
                Scopes = model.Scopes.Select(s => new ApiScope { Name = s, DisplayName = s }).ToList()
            };

            try {
                _dbContext.Add(apiResource);
                await _dbContext.SaveChangesAsync();
            } catch (DbUpdateException ex) {
                ModelState.AddModelError("", ex.Message);
                return BadRequest(ModelState);
            }

            return Ok();
        }


        [HttpPatch("{name}")]
        public async Task<IActionResult> PatchAsync([FromBody] JsonElement partialModel, [FromRoute] string name) {

            var existing = _dbContext.ApiResources.FirstOrDefault(a => a.Name == name);
            if (existing == null)
                return NotFound();

            try {
                if (partialModel.TryGetProperty("UserClaimTypes", out JsonElement userClaims))
                    existing.UserClaims = userClaims.EnumerateArray().Select(e =>
                        new ApiResourceClaim {
                            Type = e.GetString()
                        }).ToList();
            } catch (InvalidOperationException ex) {
                ModelState.AddModelError("UserClaimTypes", ex.Message);
                return BadRequest(ModelState);
            }

            try {
                if (partialModel.TryGetProperty("Scopes", out JsonElement scopes))
                    existing.Scopes = scopes.EnumerateArray().Select(e =>
                        new ApiScope {
                            Name = e.GetString()
                        }).ToList();
            } catch (InvalidOperationException ex) {
                ModelState.AddModelError("Scopes", ex.Message);
                return BadRequest(ModelState);
            }


            _dbContext.Entry(existing).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            return Ok();

        }

    }
}
