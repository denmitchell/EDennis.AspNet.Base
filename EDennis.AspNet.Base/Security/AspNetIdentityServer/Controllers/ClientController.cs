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
using EDennis.AspNet.Base.Security.AspNetIdentityServer.Models.EditModels;

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
