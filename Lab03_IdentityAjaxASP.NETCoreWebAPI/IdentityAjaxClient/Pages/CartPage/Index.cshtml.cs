using System.Text.Json;
using BusinessObjects;
using DataAccess.DTO.Order;
using IdentityAjaxClient.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityAjaxClient.Pages.CartPage
{
    public class IndexModel : BasePageModel
    {
        private const string CartSessionKey = "Cart";

        private readonly HttpClient _httpClient;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        [BindProperty]
        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();

        public decimal TotalPrice { get; set; }

        public async Task OnGetAsync()
        {

            // Get cart from session
            var cartJson = HttpContext.Session.GetString(CartSessionKey);

            if (!string.IsNullOrEmpty(cartJson))
            {
                var sessionCartItems = JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();

                // Convert session cart items to view model with orchid details
                CartItems = new List<CartItemViewModel>();
                foreach (var item in sessionCartItems)
                {
                    try
                    {
                        // Fetch orchid details
                        var orchidResponse = await _httpClient.GetAsync($"orchids?idSearch={item.OrchidId}");

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
                    catch (Exception)
                    {
                        // Handle the case where orchid cannot be fetched
                    }
                }

                CalculateTotalPrice();
            }
        }

        public async Task<IActionResult> OnPostUpdateQuantityAsync(int productId, int quantity)
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);

            if (!string.IsNullOrEmpty(cartJson))
            {
                var sessionCartItems = JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
                var item = sessionCartItems.FirstOrDefault(i => i.OrchidId == productId);

                if (item != null)
                {
                    if (quantity > 0)
                    {
                        item.Quantity = quantity;
                    }
                    else
                    {
                        sessionCartItems.Remove(item);
                    }

                    HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(sessionCartItems));
                }
            }

            return RedirectToPage();
        }

        public IActionResult OnPostRemoveItem(int productId)
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);

            if (!string.IsNullOrEmpty(cartJson))
            {
                var sessionCartItems = JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
                var item = sessionCartItems.FirstOrDefault(i => i.OrchidId == productId);

                if (item != null)
                {
                    sessionCartItems.Remove(item);
                    HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(sessionCartItems));
                }
            }

            return RedirectToPage();
        }

        public IActionResult OnPostClearCart()
        {
            HttpContext.Session.Remove(CartSessionKey);
            return RedirectToPage();
        }

        private void CalculateTotalPrice()
        {
            TotalPrice = CartItems.Sum(item => item.Product.Price * item.Quantity) ?? 0;
        }

    }
}