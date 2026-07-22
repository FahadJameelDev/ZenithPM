using Microsoft.AspNetCore.Mvc;
using ZenithPM.Web.Models.Entities;
using ZenithPM.Web.Models.ViewModels;
using ZenithPM.Web.Security.Gateway;

namespace ZenithPM.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IIdentityGateway _identityGateway;

        public AuthController(IIdentityGateway identityGateway)
        {
            _identityGateway = identityGateway;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF Protection Added
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var isValid = await _identityGateway.ValidateCredentialsAsync(model.Email, model.Password);
            if (!isValid)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            var isActive = await _identityGateway.VerifyAccountStatusAsync(model.Email);
            if (!isActive) return RedirectToAction("AccountLocked");

            var user = await _identityGateway.GetUserByEmailAsync(model.Email);
            if (user == null) return View(model);

            // Session & Cookie Setup
            await _identityGateway.CreateSessionAsync(user.Id);
            var cookieValue = await _identityGateway.GenerateAuthCookieAsync(user.Id);

            Response.Cookies.Append("ZenithAuth", cookieValue, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            var role = await _identityGateway.GetUserRoleAsync(user.Id);
            return Redirect(_identityGateway.GetRedirectUrl(role.Name));
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (await _identityGateway.IsEmailRegisteredAsync(model.Email))
            {
                ModelState.AddModelError("Email", "Email already registered.");
                return View(model);
            }

            var user = new User
            {
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                RoleId = 4, // Default Employee
                CreatedAt = DateTime.UtcNow
            };

            await _identityGateway.RegisterUserAsync(user);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _identityGateway.LogoutAsync();
            Response.Cookies.Delete("ZenithAuth"); // Clear Cookie
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MfaVerify(MfaViewModel model)
        {
            var result = await _identityGateway.VerifyMfaAsync(model.OTPCode);
            if (!result)
            {
                ModelState.AddModelError("", "Invalid OTP.");
                return View(model);
            }

            // Get user ID from session to determine role and redirect
            var userId = HttpContext.Session.GetInt32("MfaUserId") ?? 0;
            var role = await _identityGateway.GetUserRoleAsync(userId);

            return Redirect(_identityGateway.GetRedirectUrl(role.Name));
        }

        // Add Anti-Forgery for all sensitive POST actions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            await _identityGateway.ForgotPasswordAsync(model.Email);
            ViewBag.Message = "If account exists, reset instructions sent.";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var result = await _identityGateway.ResetPasswordAsync(model.Email, model.Token, model.NewPassword);
            if (!result) { ModelState.AddModelError("", "Invalid token."); return View(model); }
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => View();
        public IActionResult AccountLocked() => View();
    }
}