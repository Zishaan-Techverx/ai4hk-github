using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
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
            if (user?.FarmId != null)
                return user.FarmId;

            // Fallback: try FarmId claim if user lookup fails but principal exists.
            var farmIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("FarmId")?.Value;
            if (long.TryParse(farmIdClaim, out var farmId))
                return farmId;

            return null;
        }

        public async Task<long?> GetCurrentUserPersonIdAsync()
        {
            var user = await GetCurrentUserAsync();
            return user?.PersonId;
        }

        public async Task<bool> IsCurrentUserSuperAdminAsync()
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                return roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase);
            }

            // Fallback: if user entity lookup fails, trust role claims from current principal.
            var principal = _httpContextAccessor.HttpContext?.User;
            if (principal?.Identity?.IsAuthenticated == true)
            {
                if (principal.IsInRole("SuperAdmin"))
                    return true;

                var roleClaims = principal.FindAll(ClaimTypes.Role).Select(c => c.Value);
                return roleClaims.Any(r => string.Equals(r, "SuperAdmin", StringComparison.OrdinalIgnoreCase));
            }

            return false;
        }

        private async Task<TpaSodManagementUser?> GetCurrentUserAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null || httpContext.User.Identity?.IsAuthenticated != true)
                return null;

            // Primary path from current principal.
            var user = await _userManager.GetUserAsync(httpContext.User);
            if (user != null)
                return user;

            // Fallback 1: resolve by NameIdentifier claim.
            var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userIdClaim))
            {
                var byId = await _userManager.FindByIdAsync(userIdClaim);
                if (byId != null)
                    return byId;
            }

            // Fallback 2: resolve by username claim.
            var userName = httpContext.User.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(userName))
            {
                var byName = await _userManager.FindByNameAsync(userName);
                if (byName != null)
                    return byName;
            }

            return null;
        }
    }
}

