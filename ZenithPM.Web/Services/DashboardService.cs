using Microsoft.EntityFrameworkCore;
using ZenithPM.Web.Data;
using ZenithPM.Web.Models.ViewModels;

namespace ZenithPM.Web.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        // SuperAdmin Dashboard
        public async Task<SuperAdminDashboardViewModel> GetSuperAdminDashboardAsync()
        {
            var totalUsers = await _context.Users.CountAsync(u => !u.IsDeleted);
            var totalProjects = await _context.Projects.CountAsync(p => !p.IsDeleted);
            var totalTasks = await _context.Tasks.CountAsync(t => !t.IsDeleted);
            var totalDepartments = await _context.Departments.CountAsync(d => !d.IsDeleted);
            var totalOrganizations = 1; // Single org for now

            var recentActivities = await _context.SecurityLogs
                .OrderByDescending(s => s.Timestamp)
                .Take(5)
                .Select(s => new ActivityLogDto
                {
                    User = s.User != null ? $"{s.User.FirstName} {s.User.LastName}" : "Unknown",
                    Action = s.ActionType,
                    Timestamp = s.Timestamp
                })
                .ToListAsync();

            var systemStats = new SystemStatsDto
            {
                Revenue = 1250000m,
                Currency = "USD",
                Period = "Yearly",
                ActiveUsers = await _context.Users.CountAsync(u => !u.IsDeleted && u.IsLocked == false),
                TotalProjects = totalProjects
            };

            return new SuperAdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalProjects = totalProjects,
                TotalTasks = totalTasks,
                TotalDepartments = totalDepartments,
                TotalOrganizations = totalOrganizations,
                SecurityAlerts = 0,
                RecentActivities = recentActivities,
                SystemStats = systemStats
            };
        }

        // Admin Dashboard
        public async Task<AdminDashboardViewModel> GetAdminDashboardAsync(int organizationId)
        {
            var totalEmployees = await _context.Users.CountAsync(u => !u.IsDeleted && u.RoleId == 4);
            var totalDepartments = await _context.Departments.CountAsync(d => !d.IsDeleted && d.OrganizationId == organizationId);
            var totalProjects = await _context.Projects.CountAsync(p => !p.IsDeleted && p.OrganizationId == organizationId);
            var pendingTasks = await _context.Tasks.CountAsync(t => !t.IsDeleted && t.Status == "Pending");

            var recentProjects = await _context.Projects
                .Where(p => !p.IsDeleted && p.OrganizationId == organizationId)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => new ProjectSummaryDto
                {
                    Name = p.Name,
                    Status = p.Status ?? "Not Started",
                    Progress = p.Status == "Completed" ? 100 : p.Status == "InProgress" ? 50 : 0
                })
                .ToListAsync();

            var employeeStats = new EmployeeStatsDto
            {
                Active = await _context.Users.CountAsync(u => !u.IsDeleted && u.RoleId == 4 && u.IsLocked == false),
                OnLeave = 0,
                Total = totalEmployees
            };

            return new AdminDashboardViewModel
            {
                TotalEmployees = totalEmployees,
                TotalDepartments = totalDepartments,
                TotalProjects = totalProjects,
                PendingTasks = pendingTasks,
                RecentProjects = recentProjects,
                EmployeeStats = employeeStats
            };
        }

        // Manager Dashboard
        public async Task<ManagerDashboardViewModel> GetManagerDashboardAsync(int userId)
        {
            // Manager ki team (same department ke users)
            var manager = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var departmentId = await _context.Departments
                .Where(d => d.ManagerId == userId)
                .Select(d => d.Id)
                .FirstOrDefaultAsync();

            var teamSize = await _context.Users
                .CountAsync(u => !u.IsDeleted && u.RoleId == 4 && u.Role != null);

            var activeProjects = await _context.Projects
                .CountAsync(p => !p.IsDeleted && p.ManagerId == userId && p.Status != "Completed");

            var completedTasks = await _context.Tasks
                .CountAsync(t => !t.IsDeleted && t.AssignedToId == userId && t.Status == "Completed");

            var pendingApprovals = await _context.Tasks
                .CountAsync(t => !t.IsDeleted && t.AssignedToId == userId && t.Status == "Pending");

            var teamPerformance = await _context.Users
                .Where(u => !u.IsDeleted && u.RoleId == 4)
                .Select(u => new PerformanceDto
                {
                    User = $"{u.FirstName} {u.LastName}",
                    TasksCompleted = _context.Tasks.Count(t => t.AssignedToId == u.Id && t.Status == "Completed"),
                    Hours = 0
                })
                .ToListAsync();

            var upcomingDeadlines = await _context.Tasks
                .Where(t => !t.IsDeleted && t.AssignedToId == userId && t.DueDate > DateTime.UtcNow)
                .OrderBy(t => t.DueDate)
                .Take(5)
                .Select(t => new TaskDeadlineDto
                {
                    Title = t.Title,
                    DueDate = t.DueDate
                })
                .ToListAsync();

            return new ManagerDashboardViewModel
            {
                TeamSize = teamSize,
                ActiveProjects = activeProjects,
                CompletedTasks = completedTasks,
                PendingApprovals = pendingApprovals,
                TeamPerformance = teamPerformance,
                UpcomingDeadlines = upcomingDeadlines
            };
        }

        // Employee Dashboard
        public async Task<EmployeeDashboardViewModel> GetEmployeeDashboardAsync(int userId)
        {
            var today = DateTime.UtcNow.Date;

            var myTasks = await _context.Tasks
                .Where(t => !t.IsDeleted && t.AssignedToId == userId)
                .Select(t => new TaskDto
                {
                    Title = t.Title,
                    Status = t.Status ?? "Pending",
                    DueDate = t.DueDate
                })
                .ToListAsync();

            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Date == today);

            var upcomingDeadlines = await _context.Tasks
                .Where(t => !t.IsDeleted && t.AssignedToId == userId && t.DueDate > DateTime.UtcNow)
                .OrderBy(t => t.DueDate)
                .Take(5)
                .Select(t => new TaskDeadlineDto
                {
                    Title = t.Title,
                    DueDate = t.DueDate
                })
                .ToListAsync();

            return new EmployeeDashboardViewModel
            {
                MyTasks = myTasks,
                PendingTasks = myTasks.Count(t => t.Status == "Pending"),
                CompletedTasks = myTasks.Count(t => t.Status == "Completed"),
                TodayAttendance = attendance != null ? new AttendanceDto
                {
                    Status = attendance.Status ?? "Present",
                    CheckIn = attendance.CheckIn,
                    CheckOut = attendance.CheckOut
                } : new AttendanceDto { Status = "Not Marked" },
                UpcomingDeadlines = upcomingDeadlines,
                Notifications = new List<NotificationDto>()
            };
        }
    }
}