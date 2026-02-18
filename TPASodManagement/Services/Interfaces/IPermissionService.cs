using Microsoft.AspNetCore.Identity;
using TpaSodManagement.Database.Entities;

namespace TpaSodManagement.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<List<Permission>> GetAllPermissionsAsync();
        Task<List<RolePermission>> GetRolePermissionsAsync(long roleId);
        Task<bool> UpdateRolePermissionsAsync(long roleId, List<int> grantedPermissionIds);
    }
}