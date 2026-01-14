using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Services;

namespace TpaSodManagement.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<TpaSodManagementUser>> GetAllUsersAsync(long? organizationId = null);
        Task<TpaSodManagementUser?> GetUserByIdAsync(string id);
        Task<(bool success, string message)> UpdateUserAsync(TpaSodManagementUser user);
        Task<(bool success, string message)> DeleteUserAsync(string id);
        Task<(bool success, string message)> ResetPasswordAsync(string userId, string? customPassword = null);
        Task<ServiceResponse<List<TpaSodManagementUser>>> GetFilteredAsync(Dictionary<string, string> filters, long? organizationId = null);
    }
}