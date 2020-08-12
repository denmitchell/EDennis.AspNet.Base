using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EDennis.Samples.ColorApp;
using EDennis.NetStandard.Base;

namespace EDennis.Samples.ColorApp.Razor.Pages
{
    public class IndexModel : PageModel
    {
        private readonly EDennis.Samples.ColorApp.ColorContext _context;

        public IndexModel( context)
        {
            _context = context;
        }

        public IList<Rgb> Rgb { get;set; }

        public async Task OnGetAsync()
        {
            Rgb = await _context.Rgb.ToListAsync();
        }
    }
}
