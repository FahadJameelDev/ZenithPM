using System.Collections.Generic;

namespace ZenithPM.Web.Models.Entities
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int OrganizationId { get; set; }
        public int? ManagerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation properties
        public User? Manager { get; set; }
        public ICollection<Project>? Projects { get; set; }
    }
}