using System;

namespace ZenithPM.Web.Models.Entities
{
    public class Task
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ProjectId { get; set; }
        public int? AssignedToId { get; set; }
        public int? AssignedById { get; set; }
        public DateTime DueDate { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
        // public int? MilestoneId { get; set; }  // Commented - Milestone entity nahi hai
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation properties
        public Project? Project { get; set; }
        public User? AssignedTo { get; set; }
        public User? AssignedBy { get; set; }
        // public Milestone? Milestone { get; set; }  // Commented
    }
}