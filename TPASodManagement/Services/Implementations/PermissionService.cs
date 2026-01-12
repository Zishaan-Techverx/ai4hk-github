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

        // PermissionService.cs

        public async Task<bool> UpdateRolePermissionsAsync(long roleId, List<RolePermission> permissions)
        {
            var existingPermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();

            _context.RolePermissions.RemoveRange(existingPermissions);
            await _context.SaveChangesAsync();

            foreach (var permission in permissions)
            {
                if (permission.IsActive)
                {
                    _context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = roleId,
                        PermissionId = permission.PermissionId,
                        IsActive = permission.IsActive 
                    });
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}