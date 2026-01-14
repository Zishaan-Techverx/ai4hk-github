namespace TpaSodManagement.Services.Interfaces
{
    public interface ICurrentUserService
    {
        /// <summary>
        /// Gets the ID of the current logged-in user
        /// </summary>
        /// <returns>The user ID if authenticated, null otherwise</returns>
        Task<long?> GetCurrentUserIdAsync();
    }
}

