using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Threading.Tasks;
using S = EDennis.Samples.ColorApp;

namespace EDennis.Samples.ColorApp.Razor.Pages.Rgb {
    public class DetailsModel : PageModel
    {
        private readonly RgbApiClient _apiClient;

        public DetailsModel(RgbApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public S.Rgb Rgb { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var result = await _apiClient.GetByIdAsync(id.ToString());
            if(result.StatusCode == (int)HttpStatusCode.NotFound)
                return NotFound();

            Rgb = result.TypedValue;
            return Page();
        }
    }
}
