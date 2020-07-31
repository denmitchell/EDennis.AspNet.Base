using EDennis.NetStandard.Base;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace EDennis.AspNetBase.Security {


    public abstract class IdpUserController : IdpBaseController {

        private readonly DomainUserRepo _repo;

        public IdpUserController(DomainUserRepo repo) {
            _repo = repo;
        }


        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery] string appName = null, [FromQuery] string orgName = null,
            [FromQuery] int? pageNumber = 1, [FromQuery] int? pageSize = 100)
                => await _repo.GetAsync(appName, orgName, pageNumber, pageSize);
        

        [HttpGet("{pathParameter}")]
        public async Task<IActionResult> GetAsync([FromRoute] string pathParameter)
                => await _repo.GetAsync(pathParameter);


        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] JsonElement jsonElement)
                => await _repo.CreateAsync(jsonElement, ModelState, GetSysUser());


        [HttpPatch("{pathParameter}")]
        public async Task<IActionResult> PatchAsync([FromRoute] string pathParameter, [FromBody] JsonElement jsonElement)
                => await _repo.PatchAsync(pathParameter, jsonElement, ModelState, GetSysUser());


        [HttpDelete("{pathParameter}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] string pathParameter)
                => await _repo.DeleteAsync(pathParameter, ModelState, GetSysUser());


        [HttpPut("{userName}/DomainUserClaims")]
        public async Task UpdateUserClaims([FromRoute] string userName, [FromBody] List<DomainUserClaim> claims)
                => await _repo.UpdateDomainUserClaims(userName, claims, GetSysUser());


        [HttpPut("{userName}/DictionaryUserClaims")]
        public async Task UpdateUserClaims([FromRoute] string userName, [FromBody] Dictionary<string,List<string>> claims)
                => await _repo.UpdateDictionaryUserClaims(userName, claims, GetSysUser());


        [HttpPut("{userName}/DomainUserRolesForUser")]
        public async Task UpdateUserRolesForUser([FromRoute] string userName, [FromBody] List<DomainUserRole> roles)
                => await _repo.UpdateDomainUserRolesForUser(userName, roles, GetSysUser());


        [HttpPut("{userName}/DictionaryUserRolesForUser")]
        public async Task UpdateUserRolesForUser([FromRoute] string userName, [FromBody] Dictionary<string, List<string>> roles)
                => await _repo.UpdateDictionaryUserRolesForUser(userName, roles, GetSysUser());


        [HttpPut("{appName}/DomainUserRolesForAppUsers")]
        public async Task UpdateUserRolesForAppUsers([FromRoute] string appName, [FromBody] List<DomainUserRole> roles)
                => await _repo.UpdateDomainUserRolesForAppUsers(appName, roles, GetSysUser());


        [HttpPut("{appName}/DictionaryUserRolesForAppUsers")]
        public async Task UpdateUserRolesForAppUsers([FromRoute] string appName, [FromBody] Dictionary<string, List<string>> roles)
                => await _repo.UpdateDictionaryUserRolesForAppUsers(appName, roles, GetSysUser());

    }
}
