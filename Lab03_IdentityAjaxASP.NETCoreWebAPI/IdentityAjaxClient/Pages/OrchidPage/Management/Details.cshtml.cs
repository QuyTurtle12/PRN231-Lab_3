using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BusinessObjects;
using DataAccess;

namespace IdentityAjaxClient.Pages.OrchidPage.Management
{
    public class DetailsModel : PageModel
    {
        private readonly DataAccess.ProductManagementDbContext _context;

        public DetailsModel(DataAccess.ProductManagementDbContext context)
        {
            _context = context;
        }

        public Orchid Orchid { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orchid = await _context.Orchids.FirstOrDefaultAsync(m => m.OrchidId == id);
            if (orchid == null)
            {
                return NotFound();
            }
            else
            {
                Orchid = orchid;
            }
            return Page();
        }
    }
}
