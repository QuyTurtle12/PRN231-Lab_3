using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using BusinessObjects;
using DataAccess.DTO.Orchid;
using System.Net.Http.Json;
using IdentityAjaxClient.Model;

namespace IdentityAjaxClient.Pages.OrchidPage.Management
{
    public class CreateModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly IWebHostEnvironment _environment;

        public CreateModel(IHttpClientFactory httpClientFactory, IWebHostEnvironment environment)
        {
            _httpClient = httpClientFactory.CreateClient("API");
            _environment = environment;
        }

        [BindProperty]
        public CreateOrchidDTO Orchid { get; set; } = default!;
        public SelectList? CategoryList { get; set; }

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Fetch categories for dropdown
                var categories = await _httpClient.GetFromJsonAsync<PaginationDTO<Category>>("categories");
                if (categories != null)
                {
                    CategoryList = new SelectList(categories.Items.ToList(), "CategoryId", "CategoryName");
                }
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error loading categories: " + ex.Message);
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
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

                // Reload categories
                var categories = await _httpClient.GetFromJsonAsync<PaginationDTO<Category>>("categories");
                if (categories != null)
                {
                    CategoryList = new SelectList(categories.Items.ToList(), "CategoryId", "CategoryName");
                }

                // Handle image upload if a file was selected
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // Validate file type
                    var allowedTypes = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();

                    if (!allowedTypes.Contains(extension))
                    {
                        ModelState.AddModelError("ImageFile", "Invalid file type. Only .jpg, .jpeg, .png, and .gif are allowed.");
                        return Page();
                    }

                    // Generate unique filename
                    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "images");

                    // Create directory if it doesn't exist
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }

                    // Set the URL in the DTO
                    Orchid.OrchidUrl = $"/images/{uniqueFileName}";
                }

                // Send to API
                var response = await _httpClient.PostAsJsonAsync("orchids", Orchid);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Orchid created successfully.";
                    return RedirectToPage("./Index");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, $"Failed to create orchid: {error}");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error creating orchid: " + ex.Message);
                return Page();
            }
        }
    }
}