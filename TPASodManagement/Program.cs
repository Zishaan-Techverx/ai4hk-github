using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Implementations;
using TpaSodManagement.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ----------------------
// Configure SodDbContext
// ----------------------
// Use SQL Server and the same connection for both Identity and app data
builder.Services.AddDbContext<SodDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ----------------------
// Configure Identity
// ----------------------
// Use custom TpaUser and UserRole for authentication
builder.Services.AddIdentity<TpaUser, UserRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true; // Require email confirmation
})
.AddEntityFrameworkStores<SodDbContext>()
.AddDefaultTokenProviders();

// ----------------------
// Register application services (Dependency Injection)
// ----------------------
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IFarmService, FarmService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<ISeedingService, SeedingService>();

// ----------------------
// Add MVC Controllers with Views and Razor Pages
// ----------------------
builder.Services.AddControllersWithViews(); // For MVC
builder.Services.AddRazorPages();           // Required for Identity UI

// ----------------------
// AutoMapper registration
// ----------------------
builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// ----------------------
// Configure HTTP request pipeline
// ----------------------
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Enforce HTTPS
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ----------------------
// Enable Authentication & Authorization
// ----------------------
app.UseAuthentication();
app.UseAuthorization();

// ----------------------
// Map MVC Controller routes
// ----------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map Razor Pages (required for Identity)
app.MapRazorPages();

app.Run();
