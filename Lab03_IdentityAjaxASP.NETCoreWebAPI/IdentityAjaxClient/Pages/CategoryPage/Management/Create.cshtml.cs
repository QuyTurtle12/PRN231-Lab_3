using DataAccess.DTO.Category;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityAjaxClient.Pages.CategoryPage.Management
{
    public class CreateModel : BasePageModel
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(IHttpClientFactory httpClientFactory, ILogger<CreateModel> logger)
        {
            _httpClient = httpClientFactory.CreateClient("API");
            _logger = logger;
        }

        [BindProperty]
        public CreateCategoryDTO Category { get; set; } = default!;

        public IActionResult OnGet()
        {
            // Check if user is authorized
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole) || !userRole.Equals("Staff", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "You are not authorized to create categories.";
                return RedirectToPage("/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Check if user is authorized
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole) || !userRole.Equals("Staff"))
            {
                return RedirectToPage("/Index");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("categories", Category);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Category created successfully.";
                    return RedirectToPage("./Index");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, $"Failed to create Category: {error}");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the category. Please try again.");
                return Page();
            }
        }
    }
}