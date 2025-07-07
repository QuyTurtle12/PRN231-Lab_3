using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BusinessObjects;
using IdentityAjaxClient.Model;

namespace IdentityAjaxClient.Pages.OrderPage.Management
{
    public class DetailsModel : BasePageModel
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<IndexModel> _logger;

        public DetailsModel(IHttpClientFactory httpClientFactory, ILogger<IndexModel> logger)
        {
            _httpClient = httpClientFactory.CreateClient("API");
            _logger = logger;
        }

        public Order Order { get; set; } = default!;
        public string ErrorMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Check authorization
                var userIdString = HttpContext.Session.GetString("UserId");
                var userRole = HttpContext.Session.GetString("UserRole");

                // Fetch order by id
                var response = await _httpClient.GetAsync($"orders?idSearch={id}");

                if (!response.IsSuccessStatusCode)
                {
                    return NotFound();
                }

                var orderPaginatedList = await response.Content.ReadFromJsonAsync<PaginationDTO<Order>>();
                Order = orderPaginatedList?.Items.FirstOrDefault();

                if (Order == null)
                {
                    return NotFound();
                }

                // Authorization: Check if user is staff or the owner of this order
                int currentUserId = 0;
                if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int parsedId))
                {
                    currentUserId = parsedId;
                }

                bool isStaff = !string.IsNullOrEmpty(userRole) &&
                     userRole.Equals("Staff", StringComparison.OrdinalIgnoreCase);

                if (!isStaff && currentUserId != Order.AccountId)
                {
                    return RedirectToPage("/Index");
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order details");
                ErrorMessage = "An error occurred while loading order details.";
                return Page();
            }
        }
    }
}