using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class OrganizationController : Controller
        {
            private readonly IOrganizationService _organizationService;

            public OrganizationController(IOrganizationService organizationService)
            {
                _organizationService = organizationService;
            }

            public async Task<IActionResult> Index()
            {
                var organizations = await _organizationService.GetAllOrganizationsAsync();
                return View(organizations);
            }

            public async Task<IActionResult> Details(long? id)
            {
                if (id == null)
                    return NotFound();

                var organization = await _organizationService.GetOrganizationByIdAsync(id.Value);
                if (organization == null)
                    return NotFound();

                return View(organization);
            }

            public IActionResult Create()
            {
                return View();
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create(Organization organization)
            {
                if (ModelState.IsValid)
                {
                    await _organizationService.CreateOrganizationAsync(organization, organization.LogoFile);
                    return RedirectToAction(nameof(Index));
                }
                return View(organization);
            }

            public async Task<IActionResult> Edit(long? id)
            {
                if (id == null)
                    return NotFound();

                var organization = await _organizationService.GetOrganizationByIdAsync(id.Value);
                if (organization == null)
                    return NotFound();

                return View(organization);
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(long id, Organization updatedOrg)
            {
                if (id != updatedOrg.OrganizationId)
                    return NotFound();

                if (ModelState.IsValid)
                {
                    try
                    {
                        var result = await _organizationService.UpdateOrganizationAsync(id, updatedOrg, updatedOrg.LogoFile);
                        if (result == null)
                            return NotFound();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!await _organizationService.OrganizationExistsAsync(updatedOrg.OrganizationId))
                            return NotFound();
                        else
                            throw;
                    }
                    return RedirectToAction(nameof(Index));
                }
                return View(updatedOrg);
            }

            public async Task<IActionResult> Delete(long? id)
            {
                if (id == null)
                    return NotFound();

                var organization = await _organizationService.GetOrganizationByIdAsync(id.Value);
                if (organization == null)
                    return NotFound();

                return View(organization);
            }

            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(long id)
            {
                await _organizationService.DeleteOrganizationAsync(id);
                return RedirectToAction(nameof(Index));
            }

        public async Task<IActionResult> GetLogo(long id)
        {
            var organization = await _organizationService.GetOrganizationByIdAsync(id);
            if (organization?.LogoBytes == null || organization.LogoBytes.Length == 0)
                return NotFound();

            return File(organization.LogoBytes, GetImageContentType(organization.LogoBytes));
        }

        private string GetImageContentType(byte[] bytes)
        {
            if (bytes.Length < 4) return "image/jpeg";

            // PNG detection
            if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
                return "image/png";

            // JPEG detection
            if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
                return "image/jpeg";

            // GIF detection
            if (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46)
                return "image/gif";

            // Default to JPEG
            return "image/jpeg";
        }
    }
}
