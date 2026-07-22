using System.Collections.Generic;
using ZenithPM.Web.Models.Entities;

namespace ZenithPM.Web.Models.ViewModels
{
    public class UserManagementViewModel
    {
        public List<UserDto> Users { get; set; } = new();
        public List<RoleDto> Roles { get; set; } = new();
        public int SelectedRoleId { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}