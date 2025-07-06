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

                // Get existing cart from session
                var cartItems = new List<CartItem>();
                var cartJson = HttpContext.Session.GetString(CartSessionKey);

                if (!string.IsNullOrEmpty(cartJson))
                {
                    cartItems = System.Text.Json.JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
                }

                // Check if the item already exists in cart
                var existingItem = cartItems.FirstOrDefault(i => i.OrchidId == orchidId);
                if (existingItem != null)
                {
                    // Increase quantity of existing item
                    existingItem.Quantity++;
                }
                else
                {
                    // Add new item to cart
                    cartItems.Add(new CartItem
                    {
                        OrchidId = orchidId,
                        UserId = int.Parse(userId),
                        Quantity = 1
                    });
                }

                // Save updated cart back to session
                HttpContext.Session.SetString(CartSessionKey,
                    System.Text.Json.JsonSerializer.Serialize(cartItems));

                TempData["SuccessMessage"] = "Item added to cart successfully!";
                return RedirectToPage();
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