using BusinessObjects;
using DataAccess.Paginated;
using IdentityAjaxClient.Model;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityAjaxClient.Pages.OrchidPage.Management
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public PaginatedList<Orchid> Orchid { get; set; } = default!;
        public string? NameSort { get; set; }
        public string? NameSearch { get; set; }
        public string? CategorySearch { get; set; }
        public bool? IsNatural { get; set; }

        public async Task OnGetAsync(
            int pageIndex = 1,
            int pageSize = 1,
            string? nameSearch = null,
            string? categorySearch = null,
            bool? isNatural = null)
        {
            NameSearch = nameSearch;
            CategorySearch = categorySearch;
            IsNatural = isNatural;

            var query = $"orchids?pageIndex={pageIndex}&pageSize={pageSize}";

            if (!string.IsNullOrWhiteSpace(nameSearch))
            {
                query += $"&nameSearch={Uri.EscapeDataString(nameSearch)}";
            }

            if (!string.IsNullOrWhiteSpace(categorySearch))
            {
                query += $"&categorySearch={Uri.EscapeDataString(categorySearch)}";
            }

            if (isNatural.HasValue)
            {
                query += $"&isNatural={isNatural.Value}";
            }

            try
            {
                var response = await _httpClient.GetFromJsonAsync<PaginationDTO<Orchid>>(query)
                    ?? throw new Exception("Failed to fetch orchids");

                Orchid = new PaginatedList<Orchid>(
                        response.Items,
                        response.TotalCount,
                        response.PageNumber,
                        response.PageSize
                    );
            }
            catch (Exception ex)
            {
                Orchid = new PaginatedList<Orchid>(new List<Orchid>(), 0, 1, 10);
                ModelState.AddModelError(string.Empty, "Error loading orchids: " + ex.Message);
            }
        }
    }
}