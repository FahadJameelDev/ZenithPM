using System.ComponentModel.DataAnnotations;

namespace ZenithPM.Web.Models.Entities
{
    public class RolePermission
    {
        [Key]
        public int Id { get; set; }  // <-- Add this line

        public int RoleId { get; set; }
        public int PermissionId { get; set; }

        // Navigation properties
        public Role? Role { get; set; }
        public Permission? Permission { get; set; }
    }
}