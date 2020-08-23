using EDennis.NetStandard.Base;
using System.Net.Http;

namespace EDennis.Samples.ColorApp.Client {
    public class RgbApiClient : BlazorCrudApiClient<Rgb> {
        public RgbApiClient(HttpClient client,  
            ScopedRequestMessage scopedRequestMessage) 
            : base(client, scopedRequestMessage) {
        }

        public override string ControllerName => "Rgb";
    }
}
