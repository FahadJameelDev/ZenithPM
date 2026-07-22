using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using OtpNet;
using ZenithPM.Web.Data;
using ZenithPM.Web.Models.Entities;
using ZenithPM.Web.Models.Security;
using ZenithPM.Web.Security.Authentication;
using ZenithPM.Web.Security.Claims;

// --- FIX: Alias banaya taake ambiguity khatam ho ---
using Task = System.Threading.Tasks.Task;
using TaskEntity = ZenithPM.Web.Models.Entities.Task;
// ---------------------------------------------------

namespace ZenithPM.Web.Security.Gateway
{
    public class IdentityGateway : IIdentityGateway
    {
        private readonly ApplicationDbContext _context;
        private readonly IClaimsFactory _claimsFactory;
        private readonly IAuthCookieManager _cookieManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IdentityGateway(
            ApplicationDbContext context,
            IClaimsFactory claimsFactory,
            IAuthCookieManager cookieManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _claimsFactory = claimsFactory;
            _cookieManager = cookieManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> ValidateCredentialsAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;

            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }

        public async Task<bool> VerifyAccountStatusAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;

            if (user.IsLocked && user.LockEndUtc > DateTime.UtcNow) return false;
            if (user.IsDeleted) return false;

            return true;
        }

        public async Task<bool> VerifyOrganizationAsync(string email, int organizationId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user?.Role == null) return false;
            return user.Role.OrganizationId == organizationId;
        }

        public async Task<RoleDto> GetUserRoleAsync(int userId)
        {
            return await _claimsFactory.GetRoleAsync(userId);
        }

        public async Task<List<PermissionDto>> LoadPermissionsAsync(int userId)
        {
            return await _claimsFactory.GetPermissionsAsync(userId);
        }

        public async Task<List<Claim>> LoadClaimsAsync(int userId)
        {
            var role = await GetUserRoleAsync(userId);
            var permissions = await LoadPermissionsAsync(userId);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role.Name),  // <-- Role Claim Added
                new Claim("OrganizationId", role.OrganizationId.ToString())
            };

            foreach (var permission in permissions)
            {
                if (permission.IsGranted)
                    claims.Add(new Claim("Permission", permission.Name));
            }

            return claims;
        }

        public async Task CreateSessionAsync(int userId)
        {
            var role = await GetUserRoleAsync(userId);
            var user = await _context.Users.FindAsync(userId);

            if (user != null)
            {
                await _cookieManager.CreateSessionAsync(userId, user.Email, role.Name);
            }
        }

        public async Task<string> GenerateAuthCookieAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return "User not found";

            var role = await GetUserRoleAsync(userId);
            var claims = await LoadClaimsAsync(userId);

            return await _cookieManager.GenerateAuthCookieAsync(userId, user.Email, role.Name, claims);
        }

        public string GetRedirectUrl(string roleName)
        {
            return roleName?.ToLower() switch
            {
                "superadmin" => "/Dashboard/SuperAdmin",
                "admin" => "/Dashboard/Admin",
                "manager" => "/Dashboard/Manager",
                "employee" => "/Dashboard/Employee",
                _ => "/Dashboard/Index"
            };
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> IsEmailRegisteredAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task RegisterUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            _httpContextAccessor.HttpContext?.Session.SetString($"ResetToken_{email}", token);
            return true;
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;

            var storedToken = _httpContextAccessor.HttpContext?.Session.GetString($"ResetToken_{email}");
            if (storedToken != token) return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.PasswordChangedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> VerifyMfaAsync(string otpCode)
        {
            var userId = _httpContextAccessor.HttpContext?.Session.GetInt32("MfaUserId");
            if (userId == null) return false;

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null || string.IsNullOrEmpty(user.MfaSecretKey)) return false;

            var totp = new Totp(Base32Encoding.ToBytes(user.MfaSecretKey));
            if (!totp.VerifyTotp(otpCode, out _)) return false;

            await _cookieManager.CreateSessionAsync(user.Id, user.Email, "Employee");
            _httpContextAccessor.HttpContext?.Session.Remove("MfaUserId");
            return true;
        }

        public async Task LogoutAsync()
        {
            await _cookieManager.ClearAuthAsync();
        }
    }
}