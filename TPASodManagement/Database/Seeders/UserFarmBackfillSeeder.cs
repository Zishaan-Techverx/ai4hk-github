using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Database;

namespace TpaSodManagement.Database.Seeders;

/// <summary>
/// Transitional seeder:
/// Assigns FarmId to users based on their OrganizationId by selecting
/// the first non-deleted farm for that organization.
/// Safe to run multiple times (updates only users with null FarmId).
/// </summary>
public static class UserFarmBackfillSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // When OrganizationId column is removed in future migration, skip safely.
        var sql = @"
IF COL_LENGTH('AspNetUsers', 'OrganizationId') IS NULL
    RETURN;

UPDATE u
SET u.FarmId = fmatch.FarmId
FROM AspNetUsers u
OUTER APPLY (
    SELECT TOP (1) f.FarmId
    FROM Farms f
    WHERE f.OrganizationId = u.OrganizationId
      AND f.DeletedDate IS NULL
    ORDER BY f.FarmId
) fmatch
WHERE u.OrganizationId IS NOT NULL
  AND u.FarmId IS NULL
  AND fmatch.FarmId IS NOT NULL;";

        await context.Database.ExecuteSqlRawAsync(sql);
    }
}

