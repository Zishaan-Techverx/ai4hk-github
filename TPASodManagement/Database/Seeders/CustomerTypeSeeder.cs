using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Database;
using TpaSodManagement.Database.Entities;

namespace TpaSodManagement.Database.Seeders;

/// <summary>
/// Seeds CustomerType with HGTSod, RTFHGTSod, RTFSod. Only inserts those that are missing.
/// </summary>
public static class CustomerTypeSeeder
{
    public static readonly string[] CustomerTypeNames = { "HGTSod", "RTFHGTSod", "RTFSod" };

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        var existingNames = await context.CustomerTypes
            .Where(ct => ct.DeletedDate == null)
            .Select(ct => ct.CustomerTypeName)
            .ToListAsync();

        var existingSet = new HashSet<string>(existingNames, StringComparer.OrdinalIgnoreCase);
        var toAdd = CustomerTypeNames
            .Where(name => !existingSet.Contains(name))
            .ToList();

        if (toAdd.Count == 0)
            return;

        var createdDate = DateTimeOffset.UtcNow;
        var customerTypes = toAdd.Select(name => new CustomerType
        {
            CustomerTypeName = name,
            CreatedDate = createdDate
        }).ToList();

        await context.CustomerTypes.AddRangeAsync(customerTypes);
        await context.SaveChangesAsync();
    }
}
