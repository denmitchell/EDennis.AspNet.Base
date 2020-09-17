using EDennis.NetStandard.Base;
using System.Net.Http;

namespace EDennis.Samples.ColorApp.Client {
    public class RgbApiClient : BlazorCrudApiClient<Rgb> {
        public RgbApiClient(IHttpClientFactory clientFactory) : base(clientFactory) {
        }

        public override string ControllerName => "Rgb";

        public override string ClientName => "EDennis.Samples.ColorApp.Client";
    }
}
