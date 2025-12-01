using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.Models.Db;

namespace TpaSodManagement.ViewComponents
{
    public class OrganizationLogoViewComponent : ViewComponent
    {
        private readonly UserManager<TpaSodManagementUser> _userManager;
        private readonly IOrganizationService _organizationService;

        public OrganizationLogoViewComponent(
            UserManager<TpaSodManagementUser> userManager,
            IOrganizationService organizationService)
        {
            _userManager = userManager;
            _organizationService = organizationService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            Organization organization = null;

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user != null && !string.IsNullOrWhiteSpace(user.OrganizationName))
                {
                    organization = await _organizationService.GetOrganizationByNameAsync(user.OrganizationName);
                }
            }

            // Return with explicit model type
            return View("Default", organization);
        }
    }
}