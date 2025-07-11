﻿using BusinessObjects;
using DataAccess.Paginated;
using IdentityAjaxClient.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IdentityAjaxClient.Pages.OrchidPage.Management
{
    public class IndexModel : BasePageModel
    {
        private readonly HttpClient _httpClient;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public PaginationDTO<Orchid> Orchid { get; set; } = default!;
        public SelectList? CategoryList { get; set; }
        public string? NameSort { get; set; }
        public string? NameSearch { get; set; }
        public string? CategoryIdSearch { get; set; }
        public bool? IsNatural { get; set; }

        public async Task<IActionResult> OnGetAsync(
            int pageIndex = 1,
            int pageSize = 9,
            string? nameSearch = null,
            string? categoryIdSearch = null,
            bool? isNatural = null)
        {
            // Get current user role from session
            var userRole = HttpContext.Session.GetString("UserRole");

            bool isStaff = !string.IsNullOrEmpty(userRole) &&
                userRole.Equals("Staff", StringComparison.OrdinalIgnoreCase);

            // Check authorization
            if (!isStaff)
            {
                return RedirectToPage("/Index");
            }

            NameSearch = nameSearch;
            CategoryIdSearch = categoryIdSearch;
            IsNatural = isNatural;

            try
            {
                var categories = await _httpClient.GetFromJsonAsync<PaginationDTO<Category>>("categories");

                CategoryList = new SelectList(categories!.Items.ToList(), "CategoryId", "CategoryName", categoryIdSearch);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error loading categories: " + ex.Message);
            }

            var query = $"orchids?pageIndex={pageIndex}&pageSize={pageSize}";

            if (!string.IsNullOrWhiteSpace(nameSearch))
            {
                query += $"&nameSearch={Uri.EscapeDataString(nameSearch)}";
            }

            if (!string.IsNullOrWhiteSpace(categoryIdSearch))
            {
                query += $"&categoryIdSearch={Uri.EscapeDataString(categoryIdSearch)}";
            }

            if (isNatural.HasValue)
            {
                query += $"&isNatural={isNatural.Value}";
            }

            try
            {
                var response = await _httpClient.GetFromJsonAsync<PaginationDTO<Orchid>>(query)
                    ?? throw new Exception("Failed to fetch orchids");

                Orchid = response;
            }
            catch (Exception ex)
            {
                Orchid = new PaginationDTO<Orchid>
                {
                    Items = new List<Orchid>(),
                    PageNumber = 1,
                    PageSize = 10,
                    TotalCount = 0,
                    TotalPages = 0
                };
                ModelState.AddModelError(string.Empty, "Error loading orchids: " + ex.Message);
            }

            return Page();
        }
    }
}