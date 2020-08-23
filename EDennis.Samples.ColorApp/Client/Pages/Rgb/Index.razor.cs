using EDennis.NetStandard.Base;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using M = EDennis.Samples.ColorApp;

namespace EDennis.Samples.ColorApp.Client.Pages.Rgb {
    public partial class IndexBase : DynamicLinqComponentBase<M.Rgb> {
        [Inject] public RgbApiClient RgbApiClient { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        public const int PAGE_SIZE = 10;
        private readonly RgbApiClient _client;

        public const string ORDER_BY = "Name";

        protected override async Task OnInitializedAsync() {
            await base.OnInitializedAsync();
        }
    }
}
