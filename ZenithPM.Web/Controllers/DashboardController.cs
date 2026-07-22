using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZenithPM.Web.Services;

namespace ZenithPM.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // GET: Dashboard/SuperAdmin
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> SuperAdmin()
        {
            var model = await _dashboardService.GetSuperAdminDashboardAsync();
            return View(model);
        }

        // GET: Dashboard/Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin()
        {
            var orgId = GetOrganizationId();
            var model = await _dashboardService.GetAdminDashboardAsync(orgId);
            return View(model);
        }

        // GET: Dashboard/Manager
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Manager()
        {
            var userId = GetCurrentUserId();
            var model = await _dashboardService.GetManagerDashboardAsync(userId);
            return View(model);
        }

        // GET: Dashboard/Employee
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Employee()
        {
            var userId = GetCurrentUserId();
            var model = await _dashboardService.GetEmployeeDashboardAsync(userId);
            return View(model);
        }

        // Helper: Get current user ID from claims
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User ID not found in claims.");

            return int.Parse(userIdClaim);
        }

        // Helper: Get organization ID from claims
        private int GetOrganizationId()
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgIdClaim))
                return 1; // Default organization

            return int.Parse(orgIdClaim);
        }
    }
}