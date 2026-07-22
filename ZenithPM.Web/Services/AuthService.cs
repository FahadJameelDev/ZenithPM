using Microsoft.EntityFrameworkCore;
using ZenithPM.Web.Data;
using ZenithPM.Web.Models.Entities;
using ZenithPM.Web.Models.ViewModels;
using BCrypt.Net;
using OtpNet;
using System.Security.Cryptography;
using System.Threading.Tasks;

// Alias lagana zaroori hai taake Task class aur Async Task mein farq rahe
using Task = System.Threading.Tasks.Task;
using TaskEntity = ZenithPM.Web.Models.Entities.Task;

namespace ZenithPM.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> LoginAsync(LoginViewModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null) return false;

            if (user.IsLocked && user.LockEndUtc > DateTime.UtcNow)
                return false;

            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= 5)
                {
                    user.IsLocked = true;
                    user.LockEndUtc = DateTime.UtcNow.AddMinutes(15);
                }
                await _context.SaveChangesAsync();
                await LogSecurityAction(user.Id, "FailedLogin");
                return false;
            }

            user.FailedLoginAttempts = 0;
            user.IsLocked = false;
            user.LockEndUtc = null;
            await _context.SaveChangesAsync();

            if (user.IsMfaEnabled)
            {
                _httpContextAccessor.HttpContext?.Session?.SetInt32("MfaUserId", user.Id);
                return true;
            }

            SetSession(user);
            await LogSecurityAction(user.Id, "Login");
            return true;
        }

        public async Task<bool> RegisterAsync(RegisterViewModel model)
        {
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                return false;

            var user = new User
            {
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                FirstName = model.FirstName,
                LastName = model.LastName,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false,
                IsMfaEnabled = false,
                FailedLoginAttempts = 0,
                IsLocked = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            await LogSecurityAction(user.Id, "Register");
            return true;
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            _httpContextAccessor.HttpContext?.Session?.SetString($"ResetToken_{email}", token);
            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null) return false;

            var storedToken = _httpContextAccessor.HttpContext?.Session?.GetString($"ResetToken_{model.Email}");
            if (storedToken != model.Token) return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            user.PasswordChangedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await LogSecurityAction(user.Id, "PasswordReset");
            return true;
        }

        public async Task<bool> VerifyMfaAsync(MfaViewModel model)
        {
            var userId = _httpContextAccessor.HttpContext?.Session?.GetInt32("MfaUserId");
            if (userId == null) return false;

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null || string.IsNullOrEmpty(user.MfaSecretKey)) return false;

            var totp = new Totp(Base32Encoding.ToBytes(user.MfaSecretKey));
            if (!totp.VerifyTotp(model.OTPCode, out _)) return false;

            SetSession(user);
            _httpContextAccessor.HttpContext?.Session?.Remove("MfaUserId");
            await LogSecurityAction(user.Id, "MfaVerified");
            return true;
        }

        public async Task LogoutAsync(int userId)
        {
            await LogSecurityAction(userId, "Logout");
            _httpContextAccessor.HttpContext?.Session?.Clear();
        }

        public async Task<bool> IsAccountLockedAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user != null && user.IsLocked && user.LockEndUtc > DateTime.UtcNow;
        }

        private void SetSession(User user)
        {
            _httpContextAccessor.HttpContext?.Session?.SetString("UserId", user.Id.ToString());
            _httpContextAccessor.HttpContext?.Session?.SetString("UserEmail", user.Email);
        }

        private async Task LogSecurityAction(int userId, string action)
        {
            var log = new SecurityLog
            {
                UserId = userId,
                ActionType = action,
                IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown",
                UserAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown",
                Timestamp = DateTime.UtcNow
            };
            _context.SecurityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}