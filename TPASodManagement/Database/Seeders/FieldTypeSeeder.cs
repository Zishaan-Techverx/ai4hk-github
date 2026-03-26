using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Database.Entities;

namespace TpaSodManagement.Database.Seeders;

public static class FieldTypeSeeder
{
    private static readonly string[] FieldTypeNames = { "RTF_Sod", "RTF_HGT_Sod", "HGT_Sod" };

    public static async Task SeedAsync(ApplicationDbContext context, UserManager<TpaSodManagementUser> userManager)
    {
        if (await context.FieldTypes.AnyAsync())
            return;

        var createdDate = DateTimeOffset.UtcNow;
        long? createdByUserId = null;

        var superAdminUsers = await userManager.GetUsersInRoleAsync("SuperAdmin");
        var superAdmin = superAdminUsers.FirstOrDefault();
        if (superAdmin != null)
            createdByUserId = superAdmin.Id;

        var fieldTypes = FieldTypeNames.Select(name => new FieldType
        {
            FieldTypeName = name,
            CreatedDate = createdDate,
            CreatedByUserId = createdByUserId
        }).ToList();

        await context.FieldTypes.AddRangeAsync(fieldTypes);
        await context.SaveChangesAsync();
    }
}
