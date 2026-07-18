using ZenithPM.Web.Models.ViewModels;

namespace ZenithPM.Web.Services
{
    // Interface for Auth Service (Contracts)
    public interface IAuthService
    {
        Task<bool> LoginAsync(LoginViewModel model);
        Task<bool> RegisterAsync(RegisterViewModel model);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPasswordViewModel model);
        Task<bool> VerifyMfaAsync(MfaViewModel model);
        Task LogoutAsync(int userId);
        Task<bool> IsAccountLockedAsync(string email);
    }
}