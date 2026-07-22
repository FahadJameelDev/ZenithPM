using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ZenithPM.Web.Data;
using ZenithPM.Web.Models.Security;

namespace ZenithPM.Web.Security.Claims
{
    public class ClaimsFactory : IClaimsFactory
    {
        private readonly ApplicationDbContext _context;

        public ClaimsFactory(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RoleDto> GetRoleAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Role == null)
            {
                return new RoleDto
                {
                    Id = 4,
                    Name = "Employee",
                    Description = "Default role",
                    OrganizationId = 1,
                    Permissions = new List<string>()
                };
            }

            return new RoleDto
            {
                Id = user.Role.Id,
                Name = user.Role.Name,
                Description = user.Role.Description,
                OrganizationId = user.Role.OrganizationId,
                Permissions = new List<string>()
            };
        }

        public async Task<List<PermissionDto>> GetPermissionsAsync(int userId)
        {
            // Fix: Use SelectMany or separate query to avoid ThenInclude nullability issues
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Role == null)
            {
                return new List<PermissionDto>();
            }

            // Load permissions separately
            var rolePermissions = await _context.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => rp.RoleId == user.Role.Id)
                .ToListAsync();

            if (rolePermissions == null || !rolePermissions.Any())
            {
                return new List<PermissionDto>();
            }

            return rolePermissions
                .Where(rp => rp.Permission != null)
                .Select(rp => new PermissionDto
                {
                    Id = rp.Permission!.Id,
                    Name = rp.Permission.Name,
                    Module = rp.Permission.Module,
                    IsGranted = true,
                    OrganizationId = rp.Permission.OrganizationId
                })
                .ToList();
        }

        public async Task<List<Claim>> GenerateClaimsAsync(int userId)
        {
            var role = await GetRoleAsync(userId);
            var permissions = await GetPermissionsAsync(userId);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role.Name),
                new Claim("OrganizationId", role.OrganizationId.ToString())
            };

            foreach (var permission in permissions)
            {
                if (permission.IsGranted)
                {
                    claims.Add(new Claim("Permission", permission.Name));
                }
            }

            return claims;
        }
    }
}