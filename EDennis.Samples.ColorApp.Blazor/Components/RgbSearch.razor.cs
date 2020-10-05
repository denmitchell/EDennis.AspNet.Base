using EDennis.NetStandard.Base;
using Microsoft.AspNetCore.Components;
using System.Text.Json;
using System.Threading.Tasks;

namespace EDennis.Samples.ColorApp.Blazor.Components {
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


        private async Task ExecuteSearchAsync(bool resetRowCount) {
            ObjectResult<DynamicLinqResult<Rgb>> result;

            System.Diagnostics.Debug.WriteLine(JsonSerializer.Serialize(SearchTable));

            var where = SearchTable?.Where;
            var take = Pager?.PageSize ?? PAGE_SIZE;

            if(resetRowCount)
                result = await Client.GetWithDynamicLinqAsync(where, null, null, null, 0, take, default(int?));
            else
                result = await Client.GetWithDynamicLinqAsync(where, null, null, null, Pager.CurrentPage - 1, take, RowCount);

            StatusCode = result.StatusCode;
            var dlr = result.TypedValue;
            Data = dlr.Data;
            RowCount = dlr.RowCount;


        }

        protected void OnNewAsync(bool _) {
            NavigationManager.NavigateTo("/Rgb/Details/0?Editable=true");
        }

    }
}
