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
        }
}
