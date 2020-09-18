using EDennis.NetStandard.Base;
using EDennis.Samples.ColorApp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace EDennis.Samples.ColorApi.Controllers {
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class RgbController : ProxyCrudController<Rgb> {

        public RgbController(IHttpClientFactory clientFactory, 
            ITokenService tokenService) 
            : base(clientFactory, tokenService) {
        }

        public override string ClientName => "RgbClient";
    }
}
