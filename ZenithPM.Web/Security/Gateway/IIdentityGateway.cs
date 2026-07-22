using System.Security.Claims;
using System.Collections.Generic;
using ZenithPM.Web.Models.Entities;
using ZenithPM.Web.Models.Security;

// Alias lagana zaroori hai
using Task = System.Threading.Tasks.Task;

namespace ZenithPM.Web.Security.Gateway
{
    public interface IIdentityGateway
    {
        // 1. Validate Credentials
        Task<bool> ValidateCredentialsAsync(string email, string password);

        // 2. Verify Account Status (Locked, Deleted, etc.)
        Task<bool> VerifyAccountStatusAsync(string email);

        // 3. Verify Organization (if multi-tenant)
        Task<bool> VerifyOrganizationAsync(string email, int organizationId);

        // 4. Get User Role
        Task<RoleDto> GetUserRoleAsync(int userId);

        // 5. Load Permissions
        Task<List<PermissionDto>> LoadPermissionsAsync(int userId);

        // 6. Load Claims
        Task<List<Claim>> LoadClaimsAsync(int userId);

        // 7. Create Secure Session
        Task CreateSessionAsync(int userId);

        // 8. Generate Authentication Cookie
        Task<string> GenerateAuthCookieAsync(int userId);

        // 9. Redirect User
        string GetRedirectUrl(string roleName);

        // ----- EXTRA METHODS FOR CONTROLLER -----
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> IsEmailRegisteredAsync(string email);
        Task RegisterUserAsync(User user);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
        Task<bool> VerifyMfaAsync(string otpCode);
        Task LogoutAsync();
    }
}