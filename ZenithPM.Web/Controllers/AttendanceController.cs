using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZenithPM.Web.Models.Entities;
using ZenithPM.Web.Services;

namespace ZenithPM.Web.Controllers
{
    [Authorize]
    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        [Authorize(Roles = "SuperAdmin,Admin,Manager")]
        public async System.Threading.Tasks.Task<IActionResult> Index()
        {
            var attendances = await _attendanceService.GetAllAttendanceAsync(1);
            return View(attendances);
        }

        [Authorize(Roles = "Employee")]
        public async System.Threading.Tasks.Task<IActionResult> Mark()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var today = System.DateTime.UtcNow.Date;

            var existing = await _attendanceService.GetAttendanceByDateAsync(userId, today);
            if (existing != null)
            {
                ViewBag.AlreadyMarked = true;
                ViewBag.Status = existing.Status;
                ViewBag.CheckIn = existing.CheckIn?.ToString("hh:mm tt");
                ViewBag.CheckOut = existing.CheckOut?.ToString("hh:mm tt");
                return View(existing);
            }

            return View(new Attendance { UserId = userId, Date = today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Employee")]
        public async System.Threading.Tasks.Task<IActionResult> Mark(Attendance attendance)
        {
            if (ModelState.IsValid)
            {
                var existing = await _attendanceService.GetAttendanceByDateAsync(attendance.UserId, attendance.Date);
                if (existing != null)
                {
                    TempData["Error"] = "Attendance already marked for today.";
                    return RedirectToAction(nameof(Mark));
                }

                await _attendanceService.CreateAttendanceAsync(attendance);
                TempData["Success"] = "Attendance marked successfully.";
                return RedirectToAction(nameof(Mark));
            }

            return View(attendance);
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        public async System.Threading.Tasks.Task<IActionResult> Edit(int id)
        {
            var attendance = await _attendanceService.GetAttendanceByIdAsync(id);
            if (attendance == null)
                return NotFound();

            return View(attendance);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async System.Threading.Tasks.Task<IActionResult> Edit(int id, Attendance attendance)
        {
            if (id != attendance.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                await _attendanceService.UpdateAttendanceAsync(attendance);
                TempData["Success"] = "Attendance updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(attendance);
        }
    }
}