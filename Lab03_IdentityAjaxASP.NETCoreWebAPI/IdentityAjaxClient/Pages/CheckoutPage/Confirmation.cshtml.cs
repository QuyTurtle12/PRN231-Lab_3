using BusinessObjects;
using IdentityAjaxClient.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityAjaxClient.Pages.CheckoutPage
{
    public class ConfirmationModel : BasePageModel
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ConfirmationModel> _logger;

        public ConfirmationModel(IHttpClientFactory httpClientFactory, ILogger<ConfirmationModel> logger)
        {
            _httpClient = httpClientFactory.CreateClient("API");
            _logger = logger;
        }

        public Order? Order { get; set; }
        public List<OrderDetail> OrderItems { get; set; } = new List<OrderDetail>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            try
            {
                // Get user ID from session for authorization
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToPage("/AuthPage/Login");
                }

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

                // Verify that this order belongs to the current user
                if (Order.AccountId != int.Parse(userId))
                {
                    return RedirectToPage("/Index");
                }

                // Get order details
                if (Order.OrderDetails != null)
                {
                    OrderItems = Order.OrderDetails.ToList();
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order confirmation");
                return RedirectToPage("/Index");
            }
        }
    }
}