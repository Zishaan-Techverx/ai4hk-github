using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.AreaType;
using TpaSodManagement.Utilities;
using TpaSodManagement.Database.Entities;
using Microsoft.AspNetCore.Identity;
using TpaSodManagement.Areas.Identity.Data;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class AreaTypeController : Controller
    {
        private readonly IAreaTypeService _areaTypeService;
        private readonly IExportToExcel _exportToExcel;
        private readonly UserManager<TpaSodManagementUser> _userManager;

        public AreaTypeController(IAreaTypeService areaTypeService, IExportToExcel exportToExcel, UserManager<TpaSodManagementUser> userManager)
        {
            _areaTypeService = areaTypeService;
            _exportToExcel = exportToExcel;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Set filter columns for the partial view
            ViewBag.FilterColumns = new Dictionary<string, string>
            {
                { "AreaTypeName", "Area Type Name" },
                { "UnitAbbreviation", "Unit Abbreviation" },
                { "UnitSystem", "Unit System" },
                { "ConversionToSquareMeters", "Conversion To Square Meters" },
                { "IsActive", "Is Active" }
            };
            ViewBag.ModuleName = "Area Types";
            ViewBag.BooleanColumns = new HashSet<string>();
            ViewBag.TriStateColumns = new HashSet<string> { "IsActive" };

            var result = await _areaTypeService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(new List<AreaTypeItemViewModel>());
            }
            var vm = result.Data?.Select(MapToItemViewModel).ToList() ?? new List<AreaTypeItemViewModel>();
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Filter([FromBody] Dictionary<string, string> filters)
        {
            try
            {
                var result = await _areaTypeService.GetFilteredAsync(filters ?? new Dictionary<string, string>());
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var vm = result.Data?.Select(MapToItemViewModel).ToList() ?? new List<AreaTypeItemViewModel>();
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
                // Extract filters and hiddenColumns from request
                Dictionary<string, string> filters = new Dictionary<string, string>();
                List<string> hiddenColumns = new List<string>();

                if (requestData.ValueKind == JsonValueKind.Object)
                {
                    // Extract filters
                    if (requestData.TryGetProperty("filters", out var filtersElement))
                    {
                        filters = JsonSerializer.Deserialize<Dictionary<string, string>>(filtersElement.GetRawText()) ?? new Dictionary<string, string>();
                    }
                    else
                    {
                        // Backward compatibility: if filters are sent directly (old format)
                        var directFilters = JsonSerializer.Deserialize<Dictionary<string, string>>(requestData.GetRawText());
                        if (directFilters != null && !directFilters.ContainsKey("hiddenColumns"))
                        {
                            filters = directFilters;
                        }
                    }

                    // Extract hiddenColumns
                    if (requestData.TryGetProperty("hiddenColumns", out var hiddenColumnsElement))
                    {
                        hiddenColumns = JsonSerializer.Deserialize<List<string>>(hiddenColumnsElement.GetRawText()) ?? new List<string>();
                    }
                }

                var result = await _areaTypeService.GetFilteredAsync(filters);
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var areaTypes = result.Data ?? new List<AreaType>();
                var vm = areaTypes.Select(MapToItemViewModel).ToList();

                var allColumns = new List<(string Header, string PropertyName)>
                {
                    ("Area Type Name", "AreaTypeName"),
                    ("Unit Abbreviation", "UnitAbbreviation"),
                    ("Unit System", "UnitSystem"),
                    ("Conversion To Square Meters", "ConversionToSquareMeters"),
                    ("Is Active", "IsActive")
                };

                var visibleColumns = allColumns.Where(col => !hiddenColumns.Contains(col.PropertyName)).ToList();
                var columnHeaders = visibleColumns.Select(col => col.Header).ToList();
                var columnIndices = visibleColumns.Select(col => allColumns.IndexOf(allColumns.First(c => c.PropertyName == col.PropertyName))).ToList();

                var stream = _exportToExcel.GenerateExcel(
                    moduleName: "AreaTypes",
                    worksheetName: "Area Types",
                    columnHeaders: columnHeaders,
                    data: vm,
                    rowMapper: item =>
                    {
                        var allValues = new List<object>
                        {
                            item.AreaTypeName ?? "",
                            item.UnitAbbreviation ?? "",
                            item.UnitSystem ?? "",
                            item.ConversionToSquareMeters?.ToString("N2") ?? "",
                            item.IsActive ? "Yes" : "No"
                        };
                        return columnIndices.Select(idx => allValues[idx]).ToList();
                    }
                );

                var fileName = $"AreaTypes_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error generating Excel: {ex.Message}" });
            }
        }

        // GET: AreaType/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var result = await _areaTypeService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null)
                return NotFound();

            var vm = MapToEditViewModel(result.Data, isDetailsView: true);
            ViewBag.IsDetailsView = true;
            ViewBag.Title = "Area Type Details";
            return View("Edit", vm);
        }

        public IActionResult Create()
        {
            return View(new AreaTypeEditViewModel { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AreaTypeEditViewModel areaTypeVm)
        {
            if (ModelState.IsValid)
            {
                var areaType = MapToEntity(areaTypeVm);
                var result = await _areaTypeService.CreateAsync(areaType);
                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message;
                    return View(areaTypeVm);
                }

                TempData["SuccessMessage"] = "Area type created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(areaTypeVm);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var result = await _areaTypeService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null)
                return NotFound();

            var vm = MapToEditViewModel(result.Data);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AreaTypeEditViewModel areaTypeVm)
        {
            if (id != areaTypeVm.AreaTypeId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var areaType = MapToEntity(areaTypeVm);
                    var result = await _areaTypeService.UpdateAsync(areaType);
                    if (!result.Success)
                    {
                        TempData["ErrorMessage"] = result.Message;
                        return View(areaTypeVm);
                    }

                    TempData["SuccessMessage"] = "Area type updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    var exists = await _areaTypeService.ExisTpasync(areaTypeVm.AreaTypeId);
                    if (!exists.Data)
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(areaTypeVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            long? deletedByUserId = currentUser?.Id;
            
            var result = await _areaTypeService.DeleteAsync(id, deletedByUserId);
            if (!result.Success)
            {
                return Json(new { success = false, message = result.Message });
            }
            return Json(new { success = true, message = "Area type deleted successfully." });
        }

        private static AreaTypeItemViewModel MapToItemViewModel(AreaType entity)
        {
            return new AreaTypeItemViewModel
            {
                AreaTypeId = entity.AreaTypeId,
                AreaTypeName = entity.AreaTypeName,
                UnitAbbreviation = entity.UnitAbbreviation,
                UnitSystem = entity.UnitSystem,
                ConversionToSquareMeters = entity.ConversionToSquareMeters,
                IsActive = entity.IsActive
            };
        }

        private static AreaTypeEditViewModel MapToEditViewModel(AreaType entity, bool isDetailsView = false)
        {
            return new AreaTypeEditViewModel
            {
                AreaTypeId = entity.AreaTypeId,
                AreaTypeName = entity.AreaTypeName,
                UnitAbbreviation = entity.UnitAbbreviation,
                UnitSystem = entity.UnitSystem,
                ConversionToSquareMeters = entity.ConversionToSquareMeters,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreatedDate = entity.CreatedDate,
                IsDetailsView = isDetailsView
            };
        }

        private static AreaType MapToEntity(AreaTypeEditViewModel vm)
        {
            return new AreaType
            {
                AreaTypeId = vm.AreaTypeId,
                AreaTypeName = vm.AreaTypeName,
                UnitAbbreviation = vm.UnitAbbreviation,
                UnitSystem = vm.UnitSystem,
                ConversionToSquareMeters = vm.ConversionToSquareMeters ?? 0,
                Description = vm.Description,
                IsActive = vm.IsActive,
                CreatedDate = vm.CreatedDate ?? DateTimeOffset.UtcNow
            };
        }
    }
}
