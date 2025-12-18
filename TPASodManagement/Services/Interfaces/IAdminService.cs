using Microsoft.AspNetCore.Identity;
using TpaSodManagement.Areas.Identity.Data;

namespace TpaSodManagement.Services.Interfaces
{
    public interface IAdminService
    {
        Task<List<IdentityRole<long>>> GetAllRolesAsync();
        Task<List<TpaSodManagementUser>> GetAllUsersAsync(string? organizationName = null);
        Task<(bool success, string message)> AssignRoleToUserAsync(long userId, string roleName);
        Task<(bool success, string message)> RemoveRoleFromUserAsync(long userId, string roleName);
        Task<(bool success, string message)> CreateRoleAsync(string roleName);
        Task<IdentityRole<long>?> GetRoleByIdAsync(long id);
        Task<List<TpaSodManagementUser>> GetUsersInRoleAsync(string roleName);
        Task<(bool success, string message)> UpdateRoleAsync(long id, string roleName);
        Task<(bool success, string message)> DeleteRoleAsync(long id);
        Task<List<string>> GetUserRolesAsync(long userId);

        // ViewModel helpers to keep controllers thin
        Task<TpaSodManagement.ViewModels.Admin.AdminIndexViewModel> GetAdminIndexViewModelAsync(string? organizationName = null);
        Task<TpaSodManagement.ViewModels.Admin.EditRoleViewModel?> GetEditRoleViewModelAsync(long roleId);
        Task<TpaSodManagement.ViewModels.Admin.RoleDetailsViewModel?> GetRoleDetailsViewModelAsync(long roleId);
    }
}