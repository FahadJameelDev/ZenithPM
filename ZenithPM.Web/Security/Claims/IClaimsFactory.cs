using System.Security.Claims;
using ZenithPM.Web.Models.Security;

namespace ZenithPM.Web.Security.Claims
{
    public interface IClaimsFactory
    {
        Task<RoleDto> GetRoleAsync(int userId);
        Task<List<PermissionDto>> GetPermissionsAsync(int userId);
        Task<List<Claim>> GenerateClaimsAsync(int userId);
    }
}