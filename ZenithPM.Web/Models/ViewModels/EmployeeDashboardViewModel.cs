using System;
using System.Collections.Generic;

namespace ZenithPM.Web.Models.ViewModels
{
    public class EmployeeDashboardViewModel
    {
        public List<TaskDto> MyTasks { get; set; } = new();
        public int PendingTasks { get; set; }
        public int CompletedTasks { get; set; }
        public AttendanceDto TodayAttendance { get; set; } = new();
        public List<TaskDeadlineDto> UpcomingDeadlines { get; set; } = new();
        public List<NotificationDto> Notifications { get; set; } = new();
    }

    public class TaskDto
    {
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
    }

    public class AttendanceDto
    {
        public string Status { get; set; } = string.Empty;
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
    }

    public class NotificationDto
    {
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}