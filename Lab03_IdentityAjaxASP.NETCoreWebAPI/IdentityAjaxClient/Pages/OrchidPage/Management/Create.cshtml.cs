using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using BusinessObjects;
using DataAccess;

namespace IdentityAjaxClient.Pages.OrchidPage.Management
{
    public class CreateModel : PageModel
    {
        private readonly DataAccess.ProductManagementDbContext _context;

        public CreateModel(DataAccess.ProductManagementDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId");
            return Page();
        }

        [BindProperty]
        public Orchid Orchid { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Orchids.Add(Orchid);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
