using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using BusinessObjects;
using IdentityAjaxClient.Model;

namespace IdentityAjaxClient.Pages.OrderPage.Management
{
    public class EditModel : BasePageModel
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EditModel> _logger;

        public EditModel(IHttpClientFactory httpClientFactory, ILogger<EditModel> logger)
        {
            _httpClient = httpClientFactory.CreateClient("API");
            _logger = logger;
        }

        [BindProperty]
        public Order Order { get; set; } = default!;

        [BindProperty]
        public string OrderStatus { get; set; } = string.Empty;

        public SelectList OrderStatusOptions { get; set; } = default!;

        public string ErrorMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Check if the user is authorized (admin or staff)
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole) ||
                (userRole != "Admin" && userRole != "Staff"))
            {
                return RedirectToPage("/Index");
            }

            try
            {
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

                // Set current status
                OrderStatus = Order.OrderStatus ?? "Processing";

                // Setup order status options
                OrderStatusOptions = new SelectList(
                    new List<string> { "Processing", "Shipping", "Completed", "Cancelled" },
                    OrderStatus);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order for editing");
                ErrorMessage = "An error occurred while loading the order.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            // Check if the user is authorized (admin or staff)
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole) ||
                (userRole != "Admin" && userRole != "Staff"))
            {
                return RedirectToPage("/Index");
            }

            if (!ModelState.IsValid)
            {
                // Repopulate status options on validation failure
                OrderStatusOptions = new SelectList(
                    new List<string> { "Processing", "Shipping", "Completed", "Cancelled" },
                    OrderStatus);
                return Page();
            }

            try
            {
                // Fetch current order first
                var getResponse = await _httpClient.GetAsync($"orders?idSearch={id}");
                if (!getResponse.IsSuccessStatusCode)
                {
                    return NotFound();
                }

                var orderPaginatedList = await getResponse.Content.ReadFromJsonAsync<PaginationDTO<Order>>();
                var currentOrder = orderPaginatedList?.Items.FirstOrDefault();

                if (currentOrder == null)
                {
                    return NotFound();
                }

                // Update only the status field
                currentOrder.OrderStatus = OrderStatus;

                // Send update to API
                var updateResponse = await _httpClient.PutAsJsonAsync($"orders/{id}", currentOrder);

                if (updateResponse.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Order status updated successfully.";
                    return RedirectToPage("./Details", new { id });
                }

                var errorContent = await updateResponse.Content.ReadAsStringAsync();
                ErrorMessage = $"Failed to update order status: {errorContent}";

                // Repopulate status options
                OrderStatusOptions = new SelectList(
                    new List<string> { "Processing", "Shipping", "Completed", "Cancelled" },
                    OrderStatus);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status");
                ErrorMessage = "An error occurred while updating the order status.";

                // Repopulate status options
                OrderStatusOptions = new SelectList(
                    new List<string> { "Processing", "Shipping", "Completed", "Cancelled" },
                    OrderStatus);
                return Page();
            }
        }
    }
}