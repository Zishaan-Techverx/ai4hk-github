namespace TpaSodManagement.Services.Interfaces
{
    public interface ICurrentUserService
    {
        Task<long?> GetCurrentUserIdAsync();
    }
}

