using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ZenithPM.Web.Data;
using ZenithPM.Web.Models.Entities;
using ZenithPM.Web.Services;
using TaskEntity = ZenithPM.Web.Models.Entities.Task;

namespace ZenithPM.Web.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly ApplicationDbContext _context;

        public TaskController(ITaskService taskService, ApplicationDbContext context)
        {
            _taskService = taskService;
            _context = context;
        }

        [Authorize(Roles = "SuperAdmin,Admin,Manager")]
        public async System.Threading.Tasks.Task<IActionResult> Index()
        {
            var organizationId = 1;
            var tasks = await _taskService.GetAllTasksAsync(organizationId);
            return View(tasks);
        }

        [Authorize(Roles = "Employee")]
        public async System.Threading.Tasks.Task<IActionResult> MyTasks()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var tasks = await _taskService.GetTasksByUserAsync(userId);
            return View(tasks);
        }

        [Authorize(Roles = "SuperAdmin,Admin,Manager")]
        public async System.Threading.Tasks.Task<IActionResult> Create()
        {
            await LoadViewBags();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin,Manager")]
        public async System.Threading.Tasks.Task<IActionResult> Create(TaskEntity task)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                task.AssignedById = userId;
                await _taskService.CreateTaskAsync(task);
                TempData["Success"] = "Task created successfully.";
                return RedirectToAction(nameof(Index));
            }

            await LoadViewBags();
            return View(task);
        }

        [Authorize(Roles = "SuperAdmin,Admin,Manager")]
        public async System.Threading.Tasks.Task<IActionResult> Edit(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
                return NotFound();

            await LoadViewBags(task.ProjectId, task.AssignedToId);
            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin,Manager")]
        public async System.Threading.Tasks.Task<IActionResult> Edit(int id, TaskEntity task)
        {
            if (id != task.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                await _taskService.UpdateTaskAsync(task);
                TempData["Success"] = "Task updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            await LoadViewBags(task.ProjectId, task.AssignedToId);
            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin,Manager")]
        public async System.Threading.Tasks.Task<IActionResult> Delete(int id)
        {
            await _taskService.DeleteTaskAsync(id);
            TempData["Success"] = "Task deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async System.Threading.Tasks.Task LoadViewBags(int? selectedProjectId = null, int? selectedUserId = null)
        {
            ViewBag.Projects = new SelectList(
                await _context.Projects.Where(p => !p.IsDeleted).ToListAsync(),
                "Id",
                "Name",
                selectedProjectId
            );

            ViewBag.Users = new SelectList(
                await _context.Users.Where(u => !u.IsDeleted && u.RoleId == 4).ToListAsync(),
                "Id",
                "FirstName",
                selectedUserId
            );
        }
    }
}