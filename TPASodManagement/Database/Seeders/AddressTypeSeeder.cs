using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Database;
using TpaSodManagement.Database.Entities;

namespace TpaSodManagement.Database.Seeders;

/// <summary>
/// Seeds the three address types used by Farm, Customer, and Organization modules.
/// Only inserts those that are missing in the database.
/// </summary>
public static class AddressTypeSeeder
{
    /// <summary>
    /// Display names used in Farm, Customer, and Organization address forms.
    /// Use this list when loading address type dropdowns in those modules so only these 3 are shown.
    /// </summary>
    public static readonly string[] ModuleAddressTypeNames = { "Farm", "Customer", "Organization" };

    /// <summary>
    /// Returns address types for Farm, Customer, and Organization modules (only the 3 seeded types).
    /// Use this when populating the address form dropdown in Customer, Farm, or Organization controllers.
    /// </summary>
    public static async Task<List<AddressType>> GetAddressTypesForEntityModulesAsync(ApplicationDbContext context)
    {
        return await context.AddressTypes
            .Where(at => at.DeletedDate == null && at.IsActive && ModuleAddressTypeNames.Contains(at.AddressTypeName))
            .OrderBy(at => at.AddressTypeName)
            .ToListAsync();
    }

    /// <summary>
    /// Returns the AddressTypeId for the given type name (Farm, Customer, or Organization), or null if not found.
    /// </summary>
    public static async Task<int?> GetAddressTypeIdByNameAsync(ApplicationDbContext context, string addressTypeName)
    {
        if (string.IsNullOrWhiteSpace(addressTypeName))
            return null;
        var at = await context.AddressTypes
            .AsNoTracking()
            .Where(x => x.DeletedDate == null && x.IsActive && x.AddressTypeName == addressTypeName)
            .Select(x => x.AddressTypeId)
            .FirstOrDefaultAsync();
        return at == 0 ? null : at;
    }

    /// <summary>
    /// Returns SelectListItems for the address type dropdown in Farm, Customer, Organization address forms.
    /// </summary>
    public static async Task<List<SelectListItem>> GetAddressTypeSelectListAsync(ApplicationDbContext context, int? selectedId = null)
    {
        var types = await GetAddressTypesForEntityModulesAsync(context);
        return types.Select(at => new SelectListItem
        {
            Value = at.AddressTypeId.ToString(),
            Text = at.AddressTypeName ?? "",
            Selected = selectedId.HasValue && at.AddressTypeId == selectedId.Value
        }).ToList();
    }

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        var existingNames = await context.AddressTypes
            .Where(at => at.DeletedDate == null)
            .Select(at => at.AddressTypeName)
            .ToListAsync();

        var existingSet = new HashSet<string>(existingNames, StringComparer.OrdinalIgnoreCase);
        var toAdd = ModuleAddressTypeNames
            .Where(name => !existingSet.Contains(name))
            .ToList();

        if (toAdd.Count == 0)
            return;

        var createdDate = DateTimeOffset.UtcNow;
        var addressTypes = toAdd.Select(name => new AddressType
        {
            AddressTypeCode = name,
            AddressTypeName = name,
            Description = $"Address type for {name}",
            IsActive = true,
            CreatedDate = createdDate
        }).ToList();

        await context.AddressTypes.AddRangeAsync(addressTypes);
        await context.SaveChangesAsync();
    }
}
