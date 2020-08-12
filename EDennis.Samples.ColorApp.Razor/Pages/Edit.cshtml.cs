using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EDennis.Samples.ColorApp;

namespace EDennis.Samples.ColorApp.Razor.Pages
{
    public class EditModel : PageModel
    {
        private readonly EDennis.Samples.ColorApp.ColorContext _context;

        public EditModel(EDennis.Samples.ColorApp.ColorContext context)
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

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Rgb).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RgbExists(Rgb.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool RgbExists(int id)
        {
            return _context.Rgb.Any(e => e.Id == id);
        }
    }
}
