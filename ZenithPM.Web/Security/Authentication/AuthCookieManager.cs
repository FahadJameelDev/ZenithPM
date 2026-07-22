using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ZenithPM.Web.Security.Authentication
{
    public class AuthCookieManager : IAuthCookieManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthCookieManager(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task CreateSessionAsync(int userId, string email, string role)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                session.SetString("UserId", userId.ToString());
                session.SetString("UserEmail", email);
                session.SetString("UserRole", role);
            }
            await Task.CompletedTask;
        }

        public async Task<string> GenerateAuthCookieAsync(int userId, string email, string role, List<Claim> claims)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return "Cookie generation failed";

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

            return "Authentication cookie generated successfully";
        }

        public async Task ClearAuthAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Session?.Clear();
                await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        }
    }
}