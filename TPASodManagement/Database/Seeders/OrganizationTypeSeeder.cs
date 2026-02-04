using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Database.Entities;

namespace TpaSodManagement.Database.Seeders;

public static class OrganizationTypeSeeder
{
    private static readonly string[] OrganizationTypeNames = { "TPA", "RTF_Sod", "RTF_HGT_Sod", "HGT_Sod" };

    public static async Task SeedAsync(ApplicationDbContext context, UserManager<TpaSodManagementUser> userManager)
    {
        if (await context.OrganizationTypes.AnyAsync())
            return;

        var createdDate = DateTimeOffset.UtcNow;
        long? createdByUserId = null;

        var superAdminUsers = await userManager.GetUsersInRoleAsync("SuperAdmin");
        var superAdmin = superAdminUsers.FirstOrDefault();
        if (superAdmin != null)
            createdByUserId = superAdmin.Id;

        var organizationTypes = OrganizationTypeNames.Select(name => new OrganizationType
        {
            OrganizationTypeName = name,
            CreatedDate = createdDate,
            CreatedByUserId = createdByUserId
        }).ToList();

        await context.OrganizationTypes.AddRangeAsync(organizationTypes);
        await context.SaveChangesAsync();
    }
}
