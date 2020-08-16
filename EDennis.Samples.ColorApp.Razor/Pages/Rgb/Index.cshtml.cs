using EDennis.NetStandard.Base;
using System.Net;
using System.Threading.Tasks;
using S=EDennis.Samples.ColorApp;

namespace EDennis.Samples.ColorApp.Razor.Pages.Rgb {

    public class IndexModel : DynamicLinqPageModel<S.Rgb>
    {
        public const int PAGE_SIZE = 10;
        private readonly RgbApiClient _apiClient;


        public IndexModel(RgbApiClient apiClient)
        {
            _apiClient = apiClient;
            PageSize = PAGE_SIZE;
        }


        public async Task OnGetAsync(
                string fld1, int op1, string val1,
                string fld2, int op2, string val2,
                int pageNumber = 1, int? totalRecords = null) {


            var searchTable = new SearchTable();

            if (fld1 != null)
                searchTable.Add(new SearchRow {
                    FieldName = fld1,
                    Operator = (ComparisonOperator)op1,
                    FieldValue = val1
                });

            if (fld2 != null)
                searchTable.Add(new SearchRow {
                    FieldName = fld2,
                    Operator = (ComparisonOperator)op2,
                    FieldValue = val2
                });

            var where = searchTable.Where;
            var skip = (pageNumber - 1) * PAGE_SIZE;

            var result = (await _apiClient.GetWithDynamicLinqAsync(where:where,skip:skip,take:PAGE_SIZE,totalRecords:totalRecords));

            if (result.StatusCode == (int)HttpStatusCode.OK)
                Load(result.TypedValue);

        }
    }
}
