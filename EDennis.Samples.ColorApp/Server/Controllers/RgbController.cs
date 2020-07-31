using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EDennis.Samples.ColorApi.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class RgbController : ControllerBase {

        public RgbController(ILogger<RgbController> logger) {
            logger.LogTrace("RgbController entered.");
        }



    }
}
