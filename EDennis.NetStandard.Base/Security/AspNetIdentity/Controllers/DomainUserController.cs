using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public async Task UpdateRolesAsync([FromRoute] string pathParameter, [FromBody] IEnumerable<string> roles) {
            var user = await FindAsync(pathParameter);
            var existingRoles = (await _userManager.GetRolesAsync(user));
            var rolesToAdd = roles.Except(existingRoles);
            var rolesToRemove = existingRoles.Except(roles);
            await _userManager.RemoveFromRolesAsync(user,rolesToRemove);
            await _userManager.AddToRolesAsync(user,rolesToAdd);
        }

        [HttpPost("{pathParameter}/role/{role}")]
        public async Task AddToRoleAsync([FromRoute] string pathParameter, [FromBody] string role) {
            var user = await FindAsync(pathParameter);
            await _userManager.AddToRoleAsync(user, role);
        }


        [HttpDelete("{pathParameter}/role")]
        public async Task RemoveFromRoleAsync([FromRoute] string pathParameter, [FromBody] string role) {
            var user = await FindAsync(pathParameter);
            await _userManager.RemoveFromRoleAsync(user, role);
        }




        private async Task<TUser> FindAsync(string pathParameter) {
            if (int.TryParse(pathParameter, out int id))
                return await _userManager.FindByIdAsync(id);
            else
                return await _userManager.FindByNameAsync(pathParameter);
        }



    }
}
