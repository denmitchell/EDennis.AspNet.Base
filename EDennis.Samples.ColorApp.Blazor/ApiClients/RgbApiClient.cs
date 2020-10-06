using EDennis.NetStandard.Base;
using System.Net.Http;

namespace EDennis.Samples.ColorApp {
    public class RgbApiClient : BlazorCrudApiClient<Rgb> {
        public RgbApiClient(IHttpClientFactory factory) : base(
            factory.CreateClient(nameof(RgbApiClient))) {
        }

        public override string ControllerName => "Rgb";

    }
}
