using Microsoft.AspNetCore.Identity;
using TpaSodManagement.Models;

namespace TpaSodManagement.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<List<Permission>> GetAllPermissionsAsync();
        Task<List<RolePermission>> GetRolePermissionsAsync(string roleId);
        Task<bool> UpdateRolePermissionsAsync(string roleId, List<RolePermission> permissions);
    }
}