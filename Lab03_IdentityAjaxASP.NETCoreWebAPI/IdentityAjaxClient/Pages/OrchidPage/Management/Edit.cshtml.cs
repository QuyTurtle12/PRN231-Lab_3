using BusinessObjects;
using DataAccess.DTO.Orchid;
using IdentityAjaxClient.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IdentityAjaxClient.Pages.OrchidPage.Management
{
    public class EditModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly IWebHostEnvironment _environment;

        public EditModel(IHttpClientFactory httpClientFactory, IWebHostEnvironment environment)
        {
            _httpClient = httpClientFactory.CreateClient("API");
            _environment = environment;
        }

        [BindProperty]
        public UpdateOrchidDTO Orchid { get; set; } = default!;
        public SelectList? CategoryList { get; set; }

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Get current user role from session
                var userRole = HttpContext.Session.GetString("UserRole");

                bool isStaff = !string.IsNullOrEmpty(userRole) &&
                    userRole.Equals("Staff", StringComparison.OrdinalIgnoreCase);

                // Check authorization
                if (!isStaff)
                {
                    return RedirectToPage("/Index");
                }

                // Fetch orchid details
                var orchidResponse = await _httpClient.GetAsync($"orchids?idSearch={id}");
                if (!orchidResponse.IsSuccessStatusCode)
                {
                    return NotFound();
                }

                var orchidPaginatedList = await orchidResponse.Content.ReadFromJsonAsync<PaginationDTO<Orchid>>();

                Orchid? orchid = orchidPaginatedList?.Items.FirstOrDefault();

                if (orchid == null)
                {
                    return NotFound();
                }

                // Map to UpdateOrchidDTO
                Orchid = new UpdateOrchidDTO
                {
                    OrchidName = orchid.OrchidName,
                    OrchidDescription = orchid.OrchidDescription,
                    CategoryId = orchid.CategoryId,
                    IsNatural = orchid.IsNatural,
                    Price = orchid.Price,
                    OrchidUrl = orchid.OrchidUrl
                };

                // Fetch categories for dropdown
                var categoriesResponse = await _httpClient.GetFromJsonAsync<PaginationDTO<Category>>("categories");
                if (categoriesResponse != null)
                {
                    CategoryList = new SelectList(categoriesResponse.Items.ToList(), "CategoryId", "CategoryName", orchid.CategoryId);
                }

                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error loading orchid: " + ex.Message);
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            try
            {
                // Handle image upload if a file was selected
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var allowedTypes = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();

                    if (!allowedTypes.Contains(extension))
                    {
                        ModelState.AddModelError("ImageFile", "Invalid file type. Only .jpg, .jpeg, .png, and .gif are allowed.");
                        return Page();
                    }

                    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "images");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Delete old image if exists and is not the default image
                    if (!string.IsNullOrEmpty(Orchid.OrchidUrl))
                    {
                        var oldImagePath = Path.Combine(_environment.WebRootPath,
                            Orchid.OrchidUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }

                    Orchid.OrchidUrl = $"/images/{uniqueFileName}";
                }

                // Send update to API
                var response = await _httpClient.PutAsJsonAsync($"orchids/{id}", Orchid);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Orchid updated successfully.";
                    return RedirectToPage("./Index");
                }

                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Failed to update orchid: {error}");

                // Reload categories for the form
                var categories = await _httpClient.GetFromJsonAsync<PaginationDTO<Category>>("categories");
                if (categories != null)
                {
                    CategoryList = new SelectList(categories.Items.ToList(), "CategoryId", "CategoryName", Orchid.CategoryId);
                }

                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error updating orchid: " + ex.Message);
                return Page();
            }
        } 
    }
}
