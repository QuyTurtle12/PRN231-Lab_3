using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityAjaxClient.Pages.AuthPage
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Clear session
            HttpContext.Session.Clear();

            // Clear authentication cookie
            Response.Cookies.Delete("JWTToken");

            return RedirectToPage("/AuthPage/Login");
        }
    }
}
