using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Database;

namespace TpaSodManagement.Database.Seeders;

/// <summary>
/// Seeds default passwords for users that have a null PasswordHash.
/// Password format: (User's UserName)@123
/// Runs on application startup.
/// </summary>
public static class UserPasswordSeeder
{
    private const string PasswordSuffix = "@123";

    public static async Task SeedAsync(ApplicationDbContext context, UserManager<TpaSodManagementUser> userManager)
    {
        var usersWithNoPassword = await context.Users
            .Where(u => u.PasswordHash == null && !string.IsNullOrEmpty(u.UserName))
            .ToListAsync();

        foreach (var user in usersWithNoPassword)
        {
            var password = user.UserName! + PasswordSuffix;
            var result = await userManager.AddPasswordAsync(user, password);
            if (!result.Succeeded)
            {
                // Log but continue with other users; don't throw so app can start
                System.Diagnostics.Debug.WriteLine(
                    $"UserPasswordSeeder: Failed to set password for user '{user.UserName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}
