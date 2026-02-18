using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Database;
using TpaSodManagement.Database.Entities;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Services.Implementations
{
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole<long>> _roleManager;
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(
            ApplicationDbContext context,
            RoleManager<IdentityRole<long>> roleManager,
            ILogger<PermissionService> logger)
        {
            _context = context;
            _roleManager = roleManager;
            _logger = logger;
        }

        // SIRF YEH 3 METHODS RAHENGI
        public async Task<List<Permission>> GetAllPermissionsAsync()
        {
            return await _context.Permissions.ToListAsync();
        }

        public async Task<List<RolePermission>> GetRolePermissionsAsync(long roleId)
        {
            return await _context.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();
        }

        public async Task<bool> UpdateRolePermissionsAsync(long roleId, List<int> grantedPermissionIds)
        {
            var existing = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();

            _context.RolePermissions.RemoveRange(existing);
            await _context.SaveChangesAsync();

            var toAdd = (grantedPermissionIds ?? new List<int>())
                .Distinct()
                .Select(pid => new RolePermission { RoleId = roleId, PermissionId = pid });

            _context.RolePermissions.AddRange(toAdd);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}