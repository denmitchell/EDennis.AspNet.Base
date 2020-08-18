using EDennis.NetStandard.Base;
using System.Net;
using System.Threading.Tasks;
using S=EDennis.Samples.ColorApp;

namespace EDennis.Samples.ColorApp.Razor.Pages.Rgb {

    public class IndexModel : DynamicLinqPageModel<S.Rgb>
    {
        public const int PAGE_SIZE = 10;
        private readonly RgbApiClient _apiClient;

        public const string ORDER_BY = "Name";


        public IndexModel(RgbApiClient apiClient)
        {
            _apiClient = apiClient;
            PageSize = PAGE_SIZE;
        }

        public override int SearchTableRowCount => 2;


        public async Task OnGetAsync(
                string fld0, ComparisonOperator op0, string val0,
                string fld1, ComparisonOperator op1, string val1,
                int pageNumber = 1, int? totalRecords = null) {


            SearchTable[0].FieldName = fld0;
            SearchTable[0].Operator = op0;
            SearchTable[0].FieldValue = val0;

            SearchTable[1].FieldName = fld1;
            SearchTable[1].Operator = op1;
            SearchTable[1].FieldValue = val1;

            var where = SearchTable.Where;
            var skip = (pageNumber - 1) * PAGE_SIZE;

            var result = (await _apiClient.GetWithDynamicLinqAsync(where:where,orderBy:ORDER_BY,skip:skip,take:PAGE_SIZE,totalRecords:totalRecords));

            if (result.StatusCode == (int)HttpStatusCode.OK)
                Load(result.TypedValue);

        }
    }
}
