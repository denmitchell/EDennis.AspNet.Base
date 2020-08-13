using EDennis.NetStandard.Base;
using System.Net.Http;

namespace EDennis.Samples.ColorApp.Razor {
    public class RgbApiClient : CrudApiClient<Rgb> {
        public RgbApiClient(IHttpClientFactory clientFactory, ITokenService tokenService, 
            ScopedRequestMessage scopedRequestMessage) 
            : base(clientFactory, tokenService, scopedRequestMessage) {
        }

        public override string ControllerName => "Rgb";
    }
}
