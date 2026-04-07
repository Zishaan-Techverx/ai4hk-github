using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Database;

namespace TpaSodManagement.Database.Seeders;

/// <summary>
/// Transitional backfill seeder for Organization -> Farm mapping.
/// Uses deterministic mapping: first active farm by FarmId for each OrganizationId.
/// Safe to run multiple times.
/// </summary>
public static class OrganizationEntityFarmBackfillSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Backfill AspNetUsers.FarmId from AspNetUsers.OrganizationId
        var usersSql = @"
IF COL_LENGTH('AspNetUsers', 'OrganizationId') IS NULL OR COL_LENGTH('AspNetUsers', 'FarmId') IS NULL
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

        await context.Database.ExecuteSqlRawAsync(usersSql);

        // Backfill Customers.FarmId only when column exists (schema-later compatibility).
        var customersSql = @"
IF COL_LENGTH('Customers', 'OrganizationId') IS NULL OR COL_LENGTH('Customers', 'FarmId') IS NULL
    RETURN;

UPDATE c
SET c.FarmId = fmatch.FarmId
FROM Customers c
OUTER APPLY (
    SELECT TOP (1) f.FarmId
    FROM Farms f
    WHERE f.OrganizationId = c.OrganizationId
      AND f.DeletedDate IS NULL
    ORDER BY f.FarmId
) fmatch
WHERE c.OrganizationId IS NOT NULL
  AND c.FarmId IS NULL
  AND fmatch.FarmId IS NOT NULL;";

        await context.Database.ExecuteSqlRawAsync(customersSql);
    }
}
