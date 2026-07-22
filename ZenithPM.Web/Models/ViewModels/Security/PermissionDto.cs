namespace ZenithPM.Web.Models.Security
{
    public class PermissionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Module { get; set; }
        public bool IsGranted { get; set; }
        public int OrganizationId { get; set; }
    }
}