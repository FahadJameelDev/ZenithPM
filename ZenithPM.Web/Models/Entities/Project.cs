using System;
using System.Collections.Generic;

namespace ZenithPM.Web.Models.Entities
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ClientName { get; set; }
        public string? ProjectCategory { get; set; }
        public decimal? Budget { get; set; }
        public string? Priority { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }
        public int OrganizationId { get; set; }
        public int? ManagerId { get; set; }
        public int? DepartmentId { get; set; }  // FK to Department
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation properties
        public User? Manager { get; set; }
        public Department? Department { get; set; }
        public ICollection<Task>? Tasks { get; set; }
    }
}