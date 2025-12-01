using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Data;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Implementations;
using TpaSodManagement.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Configure DbContext
builder.Services.AddDbContext<SodDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity
builder.Services.AddDefaultIdentity<TpaSodManagementUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IUserClaimsPrincipalFactory<TpaSodManagementUser>, AppClaimsPrincipalFactory>();

// Register application services
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IFarmService, FarmService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<ISeedingService, SeedingService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanViewAdmin", policy =>
        policy.RequireClaim("Permission", "Admin.View"));
    options.AddPolicy("CanCreateAdmin", policy =>
        policy.RequireClaim("Permission", "Admin.Create"));
    options.AddPolicy("CanEditAdmin", policy =>
        policy.RequireClaim("Permission", "Admin.Edit"));
    options.AddPolicy("CanDeleteAdmin", policy =>
        policy.RequireClaim("Permission", "Admin.Delete"));

    options.AddPolicy("CanViewCustomers", policy =>
        policy.RequireClaim("Permission", "Customer.View"));
    options.AddPolicy("CanCreateCustomers", policy =>
        policy.RequireClaim("Permission", "Customer.Create"));
    options.AddPolicy("CanEditCustomers", policy =>
        policy.RequireClaim("Permission", "Customer.Edit"));
    options.AddPolicy("CanDeleteCustomers", policy =>
        policy.RequireClaim("Permission", "Customer.Delete"));

    options.AddPolicy("CanViewFarms", policy =>
        policy.RequireClaim("Permission", "Farm.View"));
    options.AddPolicy("CanCreateFarms", policy =>
        policy.RequireClaim("Permission", "Farm.Create"));
    options.AddPolicy("CanEditFarms", policy =>
        policy.RequireClaim("Permission", "Farm.Edit"));
    options.AddPolicy("CanDeleteFarms", policy =>
        policy.RequireClaim("Permission", "Farm.Delete"));

    options.AddPolicy("CanViewOrganizations", policy =>
        policy.RequireClaim("Permission", "Organization.View"));
    options.AddPolicy("CanCreateOrganizations", policy =>
        policy.RequireClaim("Permission", "Organization.Create"));
    options.AddPolicy("CanEditOrganizations", policy =>
        policy.RequireClaim("Permission", "Organization.Edit"));
    options.AddPolicy("CanDeleteOrganizations", policy =>
        policy.RequireClaim("Permission", "Organization.Delete"));

    options.AddPolicy("CanViewProducts", policy =>
        policy.RequireClaim("Permission", "Product.View"));
    options.AddPolicy("CanCreateProducts", policy =>
        policy.RequireClaim("Permission", "Product.Create"));
    options.AddPolicy("CanEditProducts", policy =>
        policy.RequireClaim("Permission", "Product.Edit"));
    options.AddPolicy("CanDeleteProducts", policy =>
        policy.RequireClaim("Permission", "Product.Delete"));

    options.AddPolicy("CanViewSales", policy =>
        policy.RequireClaim("Permission", "Sale.View"));
    options.AddPolicy("CanCreateSales", policy =>
        policy.RequireClaim("Permission", "Sale.Create"));
    options.AddPolicy("CanEditSales", policy =>
        policy.RequireClaim("Permission", "Sale.Edit"));
    options.AddPolicy("CanDeleteSales", policy =>
        policy.RequireClaim("Permission", "Sale.Delete"));

    options.AddPolicy("CanViewPermissions", policy =>
        policy.RequireClaim("Permission", "Permission.View"));
    options.AddPolicy("CanEditPermissions", policy =>
        policy.RequireClaim("Permission", "Permission.Edit"));

    options.AddPolicy("CanRunSeeding", policy =>
        policy.RequireClaim("Permission", "Seeding.Run"));
    options.AddPolicy("CanViewSeeding", policy =>
        policy.RequireClaim("Permission", "Seeding.View"));
    options.AddPolicy("CanManageSeeding", policy =>
        policy.RequireClaim("Permission", "Seeding.Manage"));
    options.AddPolicy("CanResetSeeding", policy =>
        policy.RequireClaim("Permission", "Seeding.Reset"));
});

// Add MVC and Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// AutoMapper registration
builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();


// Configure middleware...
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();