using System.Security.Claims;

namespace ZenithPM.Web.Security.Authentication
{
    public interface IAuthCookieManager
    {
        Task CreateSessionAsync(int userId, string email, string role);
        Task<string> GenerateAuthCookieAsync(int userId, string email, string role, List<Claim> claims);
        Task ClearAuthAsync();
    }
}