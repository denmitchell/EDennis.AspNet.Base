using EDennis.NetStandard.Base;
using System.Net.Http;

namespace EDennis.Samples.ColorApp.Razor {
    public class RgbApiClient : CrudApiClient<Rgb> {
        public RgbApiClient(IHttpClientFactory clientFactory, ITokenService tokenService) 
            : base(clientFactory, tokenService) {
        }

        public override string ControllerName => "Rgb";
    }
}
