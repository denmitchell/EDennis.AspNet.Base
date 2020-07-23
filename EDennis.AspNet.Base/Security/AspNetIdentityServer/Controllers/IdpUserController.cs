using EDennis.AspNet.Base.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Threading.Tasks;

namespace EDennis.AspNetBase.Security {


    [Authorize(Policy = "AdministerIDP")]
    [Route("api/[controller]")]
    [ApiController]
    public abstract class IdpUserController : ControllerBase {

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
        public async Task<IActionResult> CreateAsync([FromBody] UserEditModel userEditModel)
                => await _repo.CreateAsync(userEditModel, ModelState);


        [HttpPatch("{pathParameter}")]
        public async Task<IActionResult> PatchAsync([FromRoute] string pathParameter, [FromBody] JsonElement jsonElement)
                => await _repo.PatchAsync(pathParameter, jsonElement, ModelState);


        [HttpDelete("{pathParameter}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] string pathParameter)
                => await _repo.DeleteAsync(pathParameter);


    }
}
