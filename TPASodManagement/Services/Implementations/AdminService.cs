using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly RoleManager<IdentityRole<long>> _roleManager;
        private readonly UserManager<TpaSodManagementUser> _userManager;
        private readonly ILogger<AdminService> _logger;

        public AdminService(
            RoleManager<IdentityRole<long>> roleManager,
            UserManager<TpaSodManagementUser> userManager,
            ILogger<AdminService> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }


        public async Task<List<IdentityRole<long>>> GetAllRolesAsync()
        {
            return await _roleManager.Roles.ToListAsync();
        }

        public async Task<List<TpaSodManagementUser>> GetAllUsersAsync()
        {
            // Get all users
            var allUsers = await _userManager.Users.ToListAsync();
            
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

        public async Task<IdentityRole<long>> GetRoleByIdAsync(long id)
        {
            return await _roleManager.FindByIdAsync(id.ToString());
        }

        public async Task<List<TpaSodManagementUser>> GetUsersInRoleAsync(string roleName)
        {
            return (await _userManager.GetUsersInRoleAsync(roleName)).ToList();
        }
        
        public async Task<List<string>> GetUserRolesAsync(long userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    return new List<string>();

                var roles = await _userManager.GetRolesAsync(user);
                return roles.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles for user: {UserId}", userId);
                return new List<string>();
            }
        }

        public async Task<(bool success, string message)> AssignRoleToUserAsync(long userId, string roleName)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    return (false, "User not found.");

                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                    return (false, $"Role '{roleName}' does not exist.");

                var currentRoles = await _userManager.GetRolesAsync(user);

                if (currentRoles.Contains(roleName))
                {
                    return (false, $"User '{user.UserName}' already has the role '{roleName}'.");
                }
                    
                if (currentRoles.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!removeResult.Succeeded)
                    {
                        _logger.LogError("Failed to remove existing roles for user {UserId}: {Errors}", userId, string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                        return (false, $"Failed to remove existing roles: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}");
                    }
                }

                var addResult = await _userManager.AddToRoleAsync(user, roleName);

                if (addResult.Succeeded)
                {
                    return (true, $"Role '{roleName}' successfully assigned to user '{user.UserName}'.");
                }
                
                _logger.LogError("Failed to assign new role '{RoleName}' to user {UserId}: {Errors}", roleName, userId, string.Join(", ", addResult.Errors.Select(e => e.Description)));
                return (false, string.Join(", ", addResult.Errors.Select(e => e.Description)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning single role to user {UserId}", userId);
                return (false, "An error occurred while assigning the role.");
            }
        }
        
        public async Task<(bool success, string message)> RemoveRoleFromUserAsync(long userId, string roleName)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    return (false, "User not found.");

                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                    return (false, $"Role '{roleName}' does not exist.");
                
                var isInRole = await _userManager.IsInRoleAsync(user, roleName);
                if (!isInRole)
                    return (false, $"User is not currently in the role '{roleName}'.");

                var result = await _userManager.RemoveFromRoleAsync(user, roleName);

                if (result.Succeeded)
                {
                    return (true, $"Role '{roleName}' successfully removed from user '{user.UserName}'.");
                }
                
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role '{RoleName}' from user '{UserId}'.", roleName, userId);
                return (false, "An error occurred while removing the role.");
            }
        }
        
        public async Task<(bool success, string message)> CreateRoleAsync(string roleName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleName))
                    return (false, "Role name is required!");

                var roleExist = await _roleManager.RoleExistsAsync(roleName.Trim());
                if (roleExist)
                    return (false, "Role already exists!");

                var result = await _roleManager.CreateAsync(new IdentityRole<long>(roleName.Trim()));

                return result.Succeeded
                    ? (true, "Role created successfully!")
                    : (false, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role: {RoleName}", roleName);
                return (false, "An error occurred while creating the role.");
            }
        }

        public async Task<(bool success, string message)> UpdateRoleAsync(long id, string roleName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleName))
                    return (false, "Role name is required!");

                var role = await _roleManager.FindByIdAsync(id.ToString());
                if (role == null)
                    return (false, "Role not found!");

                // Check if role is SuperAdmin
                if (role.Name.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    return (false, "SuperAdmin role cannot be edited.");
                }

                role.Name = roleName;
                var result = await _roleManager.UpdateAsync(role);

                return result.Succeeded
                    ? (true, "Role updated successfully!")
                    : (false, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role: {RoleId}", id);
                return (false, "An error occurred while updating the role.");
            }
        }

        public async Task<(bool success, string message)> DeleteRoleAsync(long id)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id.ToString());
                if (role == null)
                {
                    return (false, "Role not found.");
                }

                // Check if role is SuperAdmin
                if (role.Name.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    return (false, "SuperAdmin role cannot be deleted.");
                }

                var users = await _userManager.GetUsersInRoleAsync(role.Name);
                int usersRemovedCount = 0;

                foreach (var user in users)
                {
                    var removeResult = await _userManager.RemoveFromRoleAsync(user, role.Name);
                    if (!removeResult.Succeeded)
                    {
                        _logger.LogError("Failed to remove role '{RoleName}' from user '{UserEmail}' during deletion attempt.", role.Name, user.Email);
                        return (false, $"Failed to remove role from user {user.Email}. Role deletion cancelled.");
                    }
                    usersRemovedCount++;
                }

                var deleteResult = await _roleManager.DeleteAsync(role);

                if (deleteResult.Succeeded)
                {
                    return (true, $"Role '{role.Name}' successfully deleted and removed from {usersRemovedCount} user(s).");
                }
                else
                {
                    _logger.LogError("Error deleting role '{RoleName}': {Errors}", role.Name, string.Join(", ", deleteResult.Errors.Select(e => e.Description)));
                    return (false, $"Error deleting role: {string.Join(", ", deleteResult.Errors.Select(e => e.Description))}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role: {RoleId}", id);
                return (false, "An error occurred while deleting the role.");
            }
        }
    }
}