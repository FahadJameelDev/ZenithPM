using System.Collections.Generic;

namespace ZenithPM.Web.Models.Entities
{
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public int OrganizationId { get; set; }

        // Navigation property
        public ICollection<RolePermission>? RolePermissions { get; set; }
    }
}