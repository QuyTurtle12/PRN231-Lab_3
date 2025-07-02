using Microsoft.AspNetCore.Http;
using System.Net.Http;

namespace IdentityAjaxClient.Middleware
{
    public class JwtTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtTokenHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return await base.SendAsync(request, cancellationToken);

            // Try to get token from session first
            var token = context.Session.GetString("JWTToken");

            // If not in session, try to get from cookie
            if (string.IsNullOrEmpty(token))
            {
                token = context.Request.Cookies["JWTToken"];

                // If found in cookie, also set in session
                if (!string.IsNullOrEmpty(token))
                {
                    context.Session.SetString("JWTToken", token);
                }
            }

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}