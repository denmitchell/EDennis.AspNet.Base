using EDennis.AspNetIdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Security {
    public class RoleController<TUser, TRole, TContext> : ControllerBase
        where TUser : DomainUser, new()
        where TRole : DomainRole, new()
        where TContext : DomainIdentityDbContext<TUser, TRole> {

        private readonly RoleManager<TRole> _roleManager;
        private readonly TContext _dbContext;

        public RoleController(RoleManager<TRole> roleManager,
            TContext dbContext) {
            _roleManager = roleManager;
            _dbContext = dbContext;
        }

        public static Regex idPattern = new Regex("[0-9a-f]{8}-([0-9a-f]{4}-){3}[0-9a-f]{12}");


        public async Task<ActionResult> GetAsync([FromQuery] string appName = null,
            [FromQuery] int? pageNumber = 1, [FromQuery] int? pageSize = 100
            ) {

            var skip = (pageNumber ?? 1 - 1) * pageSize ?? 100;
            var take = pageSize ?? 100;

            if(appName == null) {
                ModelState.AddModelError("", "The required query parameter appName was missing.");
                return BadRequest(ModelState);
            }

            var qry = _dbContext.Set<IdentityRole>()
                .Where(r => r.Name.StartsWith(appName))
                .Skip(skip)
                .Take(take);

            var result = await qry.ToListAsync();

            return Ok(result);
        }



        [HttpGet("{pathParameter}")]
        public async Task<IActionResult> GetAsync([FromRoute] string pathParameter) {
            var role = await FindAsync(pathParameter);
            if (role == null)
                return NotFound();
            else {
                var currentClaims = await _roleManager.GetClaimsAsync(role);
                var roleEditModel = new RoleModel {
                    Id = role.Id,
                    Name = role.Name,
                    Claims = currentClaims
                };

                return Ok(roleEditModel);

            }
        }


        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] RoleModel roleEditModel) {

            if(roleEditModel.Id == default)
                roleEditModel.Id = Guid.NewGuid();

            var role = new TRole {
                Id = roleEditModel.Id,
                Name = roleEditModel.Name,
                NormalizedName = roleEditModel.Name.ToUpper()
            };

            var results = new List<IdentityResult> {
                await _roleManager.CreateAsync(role)
            };

            if (roleEditModel.Claims != null)
                foreach(var claim in roleEditModel.Claims)
                    results.Add(await _roleManager.AddClaimAsync(role, claim));

            var failures = results.SelectMany(r => r.Errors);

            if (failures.Count() > 0)
                return Conflict(failures);
            else
                return Ok(roleEditModel);

        }


        [HttpPatch("{pathParameter}")]
        public async Task<IActionResult> PatchAsync([FromBody] JsonElement jsonElement, [FromRoute] string pathParameter) {

            var role = await FindAsync(pathParameter);
            if (role == null)
                return NotFound();

            if (jsonElement.TryGetString("Name", out string name)) {
                role.Name = name;
                role.NormalizedName = name.ToUpper();
            }

            role.ConcurrencyStamp = Guid.NewGuid().ToString();

            var results = new List<IdentityResult> {
                await _roleManager.UpdateAsync(role)
            };


            var hasClaims = jsonElement.TryGetProperty("Claims", out JsonElement _);

            var roleEditModel = JsonSerializer.Deserialize<RoleModel>(jsonElement.GetRawText());

            results.AddRange(await UpdateClaimsAsync(role, roleEditModel, hasClaims));

            var failures = results.SelectMany(r => r.Errors);

            if (failures.Count() > 0)
                return BadRequest(failures);
            else
                return NoContent();
        }


        [HttpDelete("{pathParameter}")]
        public async Task<IActionResult> Delete([FromRoute] string pathParameter) {
            var role = await FindAsync(pathParameter);
            if (role == null)
                return NotFound();

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
                return Conflict(result.Errors);
            else
                return NoContent();
        }




        private async Task<IEnumerable<IdentityResult>> UpdateClaimsAsync(TRole role,
            RoleModel roleEditModel, bool hasClaims) {

            List<IdentityResult> results = new List<IdentityResult>();

            if (hasClaims) {
                var currentClaims = await _roleManager.GetClaimsAsync(role);
                
                foreach(var claim in currentClaims.Except(roleEditModel.Claims))
                    results.Add(await _roleManager.RemoveClaimAsync(role, claim));

                foreach (var claim in roleEditModel.Claims.Except(currentClaims))
                    results.Add(await _roleManager.AddClaimAsync(role, claim));

            }

            return results;
        }



        private async Task<TRole> FindAsync(string pathParameter) {
            if (idPattern.IsMatch(pathParameter))
                return await _roleManager.FindByIdAsync(pathParameter);
            else
                return await _roleManager.FindByNameAsync(pathParameter);
        }


    }
}
