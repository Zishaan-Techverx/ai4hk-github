using Microsoft.AspNetCore.Identity;
using TpaSodManagement.Areas.Identity.Data;

namespace TpaSodManagement.Services.Interfaces
{
    public interface IAdminService
    {
        Task<List<IdentityRole>> GetAllRolesAsync();
        Task<List<TpaSodManagementUser>> GetAllUsersAsync();
        Task<(bool success, string message)> AssignRoleToUserAsync(string userId, string roleName);
        Task<(bool success, string message)> RemoveRoleFromUserAsync(string userId, string roleName);
        Task<(bool success, string message)> CreateRoleAsync(string roleName);
        Task<IdentityRole> GetRoleByIdAsync(string id);
        Task<List<TpaSodManagementUser>> GetUsersInRoleAsync(string roleName);
        Task<(bool success, string message)> UpdateRoleAsync(string id, string roleName);
        Task<(bool success, string message)> DeleteRoleAsync(string id);
        Task<List<string>> GetUserRolesAsync(string userId);
    }
}