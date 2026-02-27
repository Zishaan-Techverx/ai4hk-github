using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Database.Entities;

namespace TpaSodManagement.Database.Seeders;

/// <summary>
/// Seeds the Permission table with all permissions used by the application.
/// Only inserts permissions that do not already exist (by Name). Runs on app startup after migrations.
/// </summary>
public static class PermissionSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<TpaSodManagementUser>? userManager = null)
    {
        var existingNames = await context.Permissions.Where(p => p.DeletedDate == null).Select(p => p.Name).ToListAsync();
        long? createdBy = null;
        if (userManager != null)
        {
            var superAdminUsers = await userManager.GetUsersInRoleAsync("SuperAdmin");
            var superAdmin = superAdminUsers.FirstOrDefault();
            if (superAdmin != null) createdBy = superAdmin.Id;
        }
        var createdDate = DateTimeOffset.UtcNow;
        var list = new List<Permission>();
        void Add(string name, string description, string category)
        {
            if (existingNames.Contains(name)) return;
            list.Add(new Permission { Name = name, Description = description, Category = category, IsActive = true, CreatedDate = createdDate, CreatedByUserId = createdBy });
            existingNames.Add(name);
        }
        Add("Admin.View", "View admin panel and manage users/roles", "Admin");
        Add("Admin.Create", "Create new roles and assign users", "Admin");
        Add("Admin.Edit", "Edit roles and user assignments", "Admin");
        Add("Admin.Delete", "Delete roles and remove user assignments", "Admin");
        Add("Customer.View", "View customer list and details", "Customer");
        Add("Customer.Create", "Create new customers", "Customer");
        Add("Customer.Edit", "Edit customer details", "Customer");
        Add("Customer.Delete", "Delete customers", "Customer");
        Add("Customer.Detail", "View customer details (read-only)", "Customer");
        Add("Farm.View", "View farm list and details", "Farm");
        Add("Farm.Create", "Create new farms", "Farm");
        Add("Farm.Edit", "Edit farm details", "Farm");
        Add("Farm.Delete", "Delete farms", "Farm");
        Add("Farm.Detail", "View farm details (read-only)", "Farm");
        Add("Seeding.View", "View seeding list and details", "Seeding");
        Add("Seeding.Create", "Create new seedings", "Seeding");
        Add("Seeding.Edit", "Edit seeding details", "Seeding");
        Add("Seeding.Delete", "Delete seedings", "Seeding");
        Add("Seeding.Detail", "View seeding details (read-only)", "Seeding");
        Add("Product.View", "View product list and details", "Product");
        Add("Product.Create", "Create new products", "Product");
        Add("Product.Edit", "Edit product details", "Product");
        Add("Product.Delete", "Delete products", "Product");
        Add("Product.Detail", "View product details (read-only)", "Product");
        Add("Sale.View", "View sale list and details", "Sale");
        Add("Sale.Create", "Create new sales", "Sale");
        Add("Sale.Edit", "Edit sale details", "Sale");
        Add("Sale.Delete", "Delete sales", "Sale");
        Add("Sale.Detail", "View sale details (read-only)", "Sale");
        Add("Organization.View", "View organization list and details", "Organization");
        Add("Organization.Create", "Create new organizations", "Organization");
        Add("Organization.Edit", "Edit organization details", "Organization");
        Add("Organization.Delete", "Delete organizations", "Organization");
        Add("Organization.Detail", "View organization details (read-only)", "Organization");
        Add("ProductCategory.View", "View product category list and details", "ProductCategory");
        Add("ProductCategory.Create", "Create new product categories", "ProductCategory");
        Add("ProductCategory.Edit", "Edit product category details", "ProductCategory");
        Add("ProductCategory.Delete", "Delete product categories", "ProductCategory");
        Add("ProductCategory.Detail", "View product category details (read-only)", "ProductCategory");
        Add("Currency.View", "View currency list and details", "Currency");
        Add("Currency.Create", "Create new currencies", "Currency");
        Add("Currency.Edit", "Edit currency details", "Currency");
        Add("Currency.Delete", "Delete currencies", "Currency");
        Add("Currency.Detail", "View currency details (read-only)", "Currency");
        Add("AreaType.View", "View area type list and details", "AreaType");
        Add("AreaType.Create", "Create new area types", "AreaType");
        Add("AreaType.Edit", "Edit area type details", "AreaType");
        Add("AreaType.Delete", "Delete area types", "AreaType");
        Add("AreaType.Detail", "View area type details (read-only)", "AreaType");
        Add("TagRange.View", "View tag range list and details", "TagRange");
        Add("TagRange.Create", "Create new tag ranges", "TagRange");
        Add("TagRange.Edit", "Edit tag range details", "TagRange");
        Add("TagRange.Delete", "Delete tag ranges", "TagRange");
        Add("TagRange.Detail", "View tag range details (read-only)", "TagRange");
        Add("SaleType.View", "View sale type list and details", "SaleType");
        Add("SaleType.Create", "Create new sale types", "SaleType");
        Add("SaleType.Edit", "Edit sale type details", "SaleType");
        Add("SaleType.Delete", "Delete sale types", "SaleType");
        Add("SaleType.Detail", "View sale type details (read-only)", "SaleType");
        Add("Field.View", "View field list and details", "Field");
        Add("Field.Create", "Create new fields", "Field");
        Add("Field.Edit", "Edit field details", "Field");
        Add("Field.Delete", "Delete fields", "Field");
        Add("Field.Detail", "View field details (read-only)", "Field");
        Add("User.View", "View user list and details", "User");
        Add("User.Create", "Create new users", "User");
        Add("User.Edit", "Edit user details", "User");
        Add("User.Delete", "Delete users", "User");
        Add("User.Detail", "View user details (read-only)", "User");
        Add("Permission.View", "View permissions and role-permission mapping", "Permission");
        Add("Permission.Edit", "Edit role permissions", "Permission");
        Add("Export", "Export data to Excel or PDF", "Others");
        if (list.Count > 0) { await context.Permissions.AddRangeAsync(list); await context.SaveChangesAsync(); }
    }
}
