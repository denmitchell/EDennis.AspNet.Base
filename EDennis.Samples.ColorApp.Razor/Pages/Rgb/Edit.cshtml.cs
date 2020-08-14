using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Threading.Tasks;
using S=EDennis.Samples.ColorApp;

namespace EDennis.Samples.ColorApp.Razor.Pages.Rgb {
    public class EditModel : PageModel
    {
        private readonly RgbApiClient _apiClient;

        public EditModel(RgbApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [BindProperty]
        public S.Rgb Rgb { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var result = await _apiClient.GetByIdAsync(id.ToString());
            if (result.StatusCode == (int)HttpStatusCode.NotFound)
                return NotFound();

            Rgb = result.TypedValue;

            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var result = await _apiClient.UpdateAsync(Rgb.Id.ToString(), Rgb);

            if (result.StatusCode == (int)HttpStatusCode.NotFound)
                return NotFound();
            else if (result.StatusCode == (int)HttpStatusCode.Conflict)
                return new ConflictObjectResult(result.Value);

            return RedirectToPage("./Index");
        }

    }
}
