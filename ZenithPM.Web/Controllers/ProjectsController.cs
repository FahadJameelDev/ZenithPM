using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ZenithPM.Web.Data;
using ZenithPM.Web.Models.Entities;
using ZenithPM.Web.Services;

namespace ZenithPM.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,Manager")]
    public class ProjectController : Controller
    {
        private readonly IProjectService _projectService;
        private readonly ApplicationDbContext _context;

        public ProjectController(IProjectService projectService, ApplicationDbContext context)
        {
            _projectService = projectService;
            _context = context;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var organizationId = 1; // Get from session/claim
            var projects = await _projectService.GetAllProjectsAsync(organizationId);
            return View("Index", projects);
        }

        // GET: Projects/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Managers = new SelectList(await _context.Users.Where(u => !u.IsDeleted && u.RoleId <= 3).ToListAsync(), "Id", "FirstName");
            ViewBag.Departments = new SelectList(await _context.Departments.Where(d => !d.IsDeleted).ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project project)
        {
            if (ModelState.IsValid)
            {
                project.OrganizationId = 1;
                await _projectService.CreateProjectAsync(project);
                TempData["Success"] = "Project created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Managers = new SelectList(await _context.Users.Where(u => !u.IsDeleted && u.RoleId <= 3).ToListAsync(), "Id", "FirstName");
            ViewBag.Departments = new SelectList(await _context.Departments.Where(d => !d.IsDeleted).ToListAsync(), "Id", "Name");
            return View(project);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
                return NotFound();

            ViewBag.Managers = new SelectList(await _context.Users.Where(u => !u.IsDeleted && u.RoleId <= 3).ToListAsync(), "Id", "FirstName", project.ManagerId);
            ViewBag.Departments = new SelectList(await _context.Departments.Where(d => !d.IsDeleted).ToListAsync(), "Id", "Name", project.DepartmentId);
            return View(project);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Project project)
        {
            if (id != project.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                await _projectService.UpdateProjectAsync(project);
                TempData["Success"] = "Project updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Managers = new SelectList(await _context.Users.Where(u => !u.IsDeleted && u.RoleId <= 3).ToListAsync(), "Id", "FirstName", project.ManagerId);
            ViewBag.Departments = new SelectList(await _context.Departments.Where(d => !d.IsDeleted).ToListAsync(), "Id", "Name", project.DepartmentId);
            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _projectService.DeleteProjectAsync(id);
            TempData["Success"] = "Project deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}