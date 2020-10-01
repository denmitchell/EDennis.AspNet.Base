using EDennis.NetStandard.Base;
using IdentityModel.Client;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace EDennis.Samples.ColorApp.Client.Components {
    public partial class RgbSearchBase : DynamicLinqComponentBase<Rgb> {
        [Inject] protected RgbApiClient Client { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }

        public const int PAGE_SIZE = 10;

        public const string ORDER_BY = "Name";

        public int? StatusCode { get; set; }

        protected Pager Pager { get; set; } = new Pager();

        protected override async Task OnInitializedAsync() {
                await ExecuteSearchAsync(true);
        }


        public async Task OnPagerChangedAsync(bool _) => await ExecuteSearchAsync(false);


        public async Task OnSearchAsync() => await ExecuteSearchAsync(true);

        public string Token { get; set; }

        private async Task ExecuteSearchAsync(bool resetRowCount) {
            ObjectResult<DynamicLinqResult<Rgb>> result;

            result = await Client.GetWithDynamicLinqAsync(SearchTable?.Where, null, null, null, null, Pager?.PageSize ?? PAGE_SIZE, resetRowCount ? default(int?) : RowCount);
            StatusCode = result.StatusCode;
            var dlr = result.TypedValue;
            Data = dlr.Data;
            RowCount = dlr.RowCount;

        }

        protected void OnNewAsync(bool _) {
            NavigationManager.NavigateTo("/Rgb/Detail/0?Editable=true");
        }

    }
}
