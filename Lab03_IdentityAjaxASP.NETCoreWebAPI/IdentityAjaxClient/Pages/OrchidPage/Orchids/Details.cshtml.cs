using System.Threading.Tasks;
using BusinessObjects;
using DataAccess.DTO.Order;
using IdentityAjaxClient.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityAjaxClient.Pages.OrchidPage.Orchids
{
    public class DetailsModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public DetailsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
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

        public async Task<IActionResult> OnPostAddToCartAsync()
        {
            if (Id <= 0)
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
                    return RedirectToPage("/Account/Login");
                }

                // Create cart item
                var cartItem = new CartItem
                {
                    OrchidId = Id,
                    UserId = int.Parse(userId),
                    Quantity = 1
                };

                // Send to API
                var response = await _httpClient.PostAsJsonAsync("cart", cartItem);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Item added to cart successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to add item to cart.";
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error adding to cart: {ex.Message}");
                return RedirectToPage();
            }
        }
    }
}