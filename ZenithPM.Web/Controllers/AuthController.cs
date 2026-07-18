using Microsoft.AspNetCore.Mvc;
using ZenithPM.Web.Models.ViewModels;
using ZenithPM.Web.Services; // Service Layer access

namespace ZenithPM.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        // Dependency Injection through Constructor
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // GET: Login Page
        [HttpGet]
        public IActionResult Login() => View();

        // POST: Login Logic
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Check if account is locked
            if (await _authService.IsAccountLockedAsync(model.Email))
                return RedirectToAction("AccountLocked");

            // Attempt login via Service
            var success = await _authService.LoginAsync(model);
            if (!success)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            // If MFA is enabled, redirect to MFA verification
            var mfaUserId = HttpContext.Session.GetInt32("MfaUserId");
            if (mfaUserId != null) return RedirectToAction("MfaVerify");

            return RedirectToAction("Index", "Dashboard");
        }

        // GET: Register Page
        [HttpGet]
        public IActionResult Register() => View();

        // POST: Register Logic
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var success = await _authService.RegisterAsync(model);
            if (!success)
            {
                ModelState.AddModelError("Email", "Email already registered.");
                return View(model);
            }

            ViewBag.Message = "Registration successful. Please login.";
            return RedirectToAction("Login");
        }

        // GET: Forgot Password Page
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        // POST: Forgot Password Logic
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            await _authService.ForgotPasswordAsync(model.Email);
            ViewBag.Message = "If an account exists, a reset link has been processed.";
            return View();
        }

        // GET: Reset Password Page
        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            var model = new ResetPasswordViewModel { Email = email, Token = token };
            return View(model);
        }

        // POST: Reset Password Logic
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (await _authService.ResetPasswordAsync(model))
            {
                ViewBag.Message = "Password reset successful.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", "Invalid token or email.");
            return View(model);
        }

        // GET: MFA Verification Page
        [HttpGet]
        public IActionResult MfaVerify() => View();

        // POST: MFA Verification Logic
        [HttpPost]
        public async Task<IActionResult> MfaVerify(MfaViewModel model)
        {
            if (await _authService.VerifyMfaAsync(model))
                return RedirectToAction("Index", "Dashboard");

            ModelState.AddModelError("", "Invalid OTP.");
            return View(model);
        }

        // GET: Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userId))
                await _authService.LogoutAsync(int.Parse(userId));

            return RedirectToAction("Login");
        }

        // Access Denied Page
        [HttpGet]
        public IActionResult AccessDenied() => View();

        // Account Locked Page
        [HttpGet]
        public IActionResult AccountLocked() => View();
    }
}