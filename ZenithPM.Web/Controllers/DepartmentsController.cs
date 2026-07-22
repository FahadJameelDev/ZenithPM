using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ZenithPM.Web.Data;
using ZenithPM.Web.Models.Entities;

namespace ZenithPM.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class DepartmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Departments
        public async Task<IActionResult> Index()
        {
            var departments = await _context.Departments
                .Include(d => d.Manager)
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.Name)
                .ToListAsync();
            return View(departments);
        }

        // GET: Departments/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Managers = new SelectList(
                await _context.Users.Where(u => !u.IsDeleted && u.RoleId <= 3).ToListAsync(),
                "Id",
                "FirstName"
            );
            return View();
        }

        // POST: Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            if (ModelState.IsValid)
            {
                department.OrganizationId = 1;
                department.CreatedAt = DateTime.UtcNow;
                department.IsDeleted = false;
                _context.Departments.Add(department);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Department created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Managers = new SelectList(
                await _context.Users.Where(u => !u.IsDeleted && u.RoleId <= 3).ToListAsync(),
                "Id",
                "FirstName",
                department.ManagerId
            );
            return View(department);
        }

        // GET: Departments/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Manager)
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

            if (department == null)
                return NotFound();

            ViewBag.Managers = new SelectList(
                await _context.Users.Where(u => !u.IsDeleted && u.RoleId <= 3).ToListAsync(),
                "Id",
                "FirstName",
                department.ManagerId
            );
            return View(department);
        }

        // POST: Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Department department)
        {
            if (id != department.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var existing = await _context.Departments.FindAsync(id);
                if (existing == null)
                    return NotFound();

                existing.Name = department.Name;
                existing.Description = department.Description;
                existing.ManagerId = department.ManagerId;
                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Department updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Managers = new SelectList(
                await _context.Users.Where(u => !u.IsDeleted && u.RoleId <= 3).ToListAsync(),
                "Id",
                "FirstName",
                department.ManagerId
            );
            return View(department);
        }

        // POST: Departments/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                department.IsDeleted = true;
                department.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Department deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}