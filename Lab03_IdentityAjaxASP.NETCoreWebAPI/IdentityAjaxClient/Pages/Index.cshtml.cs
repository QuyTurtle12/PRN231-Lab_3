using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using BusinessObjects;
using IdentityAjaxClient.Model;

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
    }
}