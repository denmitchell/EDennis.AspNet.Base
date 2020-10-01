using EDennis.NetStandard.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
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


        [BindProperty]
        public IEnumerable<SelectListItem> FieldList { get; set; }
            = new SelectListItem[] {
                new SelectListItem { Value = "Id", Text = "Id" },
                new SelectListItem { Value = "Name", Text = "Name" },
                new SelectListItem { Value = "Red", Text = "Red" },
                new SelectListItem { Value = "Green", Text = "Green" },
                new SelectListItem { Value = "Blue", Text = "Blue" }
            };

        [BindProperty] 
        public IEnumerable<SelectListItem> OpList { get; set; }
            = GetEnumAsSelectList<ComparisonOperator>();
        

        private static IEnumerable<SelectListItem> GetEnumAsSelectList<TEnum>() {
            var list = new List<SelectListItem>();
            foreach(var value in Enum.GetValues(typeof(TEnum))) {
                var name = Enum.GetName(typeof(TEnum),(int)value);
                list.Add(new SelectListItem { Value = ((int)value).ToString(), Text = name });
            }
            return list;
        }



        #region query string aliases
        [BindProperty] public string Fld0 { get; set; }
        [BindProperty] public ComparisonOperator Op0 { get; set; }
        [BindProperty] public string Val0 { get; set; }
        [BindProperty] public string Fld1 { get; set; }
        [BindProperty] public ComparisonOperator Op1 { get; set; }
        [BindProperty] public string Val1 { get; set; }
        [BindProperty] public string Fld2 { get; set; }
        [BindProperty] public ComparisonOperator Op2 { get; set; }
        [BindProperty] public string Val2 { get; set; }
        #endregion



        public async Task OnGetAsync(
                string Fld0, ComparisonOperator Op0, string Val0,
                string Fld1, ComparisonOperator Op1, string Val1,
                int pageNumber = 1, int? totalRecords = null) {


            SearchTable[0].FieldName = Fld0;
            SearchTable[0].Operator = Op0;
            SearchTable[0].FieldValue = Val0;

            SearchTable[1].FieldName = Fld1;
            SearchTable[1].Operator = Op1;
            SearchTable[1].FieldValue = Val1;

            var where = SearchTable.Where;
            var skip = (pageNumber - 1) * PAGE_SIZE;

            var result = (await _apiClient.GetWithDynamicLinqAsync(where:where,orderBy:ORDER_BY,skip:skip,take:PAGE_SIZE,totalRecords:totalRecords));

            if (result.StatusCode == (int)HttpStatusCode.OK)
                Load(result.TypedValue);

        }
    }
}
