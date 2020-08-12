using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace EDennis.Samples.ColorApp.Razor.Pages {
    public class CreateModel : PageModel
    {
        private readonly RgbApiClient _apiClient;

        public CreateModel(RgbApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Rgb Rgb { get; set; }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _apiClient.CreateAsync(Rgb);

            return RedirectToPage("./Index");
        }
    }
}
