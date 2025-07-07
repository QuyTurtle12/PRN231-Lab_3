using BusinessObjects;
using DataAccess.Constant.Enum;
using IdentityAjaxClient.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IdentityAjaxClient.Pages.OrderPage.Management
{
    public class IndexModel : BasePageModel
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IHttpClientFactory httpClientFactory, ILogger<IndexModel> logger)
        {
            _httpClient = httpClientFactory.CreateClient("API");
            _logger = logger;
        }

        public PaginationDTO<Order> Orders { get; set; } = default!;
        public SelectList? OrderStatusList { get; set; }

        // Search and filter properties
        [BindProperty(SupportsGet = true)]
        public OrderStatusEnum? OrderStatusSearch { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? CustomerSearch { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDateSearch { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDateSearch { get; set; }

        public async Task<IActionResult> OnGetAsync(
            int pageIndex = 1,
            int pageSize = 10)
        {
            try
            {
                // Get current user role from session
                var userRole = HttpContext.Session.GetString("UserRole");

                bool isStaff = !string.IsNullOrEmpty(userRole) &&
                    (userRole.Equals("Staff", StringComparison.OrdinalIgnoreCase));

                // Check authorization
                if (!isStaff)
                {
                    TempData["ErrorMessage"] = "You are not authorized to access order management.";
                    return RedirectToPage("/Index");
                }

                // Create order status dropdown from enum
                var statusValues = Enum.GetValues<OrderStatusEnum>().ToList();
                var selectListItems = new List<SelectListItem>
                {
                    new SelectListItem("All", "")
                };

                selectListItems.AddRange(statusValues.Select(s =>
                    new SelectListItem(s.ToString(), ((int)s).ToString())));

                OrderStatusList = new SelectList(selectListItems, "Value", "Text",
                                    OrderStatusSearch.HasValue ? ((int)OrderStatusSearch.Value).ToString() : "");


                // Build query string for orders
                var query = $"orders?pageIndex={pageIndex}&pageSize={pageSize}";


                if (OrderStatusSearch.HasValue)
                {
                    query += $"&status={(int)OrderStatusSearch.Value}";
                }

                if (!string.IsNullOrWhiteSpace(CustomerSearch))
                {
                    query += $"&customerSearch={Uri.EscapeDataString(CustomerSearch)}";
                }

                if (StartDateSearch.HasValue)
                {
                    query += $"&fromDate={StartDateSearch.Value:yyyy-MM-dd}";
                }

                if (EndDateSearch.HasValue)
                {
                    query += $"&toDate={EndDateSearch.Value:yyyy-MM-dd}";
                }

                // Fetch orders with pagination and filtering
                var response = await _httpClient.GetFromJsonAsync<PaginationDTO<Order>>(query);

                if (response != null)
                {
                    Orders = response;
                }
                else
                {
                    Orders = new PaginationDTO<Order>
                    {
                        Items = new List<Order>(),
                        PageNumber = 1,
                        PageSize = 10,
                        TotalCount = 0,
                        TotalPages = 0
                    };
                    ModelState.AddModelError(string.Empty, "No orders found.");
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading orders");
                Orders = new PaginationDTO<Order>
                {
                    Items = new List<Order>(),
                    PageNumber = 1,
                    PageSize = 10,
                    TotalCount = 0,
                    TotalPages = 0
                };
                ModelState.AddModelError(string.Empty, "Error loading orders: " + ex.Message);
                return Page();
            }
        }
    }
}