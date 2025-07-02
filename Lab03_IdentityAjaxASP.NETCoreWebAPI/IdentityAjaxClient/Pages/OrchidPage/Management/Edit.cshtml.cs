using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BusinessObjects;
using DataAccess;

namespace IdentityAjaxClient.Pages.OrchidPage.Management
{
    public class EditModel : PageModel
    {
        private readonly DataAccess.ProductManagementDbContext _context;

        public EditModel(DataAccess.ProductManagementDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Orchid Orchid { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orchid =  await _context.Orchids.FirstOrDefaultAsync(m => m.OrchidId == id);
            if (orchid == null)
            {
                return NotFound();
            }
            Orchid = orchid;
           ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId");
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Orchid).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrchidExists(Orchid.OrchidId))
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

        private bool OrchidExists(int id)
        {
            return _context.Orchids.Any(e => e.OrchidId == id);
        }
    }
}
