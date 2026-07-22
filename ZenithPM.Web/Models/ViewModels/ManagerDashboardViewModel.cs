using System;
using System.Collections.Generic;

namespace ZenithPM.Web.Models.ViewModels
{
    public class ManagerDashboardViewModel
    {
        public int TeamSize { get; set; }
        public int ActiveProjects { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingApprovals { get; set; }
        public List<PerformanceDto> TeamPerformance { get; set; } = new();
        public List<TaskDeadlineDto> UpcomingDeadlines { get; set; } = new();
    }

    public class PerformanceDto
    {
        public string User { get; set; } = string.Empty;
        public int TasksCompleted { get; set; }
        public double Hours { get; set; }
    }

    public class TaskDeadlineDto
    {
        public string Title { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
    }
}