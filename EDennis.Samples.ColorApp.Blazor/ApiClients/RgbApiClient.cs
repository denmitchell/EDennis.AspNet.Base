using EDennis.NetStandard.Base;
using System.Net.Http;

namespace EDennis.Samples.ColorApp {
    public class RgbApiClient : BlazorCrudApiClient<Rgb> {
        public RgbApiClient(HttpClient client) : base(client) {
        }

        public override string ControllerName => "Rgb";

    }
}
