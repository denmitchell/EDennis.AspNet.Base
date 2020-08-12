using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EDennis.Samples.ColorApp;

namespace EDennis.Samples.ColorApp.Razor.Pages
{
    public class DeleteModel : PageModel
    {
        private readonly EDennis.Samples.ColorApp.ColorContext _context;

        public DeleteModel(EDennis.Samples.ColorApp.ColorContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Rgb Rgb { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Rgb = await _context.Rgb.FirstOrDefaultAsync(m => m.Id == id);

            if (Rgb == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Rgb = await _context.Rgb.FindAsync(id);

            if (Rgb != null)
            {
                _context.Rgb.Remove(Rgb);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
