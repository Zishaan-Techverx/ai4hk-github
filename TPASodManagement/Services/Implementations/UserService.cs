using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.Models.Db;

namespace TpaSodManagement.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly UserManager<TpaSodManagementUser> _userManager;
        private readonly ILogger<UserService> _logger;
        private readonly IRegistrationService _registrationService;
        private readonly SodDbContext _context;

        public UserService(
            UserManager<TpaSodManagementUser> userManager,
            ILogger<UserService> logger,
            IRegistrationService registrationService,
            SodDbContext context)
        {
            _userManager = userManager;
            _logger = logger;
            _registrationService = registrationService;
            _context = context;
        }

        public async Task<List<TpaSodManagementUser>> GetAllUsersAsync(long? organizationId = null)
        {
            // Get all users
            var allUsers = await _userManager.Users
                .Include(u => u.Organization)
                .ToListAsync();
            
            // Filter by organization if provided
            if (organizationId.HasValue)
            {
                allUsers = allUsers
                    .Where(u => u.OrganizationId == organizationId.Value)
                    .ToList();
            }
            
            // Filter out users with SuperAdmin role
            var filteredUsers = new List<TpaSodManagementUser>();
            
            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                // Exclude users with SuperAdmin role
                if (!roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase))
                {
                    filteredUsers.Add(user);
                }
            }
            
            return filteredUsers;
        }

        public async Task<TpaSodManagementUser?> GetUserByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        // Helper method to generate username (same logic as registration)
        private async Task<string> GenerateUsernameAsync(Organization organization, string firstName, long? excludeUserId = null)
        {
            string rawOrgName = organization?.OrganizationName?.Replace(" ", "") ?? "";
            const int OrgPrefixLength = 5;
            string orgPrefix = rawOrgName.Length >= OrgPrefixLength
                               ? rawOrgName.Substring(0, OrgPrefixLength)
                               : rawOrgName;

            orgPrefix = orgPrefix.ToUpper();

            string userInitials;
            string[] nameParts = firstName?.Split(' ', System.StringSplitOptions.RemoveEmptyEntries) ?? new string[0];

            if (nameParts.Length >= 2)
            {
                userInitials = (nameParts[0][0].ToString() + nameParts[^1][0].ToString()).ToUpper();
            }
            else if (!string.IsNullOrEmpty(firstName) && firstName.Length >= 2)
            {
                userInitials = firstName.Substring(0, 2).ToUpper();
            }
            else if (!string.IsNullOrEmpty(firstName))
            {
                userInitials = firstName.ToUpper();
            }
            else
            {
                userInitials = "XX"; // Fallback if no first name
            }

            string baseUsername = $"{orgPrefix}-{userInitials}";
            string finalUsername = baseUsername;
            int counter = 1;

            // Check if username exists (excluding current user)
            while (true)
            {
                var existingUser = await _userManager.FindByNameAsync(finalUsername);
                if (existingUser == null)
                {
                    // Username is available
                    break;
                }
                // If it's the same user, we can reuse the username
                if (excludeUserId.HasValue && existingUser.Id == excludeUserId.Value)
                {
                    break;
                }
                // Otherwise, try next variation
                finalUsername = $"{baseUsername}{counter++}";
            }

            return finalUsername;
        }

        public async Task<(bool success, string message)> UpdateUserAsync(TpaSodManagementUser user)
        {
            try
            {
                var existingUser = await _userManager.FindByIdAsync(user.Id.ToString());
                if (existingUser == null)
                    return (false, "User not found.");

                // Store existing roles before update (to preserve them)
                var existingRoles = await _userManager.GetRolesAsync(existingUser);

                // Check if OrganizationId changed
                bool organizationChanged = existingUser.OrganizationId != user.OrganizationId;
                
                // Check if Email changed
                bool emailChanged = !string.Equals(existingUser.Email, user.Email, StringComparison.OrdinalIgnoreCase);
                
                // Store original username if we need to regenerate
                string originalUsername = existingUser.UserName ?? string.Empty;

                // Update only remaining fields:
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.IsActive = user.IsActive;
                existingUser.PrimaryContact = user.PrimaryContact;
                existingUser.OrganizationId = user.OrganizationId;

                // Update NormalizedEmail if Email changed
                if (emailChanged)
                {
                    existingUser.NormalizedEmail = user.Email?.ToUpperInvariant();
                }

                // Regenerate username if OrganizationId changed - Get FirstName from Person table and Organization
                if (organizationChanged && user.OrganizationId.HasValue)
                {
                    // Get Person to get FirstName
                    var person = await _registrationService.GetUserPersonAsync(user.Id.ToString());
                    var firstName = person?.FirstName ?? "";
                    
                    // Get Organization to get OrganizationName for username generation
                    var organization = await _context.Organizations
                        .FirstOrDefaultAsync(o => o.OrganizationId == user.OrganizationId.Value);
                    if (organization != null)
                    {
                        var newUsername = await GenerateUsernameAsync(organization, firstName, user.Id);
                        existingUser.UserName = newUsername;
                        existingUser.NormalizedUserName = newUsername?.ToUpperInvariant();
                        _logger.LogInformation("Username regenerated for user {UserId}: {OldUsername} -> {NewUsername}", 
                            user.Id, originalUsername, newUsername);
                    }
                }

                // Update user
                var result = await _userManager.UpdateAsync(existingUser);
                if (!result.Succeeded)
                {
                    return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                // Roles are automatically preserved by Identity when updating user
                // But to be safe, verify roles are still there
                var rolesAfterUpdate = await _userManager.GetRolesAsync(existingUser);
                
                // Log if roles were preserved
                if (existingRoles.Any())
                {
                    _logger.LogInformation("User {UserId} roles preserved: {Roles}", user.Id, string.Join(", ", rolesAfterUpdate));
                }

                return (true, "User updated successfully." + (organizationChanged ? " Username has been regenerated." : ""));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", user.Id);
                return (false, "An error occurred while updating the user.");
            }
        }

        public async Task<(bool success, string message)> DeleteUserAsync(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return (false, "User not found.");

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                    return (true, "User deleted successfully.");
                
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", id);
                return (false, "An error occurred while deleting the user.");
            }
        }
    }
}