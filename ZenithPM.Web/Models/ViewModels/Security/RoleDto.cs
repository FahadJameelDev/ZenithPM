namespace ZenithPM.Web.Models.Security
{
    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int OrganizationId { get; set; }
        public List<string>? Permissions { get; set; }
    }
}