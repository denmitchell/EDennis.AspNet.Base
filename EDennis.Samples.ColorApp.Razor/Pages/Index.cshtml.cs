using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EDennis.Samples.ColorApp.Razor.Pages {
    public class IndexModel : PageModel
    {
        private readonly RgbApiClient _apiClient;

        public IndexModel(RgbApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public IList<Rgb> Rgb { get;set; }

        public async Task OnGetAsync()
        {
            var dynamicLinqResult = (await _apiClient.GetWithDynamicLinqAsync()).TypedValue;
            Rgb = dynamicLinqResult.Data;
        }
    }
}
