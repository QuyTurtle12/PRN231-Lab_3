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
                TempData["ErrorMessage"] = "Invalid orchid ID.";
                return RedirectToPage("/Index");
            }

            try
            {
                // Fetch orchid details
                var orchidResponse = await _httpClient.GetAsync($"orchids?idSearch={id}");
                if (!orchidResponse.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Failed to retrieve orchid details.";
                    return RedirectToPage("/Index");
                }

                var orchidPaginatedList = await orchidResponse.Content.ReadFromJsonAsync<PaginationDTO<Orchid>>();

                Orchid? orchid = orchidPaginatedList?.Items.FirstOrDefault();

                if (orchid == null)
                {
                    TempData["ErrorMessage"] = "Orchid not found.";
                    return RedirectToPage("/Index");
                }

                Orchid = orchid;

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orchid details");
                TempData["ErrorMessage"] = $"Error retrieving orchid: {ex.Message}";
                return RedirectToPage("/Index");
            }
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int orchidId, int quantity = 1)
        {
            if (orchidId <= 0)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Invalid orchid ID."
                });
            }

            // Ensure quantity is at least 1
            quantity = Math.Max(1, quantity);

            try
            {
                // Get user ID from session
                var userId = HttpContext.Session.GetString("UserId");

                if (string.IsNullOrEmpty(userId))
                {
                    // Return JSON response for AJAX handling
                    return new JsonResult(new
                    {
                        success = false,
                        message = "Please log in to add items to your cart.",
                        requireLogin = true
                    });
                }

                // Fetch the orchid from API to get its details
                var orchidResponse = await _httpClient.GetAsync($"orchids?idSearch={orchidId}");
                if (!orchidResponse.IsSuccessStatusCode)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "Could not find the specified orchid."
                    });
                }

                var orchidPaginatedList = await orchidResponse.Content.ReadFromJsonAsync<PaginationDTO<Orchid>>();

                Orchid? orchid = orchidPaginatedList?.Items.FirstOrDefault();

                if (orchid == null)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "Could not find the specified orchid."
                    });
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
                    message = existingItem != null 
                        ? $"{orchid.OrchidName} quantity increased by {quantity} in your cart!" 
                        : $"{orchid.OrchidName} added to your cart with quantity {quantity}!",
                    cartCount = cart?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart");
                return new JsonResult(new
                {
                    success = false,
                    message = $"Error adding item to cart: {ex.Message}"
                });
            }
        }
    }
}