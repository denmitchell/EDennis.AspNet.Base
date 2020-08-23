using EDennis.NetStandard.Base;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace EDennis.Samples.ColorApp.Client.Components {
    public partial class RgbSearchBase : DynamicLinqComponentBase<Rgb> {
        [Inject] public RgbApiClient Client { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        public const int PAGE_SIZE = 10;

        public const string ORDER_BY = "Name";

        protected Pager Pager { get; set; }

        protected override async Task OnInitializedAsync() {
            await base.OnInitializedAsync();
        }


        public async Task OnPagerChangedAsync(bool _) => await ExecuteSearchAsync(false);


        public async Task OnSearchAsync() => await ExecuteSearchAsync(true);


        private async Task ExecuteSearchAsync(bool resetRowCount) {
            ObjectResult<DynamicLinqResult<Rgb>> result;

            if (resetRowCount)
                result = await Client.GetWithDynamicLinqAsync(SearchTable.Where, "Id", null, null, null, Pager.PageSize, null);
            else
                result = await Client.GetWithDynamicLinqAsync(SearchTable.Where, "Id", null, null, null, Pager.PageSize, RowCount);
            
            var dlr = result.TypedValue;
            Data = dlr.Data;
            RowCount = dlr.RowCount;
        }

        protected void OnNewAsync(bool _) {
            NavigationManager.NavigateTo("/Rgb/Detail/0?Editable=true");
        }

    }
}
