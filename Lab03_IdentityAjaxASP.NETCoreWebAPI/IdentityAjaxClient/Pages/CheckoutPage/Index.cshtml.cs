using BusinessObjects;
using DataAccess.DTO.Order;
using IdentityAjaxClient.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace IdentityAjaxClient.Pages.CheckoutPage
{
    public class IndexModel : BasePageModel
    {
        private const string CartSessionKey = "Cart";
        private readonly HttpClient _httpClient;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IHttpClientFactory httpClientFactory, ILogger<IndexModel> logger)
        {
            _httpClient = httpClientFactory.CreateClient("API");
            _logger = logger;
        }

        [BindProperty]
        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();

        public decimal TotalPrice { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public Account? UserAccount { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/AuthPage/Login", new { returnUrl = "/CheckoutPage/Index" });
            }

            try
            {
                // Get user account information
                var accountResponse = await _httpClient.GetAsync($"accounts?idSearch={userId}");

                if (accountResponse.IsSuccessStatusCode)
                {
                    var accountList = await accountResponse.Content.ReadFromJsonAsync<PaginationDTO<Account>>();
                    UserAccount = accountList?.Items.FirstOrDefault();
                }

                // Get cart from session
                var cartJson = HttpContext.Session.GetString(CartSessionKey);

                if (string.IsNullOrEmpty(cartJson) || JsonSerializer.Deserialize<List<CartItem>>(cartJson)?.Count == 0)
                {
                    // Redirect to cart page if cart is empty
                    TempData["ErrorMessage"] = "Your cart is empty. Add items before proceeding to checkout.";
                    return RedirectToPage("/CartPage/Index");
                }

                // Load cart items with product details
                await LoadCartItemsAsync(cartJson);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading checkout page");
                ErrorMessage = "An error occurred while loading the checkout page.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            // Check if user is logged in
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Index");
            }

            // Check if user is a customer
            if (string.IsNullOrEmpty(userRole) || userRole != "Customer")
            {
                TempData["ErrorMessage"] = "You must be logged in as a customer to proceed with checkout.";
                return RedirectToPage("/Index");
            }

            // Get cart from session
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
            {
                TempData["ErrorMessage"] = "Your cart is empty. Add items before proceeding to checkout.";
                return RedirectToPage("/CartPage/Index");
            }

            var sessionCartItems = JsonSerializer.Deserialize<List<CartItem>>(cartJson);
            if (sessionCartItems == null || sessionCartItems.Count == 0)
            {
                TempData["ErrorMessage"] = "Your cart is empty. Add items before proceeding to checkout.";
                return RedirectToPage("/CartPage/Index");
            }

            try
            {
                // Load cart items with product details to calculate prices
                await LoadCartItemsAsync(cartJson);

                // Create order DTO
                var createOrderDto = new CreateOrderDTO
                {
                    AccountId = int.Parse(userId),
                    TotalAmount = TotalPrice,
                    OrderItems = new List<CartItem>()
                };

                // Add order items
                foreach (var item in CartItems)
                {
                    createOrderDto.OrderItems.Add(new CartItem
                    {
                        OrchidId = item.OrchidId,
                        Quantity = item.Quantity,
                        Price = item.Product.Price ?? 0
                    });
                }

                // Submit order to API
                var response = await _httpClient.PostAsJsonAsync("orders", createOrderDto);

                if (response.IsSuccessStatusCode)
                {
                    // Clear cart after successful order
                    HttpContext.Session.Remove(CartSessionKey);

                    // Get the created order ID
                    var createdOrder = await response.Content.ReadFromJsonAsync<Order>();

                    // Redirect to confirmation page
                    return RedirectToPage("./Confirmation", new { id = createdOrder?.Id });
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to create order. API Response: {ErrorResponse}", errorResponse);

                    ErrorMessage = "Failed to place your order. Please try again.";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing checkout");
                ErrorMessage = "An unexpected error occurred. Please try again.";
                return Page();
            }
        }

        private async Task LoadCartItemsAsync(string cartJson)
        {
            var sessionCartItems = JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();

            // Convert session cart items to view model with orchid details
            CartItems = new List<CartItemViewModel>();
            foreach (var item in sessionCartItems)
            {
                try
                {
                    // Fetch the orchid from API using the orchidId
                    var orchidResponse = await _httpClient.GetAsync($"orchids?idSearch={item.OrchidId}");
                    if (orchidResponse.IsSuccessStatusCode)
                    {
                        var orchidPaginatedList = await orchidResponse.Content.ReadFromJsonAsync<PaginationDTO<Orchid>>();
                        Orchid? orchid = orchidPaginatedList?.Items.FirstOrDefault();

                        if (orchid != null)
                        {
                            CartItems.Add(new CartItemViewModel
                            {
                                OrchidId = item.OrchidId,
                                Quantity = item.Quantity,
                                Product = orchid
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading orchid details for checkout");
                }
            }

            // Calculate total price
            TotalPrice = CartItems.Sum(item => (item.Product.Price ?? 0) * item.Quantity);
        }
    }
}