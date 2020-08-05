using EDennis.NetStandard.Base;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using M = IdentityServer4.Models;

namespace EDennis.AspNet.Base {

    /// <summary>
    /// Controller for managing Clients in IdentityServer
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public abstract class IdpClientController<TContext> : IdpBaseController
        where TContext : ConfigurationDbContext {

        private readonly TContext _dbContext;

        public IdpClientController(TContext dbContext) {
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
        /// Creates a new Client record
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

        /// <summary>
        /// Patch-updates a Client record with data from the provided partialModel
        /// (JSON body).
        /// </summary>
        /// <param name="partialModel">JSON object with properties to update</param>
        /// <param name="clientId">The ID of the client to update</param>
        /// <param name="mergeCollections">for each collection property, whether to merge 
        /// (default=true) or replace (false) provided items with existing items</param>
        /// <returns></returns>
        [HttpPatch("{clientId}")]
        public async Task<IActionResult> PatchAsync([FromBody] JsonElement partialModel, 
            [FromRoute] string clientId, [FromQuery] bool mergeCollections = true) {

            var existing = _dbContext.Clients.FirstOrDefault(a => a.ClientId == clientId);
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
