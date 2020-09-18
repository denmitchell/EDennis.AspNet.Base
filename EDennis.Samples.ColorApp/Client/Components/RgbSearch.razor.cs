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
        [Inject] protected IAccessTokenProvider AccessTokenProvider { get; set; }

        public const int PAGE_SIZE = 10;

        public const string ORDER_BY = "Name";

        protected Pager Pager { get; set; } = new Pager();

        protected override async Task OnInitializedAsync() {
                await ExecuteSearchAsync(true);
        }


        public async Task OnPagerChangedAsync(bool _) => await ExecuteSearchAsync(false);


        public async Task OnSearchAsync() => await ExecuteSearchAsync(true);

        public string Token { get; set; }

        private async Task ExecuteSearchAsync(bool resetRowCount) {
            ObjectResult<DynamicLinqResult<Rgb>> result;

            //Token = AccessTokenProvider.GetType().FullName;


            //var tokenResult = await AccessTokenProvider.RequestAccessToken(
            //    new AccessTokenRequestOptions {
            //        Scopes = new[] { "openid", "profile" }
            //    });

            //if (tokenResult.TryGetToken(out var token)) {
            //    //Token = token.Value;
            //}
            //Client.HttpClient.SetBearerToken(Token);
            //try {
            //    var result1 = await Client.HttpClient.GetFromJsonAsync<Rgb[]>("https://localhost:44336/api/Rgb");
            //    Data = result1.ToList();
            //    RowCount = Data.Count();
            //} catch (AccessTokenNotAvailableException ex) {
            //    ex.Redirect();
            //}

            result = await Client.GetWithDynamicLinqAsync(SearchTable?.Where, null, null, null, null, Pager?.PageSize ?? PAGE_SIZE, resetRowCount ? default(int?) : RowCount);
            var dlr = result.TypedValue;
            Data = dlr.Data;
            RowCount = dlr.RowCount;

        }

        protected void OnNewAsync(bool _) {
            NavigationManager.NavigateTo("/Rgb/Detail/0?Editable=true");
        }

    }
}
