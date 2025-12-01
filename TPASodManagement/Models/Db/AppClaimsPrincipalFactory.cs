using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using TpaSodManagement.Data;
using TpaSodManagement.Areas.Identity.Data; 

public class AppClaimsPrincipalFactory : UserClaimsPrincipalFactory<TpaSodManagementUser, IdentityRole>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<TpaSodManagementUser> _userManager;

    public AppClaimsPrincipalFactory(
        UserManager<TpaSodManagementUser> userManager, 
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor,
        ApplicationDbContext dbContext)
        : base(userManager, roleManager, optionsAccessor)
    {
        _dbContext = dbContext;
        _userManager = userManager; 
    }

    public override async Task<ClaimsPrincipal> CreateAsync(TpaSodManagementUser user)
    {
        var principal = await base.CreateAsync(user);
        var identity = (ClaimsIdentity)principal.Identity;

        var roles = await _userManager.GetRolesAsync(user);

        foreach (var roleName in roles)
        {
            var role = await RoleManager.FindByNameAsync(roleName);
            if (role == null) continue;

            var rolePermissions = await _dbContext.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => rp.RoleId == role.Id)
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToListAsync();

            foreach (var permissionName in rolePermissions)
            {
                identity.AddClaim(new Claim("Permission", permissionName));
            }
        }
        return principal;
    }
}