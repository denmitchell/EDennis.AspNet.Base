using Microsoft.AspNetCore.Mvc;
using System;
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

        private async Task<TRole> FindAsync(string pathParameter) {
            if (int.TryParse(pathParameter, out int id))
                return await _roleManager.FindByIdAsync(id);
            else
                return await _roleManager.FindByNameAsync(pathParameter);
        }

    }
}
