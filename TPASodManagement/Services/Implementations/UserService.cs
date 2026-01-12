using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.Database.Entities;
using TpaSodManagement.Database;

namespace TpaSodManagement.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly UserManager<TpaSodManagementUser> _userManager;
        private readonly ILogger<UserService> _logger;
        private readonly IRegistrationService _registrationService;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public UserService(
            UserManager<TpaSodManagementUser> userManager,
            ILogger<UserService> logger,
            IRegistrationService registrationService,
            ApplicationDbContext context,
            IEmailService emailService)
        {
            _userManager = userManager;
            _logger = logger;
            _registrationService = registrationService;
            _context = context;
            _emailService = emailService;
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

        public async Task<(bool success, string message)> ResetPasswordAsync(string userId, string? customPassword = null)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return (false, "User not found.");

                // Check if user has SuperAdmin role
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase))
                {
                    return (false, "SuperAdmin password cannot be reset.");
                }

                // Use custom password if provided, otherwise generate default: username@123
                string newPassword = !string.IsNullOrEmpty(customPassword) 
                    ? customPassword 
                    : $"{user.UserName}@123";

                // Check if user has a password
                bool hasPassword = await _userManager.HasPasswordAsync(user);

                if (hasPassword)
                {
                    // Remove existing password
                    var removeResult = await _userManager.RemovePasswordAsync(user);
                    if (!removeResult.Succeeded)
                    {
                        return (false, string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                    }
                }

                // Add new password
                var addResult = await _userManager.AddPasswordAsync(user, newPassword);
                if (addResult.Succeeded)
                {
                    _logger.LogInformation("Password reset for user {UserId} ({UserName})", user.Id, user.UserName);
                    
                    // Send email notification to user
                    if (!string.IsNullOrEmpty(user.Email))
                    {
                        try
                        {
                            _logger.LogInformation("Preparing to send password reset email to {Email} for user {UserName}", user.Email, user.UserName);
                            
                            // Get user's first name and last name from Person table
                            var person = await _registrationService.GetUserPersonAsync(user.Id.ToString());
                            var firstName = person?.FirstName ?? "";
                            var lastName = person?.LastName ?? "";
                            var fullName = string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName) 
                                ? (user.UserName ?? "User") 
                                : $"{firstName} {lastName}".Trim();
                            
                            var emailSubject = "Password Reset - TPA Sod Management System";
                            var emailBody = GeneratePasswordResetEmailBody(fullName, user.UserName ?? "User", newPassword);
                            var userName = user.UserName ?? user.Email;
                            
                            _logger.LogInformation("Calling SendEmailAsync for {Email}", user.Email);
                            
                            var emailSent = await _emailService.SendEmailAsync(
                                user.Email,
                                fullName,
                                emailSubject,
                                emailBody,
                                isHtml: true
                            );
                            
                            if (emailSent)
                            {
                                _logger.LogInformation("Password reset email sent successfully to {Email}", user.Email);
                            }
                            else
                            {
                                _logger.LogWarning("Failed to send password reset email to {Email}. Check email service logs for details.", user.Email);
                            }
                        }
                        catch (Exception emailEx)
                        {
                            // Log email error but don't fail the password reset
                            _logger.LogError(emailEx, "Exception occurred while sending password reset email to {Email}. Error: {ErrorMessage}, StackTrace: {StackTrace}", 
                                user.Email, emailEx.Message, emailEx.StackTrace);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("User {UserId} ({UserName}) does not have an email address. Cannot send password reset email.", user.Id, user.UserName);
                    }
                    
                    return (true, $"Password has been reset successfully. New password: {newPassword}");
                }
                
                return (false, string.Join(", ", addResult.Errors.Select(e => e.Description)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user: {UserId}", userId);
                return (false, "An error occurred while resetting the password.");
            }
        }

        private string GeneratePasswordResetEmailBody(string fullName, string userName, string newPassword)
        {
            return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='utf-8'>
            <style>
                body {{
                    font-family: Arial, sans-serif;
                    line-height: 1.6;
                    color: #333;
                    max-width: 600px;
                    margin: 0 auto;
                    padding: 20px;
                }}
                .container {{
                    background-color: #f8f9fa;
                    border: 1px solid #dee2e6;
                    border-radius: 5px;
                    padding: 30px;
                }}
                .header {{
                    background-color: #2B2B2B;
                    color: #ffffff;
                    padding: 20px;
                    text-align: center;
                    border-radius: 5px 5px 0 0;
                    margin: -30px -30px 20px -30px;
                }}
                .content {{
                    background-color: #ffffff;
                    padding: 20px;
                    border-radius: 5px;
                }}
                .password-box {{
                    background-color: #f8f9fa;
                    border: 2px solid #2B2B2B;
                    border-radius: 5px;
                    padding: 15px;
                    text-align: center;
                    margin: 20px 0;
                    font-size: 18px;
                    font-weight: bold;
                    color: #2B2B2B;
                }}
                .footer {{
                    margin-top: 20px;
                    padding-top: 20px;
                    border-top: 1px solid #dee2e6;
                    font-size: 12px;
                    color: #6c757d;
                    text-align: center;
                }}
                .warning {{
                    background-color: #fff3cd;
                    border-left: 4px solid #ffc107;
                    padding: 10px;
                    margin: 15px 0;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>TPA Sod Management System</h1>
                </div>
                <div class='content'>
                    <h2>Password Reset Notification</h2>
                    <p>Dear {fullName},</p>
                    <p>Your password has been reset by an administrator. Please use the following credentials to log in:</p>
                    
                    <div class='password-box'>
                        <strong>Username:</strong> {userName}<br>
                        <strong>New Password:</strong> {newPassword}
                    </div>
                    
                    <div class='warning'>
                        <strong>⚠️ Security Notice:</strong> For your security, please change your password after logging in.
                    </div>
                    
                    <p>If you did not request this password reset, please contact your system administrator immediately.</p>
                    
                    <p>Best regards,<br>
                    <strong>TPA Sod Management System</strong></p>
                </div>
                <div class='footer'>
                    <p>This is an automated message. Please do not reply to this email.</p>
                </div>
            </div>
        </body>
        </html>";
        }
    }
}