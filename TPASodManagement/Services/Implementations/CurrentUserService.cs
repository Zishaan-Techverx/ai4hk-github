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
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null || httpContext.User.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var user = await _userManager.GetUserAsync(httpContext.User);
            return user?.Id;
        }
    }
}

