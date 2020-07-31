using EDennis.NetStandard.Base;
using EDennis.Samples.ColorApp.Shared;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace EDennis.Samples.ColorApi.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class RgbController : ProxyCrudController<Rgb> {

        public RgbController(IHttpClientFactory clientFactory, 
            ClientCredentialsTokenService tokenService) 
            : base(clientFactory, tokenService) {
        }
    }
}
