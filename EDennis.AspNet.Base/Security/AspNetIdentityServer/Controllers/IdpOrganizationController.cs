using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Security {

    public abstract class IdpOrganizationController : IdpBaseController {

        private readonly DomainOrganizationRepo _repo;

        public IdpOrganizationController(DomainOrganizationRepo repo) {
            _repo = repo;
        }


        [HttpGet]
        public async Task<IActionResult> GetAsync(
            [FromQuery] int? pageNumber = 1, [FromQuery] int? pageSize = 100)
                => await _repo.GetAsync(pageNumber, pageSize);


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

    }
}
