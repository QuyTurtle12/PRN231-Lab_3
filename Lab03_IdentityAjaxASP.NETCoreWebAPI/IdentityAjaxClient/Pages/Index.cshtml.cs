using System.Text.Json;
using BusinessObjects;
using DataAccess.DTO.Order;
using IdentityAjaxClient.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IdentityAjaxClient.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly HttpClient _httpClient;

        public IndexModel(ILogger<IndexModel> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public bool IsCustomer { get; private set; }
        public PaginationDTO<Orchid> Orchids { get; set; } = default!;
        public SelectList? CategoryList { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? NameSearch { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? CategoryIdSearch { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? IsNatural { get; set; }
        private const string CartSessionKey = "Cart";

        public async Task OnGetAsync(
            int pageIndex = 1,
            int pageSize = 9)
        {
            try
            {
                // Get current user role from session
                var userRole = HttpContext.Session.GetString("UserRole");

                IsCustomer = !string.IsNullOrEmpty(userRole) && userRole.Equals("Customer", StringComparison.OrdinalIgnoreCase);

                // Fetch categories for dropdown
                var categoriesResponse = await _httpClient.GetFromJsonAsync<PaginationDTO<Category>>("categories");
                if (categoriesResponse != null)
                {
                    CategoryList = new SelectList(categoriesResponse.Items.ToList(), "CategoryId", "CategoryName", CategoryIdSearch);
                }

                // Build query string for orchids
                var query = $"orchids?pageIndex={pageIndex}&pageSize={pageSize}";

                if (!string.IsNullOrWhiteSpace(NameSearch))
                {
                    query += $"&nameSearch={Uri.EscapeDataString(NameSearch)}";
                }

                if (!string.IsNullOrWhiteSpace(CategoryIdSearch))
                {
                    query += $"&categoryIdSearch={CategoryIdSearch}";
                }

                if (IsNatural.HasValue)
                {
                    query += $"&isNatural={IsNatural.Value}";
                }

                // Fetch orchids with pagination and filtering
                var response = await _httpClient.GetFromJsonAsync<PaginationDTO<Orchid>>(query);
                Orchids = response ?? new PaginationDTO<Orchid>
                {
                    Items = new List<Orchid>(),
                    PageNumber = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    TotalPages = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading orchids");
                Orchids = new PaginationDTO<Orchid>
                {
                    Items = new List<Orchid>(),
                    PageNumber = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    TotalPages = 0
                };
            }
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int orchidId)
        {
            if (orchidId <= 0)
            {
                return BadRequest();
            }

            try
            {
                // Get user ID from session
                var userId = HttpContext.Session.GetString("UserId");

                if (string.IsNullOrEmpty(userId))
                {
                    // Redirect to login if user is not authenticated
                    return RedirectToPage("/AuthPage/Login");
                }

                // Fetch the orchid from API to get its details
                var orchidResponse = await _httpClient.GetAsync($"orchids?idSearch={orchidId}");
                if (!orchidResponse.IsSuccessStatusCode)
                {
                    return NotFound();
                }

                var orchidPaginatedList = await orchidResponse.Content.ReadFromJsonAsync<PaginationDTO<Orchid>>();

                Orchid? orchid = orchidPaginatedList?.Items.FirstOrDefault();

                if (orchid == null)
                {
                    TempData["ErrorMessage"] = "Could not find the specified orchid.";
                    return RedirectToPage();
                }

                // Get current cart
                var cartJson = HttpContext.Session.GetString("Cart");
                var cart = string.IsNullOrEmpty(cartJson)
                    ? new List<CartItem>()
                    : JsonSerializer.Deserialize<List<CartItem>>(cartJson);

                // Add item to cart
                var existingItem = cart?.FirstOrDefault(x => x.OrchidId == orchidId);
                if (existingItem != null)
                {
                    existingItem.Quantity += 1;
                }
                else
                {
                    cart?.Add(new CartItem { OrchidId = orchidId, Quantity = 1 });
                }

                // Save cart
                HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));

                return new JsonResult(new
                {
                    success = true,
                    message = existingItem != null ? "Item quantity increased in cart!" : "Item added to cart successfully!",
                    cartCount = cart?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart");
                TempData["ErrorMessage"] = $"Error adding item to cart: {ex.Message}";
                return RedirectToPage();
            }
        }
    }
}