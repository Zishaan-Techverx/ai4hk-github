namespace TpaSodManagement.Services.Interfaces
{
    public interface ICurrentUserService
    {
        Task<long?> GetCurrentUserIdAsync();
        Task<long?> GetCurrentUserOrganizationIdAsync();
        Task<bool> IsCurrentUserSuperAdminAsync();
    }
}

