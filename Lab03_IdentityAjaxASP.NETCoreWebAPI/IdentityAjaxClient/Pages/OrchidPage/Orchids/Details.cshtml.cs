using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessObjects;
using DataAccess.DTO.Order;
using IdentityAjaxClient.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace IdentityAjaxClient.Pages.OrchidPage.Orchids
{
    public class DetailsModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(IHttpClientFactory httpClientFactory, ILogger<DetailsModel> logger)
        {
            _httpClient = httpClientFactory.CreateClient("API");
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public Orchid? Orchid { get; set; }

        public int Id { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            try
            {
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

                Orchid = orchid;

                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error retrieving orchid: {ex.Message}");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int orchidId, int quantity = 1)
        {
            if (orchidId <= 0)
            {
                return BadRequest();
            }

            // Ensure quantity is at least 1
            quantity = Math.Max(1, quantity);

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
                    existingItem.Quantity += quantity;
                }
                else
                {
                    cart?.Add(new CartItem { OrchidId = orchidId, Quantity = quantity });
                }

                // Save cart
                HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));

                return new JsonResult(new
                {
                    success = true,
                    message = existingItem != null ? $"Item quantity increased by {quantity} in cart!" : $"Item added to cart with quantity {quantity}!",
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