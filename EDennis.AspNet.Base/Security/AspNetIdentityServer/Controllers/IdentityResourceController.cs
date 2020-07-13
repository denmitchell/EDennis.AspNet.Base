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

    [Authorize(Policy = "AdministerIDP")]
    [Route("api/[controller]")]
    [ApiController]
    public abstract class IdentityResourceController<TContext> : ControllerBase
        where TContext: ConfigurationDbContext {

        private readonly TContext _dbContext;

        public IdentityResourceController(TContext dbContext) {
            _dbContext = dbContext;
        }


        [HttpGet("{name}")]
        public async Task<IActionResult> GetAsync([FromRoute] string name) {
            var result = await _dbContext.IdentityResources.FirstOrDefaultAsync(a => a.Name == name);
            if (result == null)
                return NotFound();
            else
                return Ok(new IdentityResourceEditModel {
                    Name = name,
                    UserClaimTypes = result.UserClaims.Select(c => c.Type)
                });
        }


        [HttpGet("{name}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] string name) {
            var result = await _dbContext.IdentityResources.FirstOrDefaultAsync(a => a.Name == name);
            if (result == null)
                return NotFound();
            else {
                _dbContext.Remove(result);
                await _dbContext.SaveChangesAsync();
                return NoContent();
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] IdentityResourceEditModel model) {

            var apiResource = new IdentityResource {
                Name = model.Name,
                DisplayName = model.Name,
                Enabled = true,
                UserClaims = model.UserClaimTypes.Select(c => new IdentityClaim { Type = c }).ToList()
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

            var existing = _dbContext.IdentityResources.FirstOrDefault(a => a.Name == name);
            if (existing == null)
                return NotFound();

            try {
                if (partialModel.TryGetProperty("UserClaimTypes", out JsonElement userClaims))
                    existing.UserClaims = userClaims.EnumerateArray().Select(e =>
                        new IdentityClaim {
                            Type = e.GetString()
                        }).ToList();
            } catch (InvalidOperationException ex) {
                ModelState.AddModelError("UserClaimTypes", ex.Message);
                return BadRequest(ModelState);
            }


            _dbContext.Entry(existing).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            return Ok();

        }

    }
}
