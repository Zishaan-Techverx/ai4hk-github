using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.Organization;

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
            var vm = organizations?
                .Select(o => new OrganizationItemViewModel
                {
                    OrganizationId = o.OrganizationId,
                    OrganizationName = o.OrganizationName,
                    OrganizationType = o.OrganizationType,
                    HasLogo = o.LogoBytes != null && o.LogoBytes.Length > 0
                })
                .ToList() ?? new List<OrganizationItemViewModel>();
            return View(vm);
            }

            // GET: Organization/Details/{id}
            public async Task<IActionResult> Details(long? id)
            {
                if (id == null)
                    return NotFound();

                var organization = await _organizationService.GetOrganizationByIdAsync(id.Value);
                if (organization == null)
                    return NotFound();

            var vm = MapToEditViewModel(organization, isDetailsView: true);
            ViewBag.IsDetailsView = true;
            ViewBag.Title = "Organization Details";
            return View("Edit", vm); // Same Edit view use karein
            }

            public IActionResult Create()
            {
            return View(new OrganizationEditViewModel { IsActive = true });
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrganizationEditViewModel organizationVm)
            {
                if (ModelState.IsValid)
                {
                var entity = MapToEntity(organizationVm);
                await _organizationService.CreateOrganizationAsync(entity, organizationVm.LogoFile);
                    return RedirectToAction(nameof(Index));
                }
            return View(organizationVm);
            }

            public async Task<IActionResult> Edit(long? id)
            {
                if (id == null)
                    return NotFound();

                var organization = await _organizationService.GetOrganizationByIdAsync(id.Value);
                if (organization == null)
                    return NotFound();

            var vm = MapToEditViewModel(organization);
            return View(vm);
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, OrganizationEditViewModel updatedOrgVm)
            {
            if (id != updatedOrgVm.OrganizationId)
                    return NotFound();

                if (ModelState.IsValid)
                {
                    try
                    {
                    var entity = MapToEntity(updatedOrgVm);
                    var result = await _organizationService.UpdateOrganizationAsync(id, entity, updatedOrgVm.LogoFile);
                        if (result == null)
                            return NotFound();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                    if (!await _organizationService.OrganizationExistsAsync(updatedOrgVm.OrganizationId))
                            return NotFound();
                        else
                            throw;
                    }
                    return RedirectToAction(nameof(Index));
                }
            return View(updatedOrgVm);
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Delete(long id)
            {
                try
                {
                    await _organizationService.DeleteOrganizationAsync(id);
                    return Json(new { success = true, message = "Organization deleted successfully." });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
            }

        public async Task<IActionResult> GetLogo(long id)
        {
            var organization = await _organizationService.GetOrganizationByIdAsync(id);
            if (organization?.LogoBytes == null || organization.LogoBytes.Length == 0)
                return NotFound();

            return File(organization.LogoBytes, GetImageContentType(organization.LogoBytes));
        }

        // ADD THIS METHOD - Get Logo by Organization Name
        [AllowAnonymous] // Allow access even if not authorized (for navbar)
        public async Task<IActionResult> GetLogoByName(string organizationName)
        {
            if (string.IsNullOrWhiteSpace(organizationName))
                return NotFound();

            var organization = await _organizationService.GetOrganizationByNameAsync(organizationName);
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

        private static OrganizationEditViewModel MapToEditViewModel(Organization entity, bool isDetailsView = false)
        {
            return new OrganizationEditViewModel
            {
                OrganizationId = entity.OrganizationId,
                OrganizationName = entity.OrganizationName,
                OrganizationType = entity.OrganizationType,
                OrganizationCode = entity.OrganizationCode,
                TaxIdentificationNumber = entity.TaxIdentificationNumber,
                RegistrationNumber = entity.RegistrationNumber,
                EstablishedDate = entity.EstablishedDate.HasValue ? (DateTimeOffset?)new DateTimeOffset(entity.EstablishedDate.Value.ToDateTime(TimeOnly.MinValue)) : null,
                Description = entity.Description,
                IsActive = entity.IsActive,
                LogoBytes = entity.LogoBytes,
                HasLogo = entity.LogoBytes != null && entity.LogoBytes.Length > 0,
                IsDetailsView = isDetailsView
            };
        }

        private static Organization MapToEntity(OrganizationEditViewModel vm)
        {
            return new Organization
            {
                OrganizationId = vm.OrganizationId,
                OrganizationName = vm.OrganizationName,
                OrganizationType = vm.OrganizationType,
                OrganizationCode = vm.OrganizationCode,
                TaxIdentificationNumber = vm.TaxIdentificationNumber,
                RegistrationNumber = vm.RegistrationNumber,
                EstablishedDate = vm.EstablishedDate.HasValue ? DateOnly.FromDateTime(vm.EstablishedDate.Value.Date) : null,
                Description = vm.Description,
                IsActive = vm.IsActive
            };
        }
    }
}
