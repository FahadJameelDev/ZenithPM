using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtpNet;
using System.Security.Cryptography;
using ZenithPM.Web.Data;
using ZenithPM.Web.Models.Entities;
using ZenithPM.Web.Models.ViewModels;

namespace ZenithPM.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AuthController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            // Check if account is locked
            if (user.IsLocked && user.LockEndUtc > DateTime.UtcNow)
            {
                return RedirectToAction("AccountLocked");
            }

            // Verify password using BCrypt
            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= 5)
                {
                    user.IsLocked = true;
                    user.LockEndUtc = DateTime.UtcNow.AddMinutes(15);
                }
                await _context.SaveChangesAsync();

                // Log failed attempt
                await LogSecurityAction(user.Id, "FailedLogin");

                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            // Reset failed attempts on successful login
            user.FailedLoginAttempts = 0;
            user.IsLocked = false;
            user.LockEndUtc = null;
            await _context.SaveChangesAsync();

            // Check MFA
            if (user.IsMfaEnabled)
            {
                HttpContext.Session.SetInt32("MfaUserId", user.Id);
                return RedirectToAction("MfaVerify");
            }

            // Set session
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserEmail", user.Email);

            // Log successful login
            await LogSecurityAction(user.Id, "Login");

            return RedirectToAction("Index", "Dashboard");
        }

        // GET: Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userId))
            {
                await LogSecurityAction(int.Parse(userId), "Logout");
            }

            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // GET: ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: ForgotPassword
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                ViewBag.Message = "If an account exists, a password reset link has been sent.";
                return View();
            }

            // Generate token
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            // Store token in session for demo (in production: save to database)
            HttpContext.Session.SetString($"ResetToken_{model.Email}", token);

            return RedirectToAction("ResetPassword", new { email = model.Email, token = token });
        }

        // GET: ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };
            return View(model);
        }

        // POST: ResetPassword
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email.");
                return View(model);
            }

            // Verify token
            var storedToken = HttpContext.Session.GetString($"ResetToken_{model.Email}");
            if (storedToken != model.Token)
            {
                ModelState.AddModelError("", "Invalid token.");
                return View(model);
            }

            // Update password using BCrypt
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            user.PasswordChangedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Log password reset
            await LogSecurityAction(user.Id, "PasswordReset");

            ViewBag.Message = "Password has been reset successfully.";
            return RedirectToAction("Login");
        }

        // GET: MfaVerify
        [HttpGet]
        public IActionResult MfaVerify()
        {
            var userId = HttpContext.Session.GetInt32("MfaUserId");
            if (userId == null)
                return RedirectToAction("Login");

            return View();
        }

        // POST: MfaVerify
        [HttpPost]
        public async Task<IActionResult> MfaVerify(MfaViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = HttpContext.Session.GetInt32("MfaUserId");
            if (userId == null)
                return RedirectToAction("Login");

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null || string.IsNullOrEmpty(user.MfaSecretKey))
                return RedirectToAction("Login");

            // Verify OTP using OtpNet (TOTP)
            var totp = new Totp(Base32Encoding.ToBytes(user.MfaSecretKey));
            if (!totp.VerifyTotp(model.OTPCode, out _))
            {
                ModelState.AddModelError("", "Invalid OTP.");
                return View(model);
            }

            // Set session
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.Remove("MfaUserId");

            // Log MFA success
            await LogSecurityAction(user.Id, "MfaVerified");

            return RedirectToAction("Index", "Dashboard");
        }

        // GET: Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email already registered.");
                return View(model);
            }

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

            // Log registration
            await LogSecurityAction(user.Id, "Register");

            ViewBag.Message = "Registration successful. Please login.";
            return RedirectToAction("Login");
        }

        // GET: AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: AccountLocked
        [HttpGet]
        public IActionResult AccountLocked()
        {
            return View();
        }

        // Helper: Log security actions
        private async Task LogSecurityAction(int userId, string action)
        {
            var log = new SecurityLog
            {
                UserId = userId,
                ActionType = action,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                UserAgent = Request.Headers["User-Agent"].ToString() ?? "Unknown",
                Timestamp = DateTime.UtcNow
            };

            _context.SecurityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}