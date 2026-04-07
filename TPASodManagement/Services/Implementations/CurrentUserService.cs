using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Services.Implementations
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<TpaSodManagementUser> _userManager;

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            UserManager<TpaSodManagementUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<long?> GetCurrentUserIdAsync()
        {
            var user = await GetCurrentUserAsync();
            return user?.Id;
        }

        public async Task<long?> GetCurrentUserOrganizationIdAsync()
        {
            var user = await GetCurrentUserAsync();
            return user?.OrganizationId;
        }

        public async Task<long?> GetCurrentUserFarmIdAsync()
        {
            var user = await GetCurrentUserAsync();
            return user?.FarmId;
        }

        public async Task<long?> GetCurrentUserPersonIdAsync()
        {
            var user = await GetCurrentUserAsync();
            return user?.PersonId;
        }

        public async Task<bool> IsCurrentUserSuperAdminAsync()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return false;
            var roles = await _userManager.GetRolesAsync(user);
            return roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase);
        }

        private async Task<TpaSodManagementUser?> GetCurrentUserAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null || httpContext.User.Identity?.IsAuthenticated != true)
                return null;
            return await _userManager.GetUserAsync(httpContext.User);
        }
    }
}

