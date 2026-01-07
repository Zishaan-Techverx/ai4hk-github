using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.Organization;
using OfficeOpenXml;

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

            public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
            {
                // Set filter columns for the partial view
                ViewBag.FilterColumns = new Dictionary<string, string>
                {
                    { "OrganizationName", "Organization Name" },
                    { "OrganizationType", "Organization Type" },
                    { "OrganizationCode", "Organization Code" },
                    { "IsActive", "Is Active" }
                };
                ViewBag.ModuleName = "Organizations";
                ViewBag.BooleanColumns = new HashSet<string> { "IsActive" };

                var organizations = await _organizationService.GetAllOrganizationsAsync();
                var allVm = organizations?
                    .Select(o => new OrganizationItemViewModel
                    {
                        OrganizationId = o.OrganizationId,
                        OrganizationName = o.OrganizationName,
                        OrganizationType = o.OrganizationType,
                        HasLogo = o.LogoBytes != null && o.LogoBytes.Length > 0
                    })
                    .ToList() ?? new List<OrganizationItemViewModel>();

                // Pagination
                var totalCount = allVm.Count;
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                var vm = allVm.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                ViewBag.PageNumber = pageNumber;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalCount = totalCount;
                ViewBag.PageSize = pageSize;

                return View(vm);
            }

            [HttpPost]
            [IgnoreAntiforgeryToken]
            public async Task<IActionResult> Filter([FromBody] Dictionary<string, string> filters)
            {
                try
                {
                    var result = await _organizationService.GetFilteredAsync(filters ?? new Dictionary<string, string>());
                    if (!result.Success)
                    {
                        return Json(new { success = false, message = result.Message });
                    }

                    var vm = result.Data?.Select(o => new OrganizationItemViewModel
                    {
                        OrganizationId = o.OrganizationId,
                        OrganizationName = o.OrganizationName,
                        OrganizationType = o.OrganizationType,
                        HasLogo = o.LogoBytes != null && o.LogoBytes.Length > 0
                    }).ToList() ?? new List<OrganizationItemViewModel>();

                    return Json(new { success = true, data = vm });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"Error filtering data: {ex.Message}" });
                }
            }

            [HttpPost]
            [IgnoreAntiforgeryToken]
            public async Task<IActionResult> Print([FromBody] Dictionary<string, string> filters)
            {
                try
                {
                    // Get all filtered records (no pagination)
                    var result = await _organizationService.GetFilteredAsync(filters ?? new Dictionary<string, string>());
                    if (!result.Success)
                    {
                        return Json(new { success = false, message = result.Message });
                    }

                    var organizations = result.Data ?? new List<Organization>();

                    // Set EPPlus license context (non-commercial use)
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                    // Generate Excel file using EPPlus
                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Organizations");

                        // Set header row
                        worksheet.Cells[1, 1].Value = "Organization Name";
                        worksheet.Cells[1, 2].Value = "Organization Type";
                        worksheet.Cells[1, 3].Value = "Organization Code";
                        worksheet.Cells[1, 4].Value = "Is Active";

                        // Style header row
                        using (var range = worksheet.Cells[1, 1, 1, 4])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                            range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        }

                        // Add data rows
                        for (int i = 0; i < organizations.Count; i++)
                        {
                            var row = i + 2;
                            var item = organizations[i];
                            worksheet.Cells[row, 1].Value = item.OrganizationName ?? "";
                            worksheet.Cells[row, 2].Value = item.OrganizationType ?? "";
                            worksheet.Cells[row, 3].Value = item.OrganizationCode ?? "";
                            worksheet.Cells[row, 4].Value = item.IsActive ? "Yes" : "No";
                        }

                        // Auto-fit columns
                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                        // Add borders to data cells
                        if (organizations.Count > 0)
                        {
                            using (var range = worksheet.Cells[1, 1, organizations.Count + 1, 4])
                            {
                                range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            }
                        }

                        var stream = new MemoryStream();
                        package.SaveAs(stream);
                        stream.Position = 0;

                        var fileName = $"Organizations_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
                        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"Error generating Excel file: {ex.Message}" });
                }
            }

            // GET: Organization/Details/{id}
            public async Task<IActionResult> Details(long? id)
            {
                if (id == null)
                    return NotFound();

                var organization = await _organizationService.GetOrganizationByIdAsync(id.Value);
                if (organization == null)
                    return NotFound();

            // Clear TempData messages when viewing details (they should only show on Index)
            TempData.Remove("SuccessMessage");
            TempData.Remove("ErrorMessage");
            
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
                        {
                            TempData["ErrorMessage"] = "Organization not found.";
                            return View(updatedOrgVm);
                        }
                        
                        TempData["SuccessMessage"] = "Organization updated successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                    if (!await _organizationService.OrganizationExistsAsync(updatedOrgVm.OrganizationId))
                        {
                            TempData["ErrorMessage"] = "Organization not found.";
                            return View(updatedOrgVm);
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "The organization was modified by another user. Please refresh and try again.";
                            return View(updatedOrgVm);
                        }
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = $"An error occurred while updating the organization: {ex.Message}";
                        return View(updatedOrgVm);
                    }
                }
                
                // ModelState is invalid - return view with errors
                TempData["ErrorMessage"] = "Please correct the validation errors below.";
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
