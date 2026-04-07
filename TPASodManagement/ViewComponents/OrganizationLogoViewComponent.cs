using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Database;
using TpaSodManagement.Database.Entities;

namespace TpaSodManagement.ViewComponents
{
    public class OrganizationLogoViewComponent : ViewComponent
    {
        private readonly UserManager<TpaSodManagementUser> _userManager;
        private readonly ApplicationDbContext _context;

        public OrganizationLogoViewComponent(
            UserManager<TpaSodManagementUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            Farm? farm = null;

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user != null && user.FarmId.HasValue)
                {
                    farm = await _context.Farms.FirstOrDefaultAsync(f => f.FarmId == user.FarmId.Value);
                }
            }

            return View("Default", farm);
        }
    }
}