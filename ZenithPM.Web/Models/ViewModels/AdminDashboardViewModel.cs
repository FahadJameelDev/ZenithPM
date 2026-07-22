using System;
using System.Collections.Generic;

namespace ZenithPM.Web.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalEmployees { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalProjects { get; set; }
        public int PendingTasks { get; set; }
        public List<ProjectSummaryDto> RecentProjects { get; set; } = new();
        public EmployeeStatsDto EmployeeStats { get; set; } = new();
    }

    public class ProjectSummaryDto
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Progress { get; set; }
    }

    public class EmployeeStatsDto
    {
        public int Active { get; set; }
        public int OnLeave { get; set; }
        public int Total { get; set; }
    }
}