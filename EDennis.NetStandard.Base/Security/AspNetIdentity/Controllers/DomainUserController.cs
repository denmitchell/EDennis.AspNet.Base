using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {

    [Route("api/{controller}")]
    [ApiController]
    public abstract class DomainUserController<TUser, TRole> : ControllerBase
        where TUser : DomainUser
        where TRole : DomainRole {

        private readonly DomainUserManager<TUser, TRole> _userManager;

        public DomainUserController(DomainUserManager<TUser, TRole> userManager) {
            _userManager = userManager;
        }



        [HttpGet("org/{organization}")]
        public async Task<IActionResult> GetUsersForOrganizationAsync([FromRoute]string organization, 
            [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100) {
            try {
                return Ok(await _userManager.GetUsersForOrganizationAsync(organization,pageNumber,pageSize));
            } catch (Exception ex) {
                ModelState.AddModelError("", ex.Message);
                return Conflict(ModelState);
            }
        }


        [HttpGet("app/{application}")]
        public async Task<IActionResult> GetUsersForApplicationAsync([FromRoute] string application, 
            [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100) {
            try {
                return Ok(await _userManager.GetUsersForApplicationAsync(application, pageNumber, pageSize));
            } catch (Exception ex) {
                ModelState.AddModelError("", ex.Message);
                return Conflict(ModelState);
            }
        }

        [HttpGet("view/{pathParameter}")]
        public async Task<IActionResult> GetUserViewAsync([FromRoute] string pathParameter) {
            try {
                if(int.TryParse(pathParameter, out int id))
                    return Ok(await _userManager.GetUserViewAsync(id));
                else
                    return Ok(await _userManager.GetUserViewAsync(pathParameter));
            } catch (Exception ex) {
                ModelState.AddModelError("", ex.Message);
                return Conflict(ModelState);
            }
        }


        [HttpGet("view/org/{organization}")]
        public async Task<IActionResult> GetUserViewForOrganizationAsync([FromRoute] string organization,
            [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100) {
            try {
                return Ok(await _userManager.GetUserViewForOrganizationAsync(organization, pageNumber, pageSize));
            } catch (Exception ex) {
                ModelState.AddModelError("", ex.Message);
                return Conflict(ModelState);
            }
        }


        [HttpGet("view/app/{application}")]
        public async Task<IActionResult> GetUserViewForApplicationAsync([FromRoute] string application,
            [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100) {
            try {
                return Ok(await _userManager.GetUserViewForApplicationAsync(application, pageNumber, pageSize));
            } catch (Exception ex) {
                ModelState.AddModelError("", ex.Message);
                return Conflict(ModelState);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] TUser user) {
            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
                return Ok(await _userManager.FindByNameAsync(user.UserName));
            else
                return Conflict(result.Errors);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] TUser user) {
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return Ok(await _userManager.FindByNameAsync(user.UserName));
            else
                return Conflict(result.Errors);
        }



        [HttpDelete("{pathParameter}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] string pathParameter) {
            var user = await FindAsync(pathParameter);
            return await DeleteAsync(user);
        }


        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromBody] TUser user) {
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
                return NoContent();
            else
                return Conflict(result.Errors);
        }


        [HttpPut("{pathParameter}/roles")]
        public async Task UpdateRolesAsync([FromRoute] string pathParameter, [FromBody] IEnumerable<string> roleStrings) {
            var user = await FindAsync(pathParameter);
            var existingRoles = (await _userManager.GetRolesAsync(user));
            var rolesToAdd = roleStrings.Except(existingRoles);
            var rolesToRemove = existingRoles.Except(roleStrings);
            await _userManager.RemoveFromRolesAsync(user,rolesToRemove);
            await _userManager.AddToRolesAsync(user,rolesToAdd);
        }


        [HttpPost("{pathParameter}/role/{roleString}")]
        public async Task AddToRoleAsync([FromRoute] string pathParameter, [FromRoute] string roleString) {
            var user = await FindAsync(pathParameter);
            await _userManager.AddToRoleAsync(user, roleString);
        }


        [HttpPost("{pathParameter}/roles")]
        public async Task AddToRolesAsync([FromRoute] string pathParameter, [FromBody] IEnumerable<string> roleStrings) {
            var user = await FindAsync(pathParameter);
            await _userManager.AddToRolesAsync(user, roleStrings);
        }


        [HttpDelete("{pathParameter}/role/{roleString}")]
        public async Task RemoveFromRoleAsync([FromRoute] string pathParameter, [FromRoute] string roleString) {
            var user = await FindAsync(pathParameter);
            await _userManager.RemoveFromRoleAsync(user, roleString);
        }

        [HttpDelete("{pathParameter}/roles")]
        public async Task RemoveFromRolesAsync([FromRoute] string pathParameter, [FromBody] IEnumerable<string> roleStrings) {
            var user = await FindAsync(pathParameter);
            await _userManager.RemoveFromRolesAsync(user, roleStrings);
        }




        [HttpPut("{pathParameter}/claims")]
        public async Task UpdateClaimsAsync([FromRoute] string pathParameter, [FromBody] IEnumerable<ClaimView> claims) {
            var user = await FindAsync(pathParameter);
            var existingClaims = (await _userManager.GetClaimsAsync(user)).Select(c=> new ClaimView { Type = c.Type, Value = c.Value });
            var claimsToAdd = claims.Except(existingClaims).Select(c => new Claim(c.Type, c.Value));
            var claimsToRemove = existingClaims.Except(claims).Select(c => new Claim(c.Type, c.Value));
            await _userManager.RemoveClaimsAsync(user, claimsToRemove);
            await _userManager.AddClaimsAsync(user, claimsToAdd);
        }


        [HttpPost("{pathParameter}/claim")]
        public async Task AddClaimAsync([FromRoute] string pathParameter, [FromBody] ClaimView claim) {
            var user = await FindAsync(pathParameter);
            await _userManager.AddClaimAsync(user, new Claim(claim.Type,claim.Value));
        }


        [HttpPost("{pathParameter}/claims")]
        public async Task AddClaimsAsync([FromRoute] string pathParameter, [FromBody] IEnumerable<ClaimView> claims) {
            var user = await FindAsync(pathParameter);
            await _userManager.AddClaimsAsync(user, claims.Select(c => new Claim(c.Type, c.Value)));
        }


        [HttpDelete("{pathParameter}/claim")]
        public async Task RemoveClaimAsync([FromRoute] string pathParameter, [FromBody] ClaimView claim) {
            var user = await FindAsync(pathParameter);
            await _userManager.RemoveClaimAsync(user, new Claim(claim.Type, claim.Value));
        }


        [HttpDelete("{pathParameter}/claims")]
        public async Task RemoveFromClaimsAsync([FromRoute] string pathParameter, [FromBody] IEnumerable<ClaimView> claims) {
            var user = await FindAsync(pathParameter);
            await _userManager.RemoveClaimsAsync(user, claims.Select(c => new Claim(c.Type, c.Value)));
        }




        private async Task<TUser> FindAsync(string pathParameter) {
            if (int.TryParse(pathParameter, out int id))
                return await _userManager.FindByIdAsync(id);
            else
                return await _userManager.FindByNameAsync(pathParameter);
        }



    }
}
