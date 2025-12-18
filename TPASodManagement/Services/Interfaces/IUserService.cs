using TpaSodManagement.Areas.Identity.Data;

namespace TpaSodManagement.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<TpaSodManagementUser>> GetAllUsersAsync(string? organizationName = null);
        Task<TpaSodManagementUser> GetUserByIdAsync(string id);
        Task<(bool success, string message)> UpdateUserAsync(TpaSodManagementUser user);
        Task<(bool success, string message)> DeleteUserAsync(string id);
    }
}