using System;
using System.Collections.Generic;

namespace ZenithPM.Web.Models.ViewModels
{
    public class SuperAdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalProjects { get; set; }
        public int TotalTasks { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalOrganizations { get; set; }
        public int SecurityAlerts { get; set; }
        public List<ActivityLogDto> RecentActivities { get; set; } = new();
        public SystemStatsDto SystemStats { get; set; } = new();
    }

    public class ActivityLogDto
    {
        public string User { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class SystemStatsDto
    {
        public decimal Revenue { get; set; }
        public string Currency { get; set; } = "USD";
        public string Period { get; set; } = "Monthly";
        public int ActiveUsers { get; set; }
        public int TotalProjects { get; set; }
    }
}