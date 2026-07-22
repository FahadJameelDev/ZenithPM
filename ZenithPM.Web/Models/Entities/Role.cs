using System.Collections.Generic;

namespace ZenithPM.Web.Models.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int OrganizationId { get; set; }

        // Navigation properties
        public ICollection<RolePermission>? RolePermissions { get; set; }
        public ICollection<User>? Users { get; set; }
    }
}