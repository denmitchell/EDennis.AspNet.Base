using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {

    [Route("api/{controller}")]
    [ApiController]
    public abstract class DomainRoleController<TRole> : ControllerBase
        where TRole : DomainRole {

        private readonly DomainRoleManager<TRole> _roleManager;

        public DomainRoleController(DomainRoleManager<TRole> roleManager) {
            _roleManager = roleManager;
        }


        [HttpGet("{application}")]
        public async Task<IActionResult> GetRolesForApplicationAsync([FromRoute] string application) {
            try {
                return Ok(await _roleManager.GetRolesForApplicationAsync(application));
            } catch (Exception ex) {
                ModelState.AddModelError("", ex.Message);
                return Conflict(ModelState);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] TRole role) {
            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
                return Ok(await _roleManager.FindByNameAsync(role.Name));
            else
                return Conflict(result.Errors);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] TRole role) {
            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
                return Ok(await _roleManager.FindByNameAsync(role.Name));
            else
                return Conflict(result.Errors);
        }


        [HttpDelete("{pathParameter}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] string pathParameter) {
            var role = await FindAsync(pathParameter);
            return await DeleteAsync(role);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromBody] TRole role) {
            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
                return NoContent();
            else
                return Conflict(result.Errors);
        }




        [HttpPut("{pathParameter}/claims")]
        public async Task UpdateClaimsAsync([FromRoute] string pathParameter, [FromBody] IEnumerable<ClaimView> claims) {
            var role = await FindAsync(pathParameter);
            var existingClaims = (await _roleManager.GetClaimsAsync(role)).Select(c => new ClaimView { Type = c.Type, Value = c.Value });
            var claimsToAdd = claims.Except(existingClaims).Select(c => new Claim(c.Type, c.Value));
            var claimsToRemove = existingClaims.Except(claims).Select(c => new Claim(c.Type, c.Value));
            await _roleManager.RemoveClaimsAsync(role, claimsToRemove);
            await _roleManager.AddClaimsAsync(role, claimsToAdd);
        }


        [HttpPost("{pathParameter}/claim")]
        public async Task AddClaimAsync([FromRoute] string pathParameter, [FromBody] ClaimView claim) {
            var role = await FindAsync(pathParameter);
            await _roleManager.AddClaimAsync(role, new Claim(claim.Type, claim.Value));
        }


        [HttpPost("{pathParameter}/claims")]
        public async Task AddClaimsAsync([FromRoute] string pathParameter, [FromBody] IEnumerable<ClaimView> claims) {
            var role = await FindAsync(pathParameter);
            await _roleManager.AddClaimsAsync(role, claims.Select(c => new Claim(c.Type, c.Value)));
        }


        [HttpDelete("{pathParameter}/claim")]
        public async Task RemoveClaimAsync([FromRoute] string pathParameter, [FromBody] ClaimView claim) {
            var role = await FindAsync(pathParameter);
            await _roleManager.RemoveClaimAsync(role, new Claim(claim.Type, claim.Value));
        }


        [HttpDelete("{pathParameter}/claims")]
        public async Task RemoveFromClaimsAsync([FromRoute] string pathParameter, [FromBody] IEnumerable<ClaimView> claims) {
            var role = await FindAsync(pathParameter);
            await _roleManager.RemoveClaimsAsync(role, claims.Select(c => new Claim(c.Type, c.Value)));
        }




        private async Task<TRole> FindAsync(string pathParameter) {
            if (int.TryParse(pathParameter, out int id))
                return await _roleManager.FindByIdAsync(id);
            else
                return await _roleManager.FindByNameAsync(pathParameter);
        }

    }
}
