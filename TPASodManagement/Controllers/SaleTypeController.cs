using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Linq;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.SaleType;
using TpaSodManagement.Utilities;
using TpaSodManagement.Database.Entities;
using Microsoft.AspNetCore.Identity;
using TpaSodManagement.Areas.Identity.Data;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class SaleTypeController : Controller
    {
        private readonly ISaleTypeService _saleTypeService;
        private readonly IExportToExcel _exportToExcel;
        private readonly IExportToPdf _exportToPdf;
        private readonly UserManager<TpaSodManagementUser> _userManager;

        public SaleTypeController(ISaleTypeService saleTypeService, IExportToExcel exportToExcel, IExportToPdf exportToPdf, UserManager<TpaSodManagementUser> userManager)
        {
            _saleTypeService = saleTypeService;
            _exportToExcel = exportToExcel;
            _exportToPdf = exportToPdf;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            ViewBag.FilterColumns = new Dictionary<string, string>
            {
                { "SaleTypeCode", "Sale Type Code" },
                { "SaleTypeName", "Sale Type Name" },
                { "RequiresCertificate", "Requires Certificate" },
                { "CertificateTypeName", "Certificate Type" },
                { "TaxApplicable", "Tax Applicable" },
                { "IsActive", "Is Active" }
            };
            ViewBag.ModuleName = "Sale Types";
            ViewBag.BooleanColumns = new HashSet<string> { "RequiresCertificate", "TaxApplicable" };
            ViewBag.TriStateColumns = new HashSet<string> { "IsActive" };

            var result = await _saleTypeService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                ViewBag.PageNumber = 1;
                ViewBag.TotalPages = 1;
                ViewBag.TotalCount = 0;
                ViewBag.PageSize = pageSize;
                return View(new List<SaleTypeItemViewModel>());
            }

            var allSaleTypes = result.Data ?? new List<SaleType>();
            var totalCount = allSaleTypes.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var paginated = allSaleTypes
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = paginated.Select(MapToItemViewModel).ToList();

            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Filter([FromBody] Dictionary<string, string> filters)
        {
            try
            {
                var result = await _saleTypeService.GetFilteredAsync(filters ?? new Dictionary<string, string>());
                if (!result.Success)
                    return Json(new { success = false, message = result.Message });
                var vm = result.Data?.Select(MapToItemViewModel).ToList() ?? new List<SaleTypeItemViewModel>();
                return Json(new { success = true, data = vm });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error filtering data: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Print([FromBody] JsonElement requestData)
        {
            try
            {
                Dictionary<string, string> filters = new Dictionary<string, string>();
                List<string> hiddenColumns = new List<string>();
                if (requestData.ValueKind == JsonValueKind.Object)
                {
                    if (requestData.TryGetProperty("filters", out var filtersElement))
                        filters = JsonSerializer.Deserialize<Dictionary<string, string>>(filtersElement.GetRawText()) ?? new Dictionary<string, string>();
                    if (requestData.TryGetProperty("hiddenColumns", out var hiddenColumnsElement))
                        hiddenColumns = JsonSerializer.Deserialize<List<string>>(hiddenColumnsElement.GetRawText()) ?? new List<string>();
                }
                var result = await _saleTypeService.GetFilteredAsync(filters);
                if (!result.Success)
                    return Json(new { success = false, message = result.Message });
                var saleTypes = result.Data ?? new List<SaleType>();
                var vm = saleTypes.Select(MapToItemViewModel).ToList();
                var allColumns = new List<(string Header, string PropertyName)>
                {
                    ("Sale Type Code", "SaleTypeCode"),
                    ("Sale Type Name", "SaleTypeName"),
                    ("Requires Certificate", "RequiresCertificate"),
                    ("Certificate Type", "CertificateTypeName"),
                    ("Tax Applicable", "TaxApplicable"),
                    ("Is Active", "IsActive")
                };
                var visibleColumns = allColumns.Where(col => !hiddenColumns.Contains(col.PropertyName)).ToList();
                var columnHeaders = visibleColumns.Select(col => col.Header).ToList();
                var columnIndices = visibleColumns.Select(col => allColumns.IndexOf(allColumns.First(c => c.PropertyName == col.PropertyName))).ToList();
                var stream = _exportToExcel.GenerateExcel(
                    moduleName: "SaleTypes",
                    worksheetName: "Sale Types",
                    columnHeaders: columnHeaders,
                    data: vm,
                    rowMapper: item =>
                    {
                        var allValues = new List<object>
                        {
                            item.SaleTypeCode ?? "",
                            item.SaleTypeName ?? "",
                            item.RequiresCertificate ? "Yes" : "No",
                            item.CertificateTypeName ?? "",
                            item.TaxApplicable ? "Yes" : "No",
                            item.IsActive ? "Yes" : "No"
                        };
                        return columnIndices.Select(idx => allValues[idx]).ToList();
                    }
                );
                var fileName = $"SaleTypes_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error generating Excel: {ex.Message}" });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ExportPdf([FromBody] JsonElement requestData)
        {
            try
            {
                Dictionary<string, string> filters = new Dictionary<string, string>();
                List<string> hiddenColumns = new List<string>();
                byte[]? headerImageBytes = null;
                if (requestData.ValueKind == JsonValueKind.Object)
                {
                    if (requestData.TryGetProperty("filters", out var filtersElement))
                        filters = JsonSerializer.Deserialize<Dictionary<string, string>>(filtersElement.GetRawText()) ?? new Dictionary<string, string>();
                    if (requestData.TryGetProperty("hiddenColumns", out var hiddenColumnsElement))
                        hiddenColumns = JsonSerializer.Deserialize<List<string>>(hiddenColumnsElement.GetRawText()) ?? new List<string>();
                    if (requestData.TryGetProperty("headerImageBase64", out var headerImgEl))
                    {
                        var b64 = headerImgEl.GetString();
                        if (!string.IsNullOrEmpty(b64))
                        {
                            try { headerImageBytes = Convert.FromBase64String(b64); } catch { }
                        }
                    }
                }
                var result = await _saleTypeService.GetFilteredAsync(filters);
                if (!result.Success)
                    return Json(new { success = false, message = result.Message });
                var saleTypes = result.Data ?? new List<SaleType>();
                var vm = saleTypes.Select(MapToItemViewModel).ToList();
                var allColumns = new List<(string Header, string PropertyName)>
                {
                    ("Sale Type Code", "SaleTypeCode"),
                    ("Sale Type Name", "SaleTypeName"),
                    ("Requires Certificate", "RequiresCertificate"),
                    ("Certificate Type", "CertificateTypeName"),
                    ("Tax Applicable", "TaxApplicable"),
                    ("Is Active", "IsActive")
                };
                var visibleColumns = allColumns.Where(col => !hiddenColumns.Contains(col.PropertyName)).ToList();
                var columnHeaders = visibleColumns.Select(col => col.Header).ToList();
                var columnIndices = visibleColumns.Select(col => allColumns.IndexOf(allColumns.First(c => c.PropertyName == col.PropertyName))).ToList();
                var stream = _exportToPdf.GeneratePdf("Sale Types", columnHeaders, vm, item =>
                {
                    var allValues = new List<object>
                    {
                        item.SaleTypeCode ?? "",
                        item.SaleTypeName ?? "",
                        item.RequiresCertificate ? "Yes" : "No",
                        item.CertificateTypeName ?? "",
                        item.TaxApplicable ? "Yes" : "No",
                        item.IsActive ? "Yes" : "No"
                    };
                    return columnIndices.Select(idx => allValues[idx]).ToList();
                }, headerImageBytes);
                var fileName = $"SaleTypes_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
                return File(stream, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error generating PDF: {ex.Message}" });
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();
            var result = await _saleTypeService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null)
                return NotFound();
            var vm = MapToEditViewModel(result.Data, isDetailsView: true);
            await PopulateCertificateTypes(vm);
            ViewBag.IsDetailsView = true;
            ViewBag.Title = "Sale Type Details";
            return View("Edit", vm);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new SaleTypeEditViewModel { IsActive = true };
            await PopulateCertificateTypes(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaleTypeEditViewModel saleTypeVm)
        {
            if (ModelState.IsValid)
            {
                var saleType = MapToEntity(saleTypeVm);
                var result = await _saleTypeService.CreateAsync(saleType);
                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message;
                    await PopulateCertificateTypes(saleTypeVm);
                    return View(saleTypeVm);
                }
                TempData["SuccessMessage"] = "Sale type created successfully.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateCertificateTypes(saleTypeVm);
            return View(saleTypeVm);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();
            var result = await _saleTypeService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null)
                return NotFound();
            var vm = MapToEditViewModel(result.Data);
            await PopulateCertificateTypes(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SaleTypeEditViewModel saleTypeVm)
        {
            if (id != saleTypeVm.SaleTypeId)
                return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    var saleType = MapToEntity(saleTypeVm);
                    var result = await _saleTypeService.UpdateAsync(saleType);
                    if (!result.Success)
                    {
                        TempData["ErrorMessage"] = result.Message;
                        await PopulateCertificateTypes(saleTypeVm);
                        return View(saleTypeVm);
                    }
                    TempData["SuccessMessage"] = "Sale type updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    var exists = await _saleTypeService.ExisTpasync(saleTypeVm.SaleTypeId);
                    if (!exists.Data)
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            await PopulateCertificateTypes(saleTypeVm);
            return View(saleTypeVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            long? deletedByUserId = currentUser?.Id;
            var result = await _saleTypeService.DeleteAsync(id, deletedByUserId);
            if (!result.Success)
                return Json(new { success = false, message = result.Message });
            return Json(new { success = true, message = "Sale type deleted successfully." });
        }

        private async Task PopulateCertificateTypes(SaleTypeEditViewModel vm)
        {
            var result = await _saleTypeService.GetCertificateTypesForDropdownAsync();
            vm.CertificateTypes = result.Success && result.Data != null
                ? new SelectList(result.Data, "Value", "Text", vm.CertificateTypeId?.ToString())
                : Enumerable.Empty<SelectListItem>();
        }

        private static SaleTypeItemViewModel MapToItemViewModel(SaleType entity)
        {
            return new SaleTypeItemViewModel
            {
                SaleTypeId = entity.SaleTypeId,
                SaleTypeCode = entity.SaleTypeCode ?? "",
                SaleTypeName = entity.SaleTypeName ?? "",
                RequiresCertificate = entity.RequiresCertificate,
                CertificateTypeId = entity.CertificateTypeId,
                CertificateTypeName = entity.CertificateType?.CertificateTypeName,
                TaxApplicable = entity.TaxApplicable,
                Description = entity.Description,
                IsActive = entity.IsActive
            };
        }

        private static SaleTypeEditViewModel MapToEditViewModel(SaleType entity, bool isDetailsView = false)
        {
            return new SaleTypeEditViewModel
            {
                SaleTypeId = entity.SaleTypeId,
                SaleTypeCode = entity.SaleTypeCode ?? "",
                SaleTypeName = entity.SaleTypeName ?? "",
                RequiresCertificate = entity.RequiresCertificate,
                CertificateTypeId = entity.CertificateTypeId,
                TaxApplicable = entity.TaxApplicable,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreatedDate = entity.CreatedDate,
                IsDetailsView = isDetailsView
            };
        }

        private static SaleType MapToEntity(SaleTypeEditViewModel vm)
        {
            return new SaleType
            {
                SaleTypeId = vm.SaleTypeId,
                SaleTypeCode = vm.SaleTypeCode,
                SaleTypeName = vm.SaleTypeName,
                RequiresCertificate = vm.RequiresCertificate,
                CertificateTypeId = vm.CertificateTypeId,
                TaxApplicable = vm.TaxApplicable,
                Description = vm.Description,
                IsActive = vm.IsActive,
                CreatedDate = vm.CreatedDate ?? DateTimeOffset.UtcNow
            };
        }
    }
}
