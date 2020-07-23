using EDennis.AspNet.Base.Security.AspNetIdentityServer.Models.EditModels;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using M = IdentityServer4.Models;

namespace EDennis.AspNet.Base.Security {

    [Authorize(Policy = "AdministerIDP")]
    [Route("api/[controller]")]
    [ApiController]
    public abstract class IdpIdentityResourceController<TContext> : ControllerBase
        where TContext : ConfigurationDbContext {

        private readonly TContext _dbContext;

        public IdpIdentityResourceController(TContext dbContext) {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Returns an instance of IdentityServer4.Models.IdentityResource, whose name
        /// matches the name route parameter
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet("{name}")]
        public async Task<IActionResult> GetAsync([FromRoute] string name) {

            var result = await _dbContext.IdentityResources.FirstOrDefaultAsync(a => a.Name == name);
            if (result == null)
                return NotFound();
            else
                return Ok(result.ToModel());
        }


        /// <summary>
        /// Deletes a IdentityResource, whose Name
        /// matches the name route parameter
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet("{name}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] string name) {
            var result = await _dbContext.IdentityResources.FirstOrDefaultAsync(c => c.Name == name);
            if (result == null)
                return NotFound();
            else {
                _dbContext.Remove(result);
                await _dbContext.SaveChangesAsync();
                return NoContent();
            }
        }

        /// <summary>
        /// Creates a new IdentityResource record
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] M.IdentityResource model) {

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


        /// <summary>
        /// Patch-updates an IdentityResource record with data from the provided partialModel
        /// (JSON body).
        /// </summary>
        /// <param name="partialModel">JSON object with properties to update</param>
        /// <param name="name">The Name of the IdentityResource to update</param>
        /// <param name="mergeCollections">for each collection property, whether to merge 
        /// (default=true) or replace (false) provided items with existing items</param>
        /// <returns></returns>
        [HttpPatch("{name}")]
        public async Task<IActionResult> PatchAsync([FromBody] JsonElement partialModel,
            [FromRoute] string name, [FromQuery] bool mergeCollections = true) {

            var existing = _dbContext.IdentityResources.FirstOrDefault(a => a.Name == name);
            if (existing == null)
                return NotFound();

            var model = existing.ToModel();

            model.Patch(partialModel, ModelState, mergeCollections);

            if (ModelState.ErrorCount > 0)
                return BadRequest(ModelState);

            existing = model.ToEntity();

            _dbContext.Entry(existing).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            return Ok();

        }

    }
}
