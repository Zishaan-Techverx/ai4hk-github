using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.Field;
using TpaSodManagement.Utilities;
using TpaSodManagement.Database.Entities;
using Microsoft.AspNetCore.Identity;
using TpaSodManagement.Areas.Identity.Data;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class FieldController : Controller
    {
        private readonly IFieldService _fieldService;
        private readonly IExportToExcel _exportToExcel;
        private readonly UserManager<TpaSodManagementUser> _userManager;

        public FieldController(IFieldService fieldService, IExportToExcel exportToExcel, UserManager<TpaSodManagementUser> userManager)
        {
            _fieldService = fieldService;
            _exportToExcel = exportToExcel;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            // Set filter columns for the partial view
            ViewBag.FilterColumns = new Dictionary<string, string>
            {
                { "FieldName", "Field Name" },
                { "FieldCode", "Field Code" },
                { "AreaAmount", "Area Amount" },
                { "AreaTypeName", "Area Type" },
                { "FarmLicenseNumber", "Farm" },
                { "SoilType", "Soil Type" },
                { "IrrigationAvailable", "Irrigation Available" },
                { "IsActive", "Is Active" }
            };
            ViewBag.ModuleName = "Fields";
            ViewBag.BooleanColumns = new HashSet<string> { "IrrigationAvailable" };
            ViewBag.TriStateColumns = new HashSet<string> { "IsActive" };

            var result = await _fieldService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                ViewBag.PageNumber = 1;
                ViewBag.TotalPages = 1;
                ViewBag.TotalCount = 0;
                ViewBag.PageSize = pageSize;
                return View(new List<FieldItemViewModel>());
            }

            var allFields = result.Data ?? new List<Field>();
            var totalCount = allFields.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Apply pagination
            var paginatedFields = allFields
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = paginatedFields.Select(MapToItemViewModel).ToList();

            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;

            return View(vm);
        }

        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var result = await _fieldService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var vm = MapToEditViewModel(result.Data, isDetailsView: true);
            await PopulateDropdowns(vm);
            ViewBag.IsDetailsView = true;
            ViewBag.Title = "Field Details";
            return View("Edit", vm);
        }

        public async Task<IActionResult> Create()
        {
            // Clear ModelState errors on GET request (page refresh)
            ModelState.Clear();
            
            var vm = new FieldEditViewModel { IsActive = true };
            await PopulateDropdowns(vm);
            return View(vm); // Return empty View (no model)
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FieldEditViewModel fieldVm)
        {
            // Validations removed - directly save
            var field = MapToEntity(fieldVm);
            var result = await _fieldService.CreateAsync(field);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await PopulateDropdowns(fieldVm);
                return View(fieldVm);
            }

            TempData["SuccessMessage"] = "Field created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            ModelState.Clear();
            var result = await _fieldService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var vm = MapToEditViewModel(result.Data);
            await PopulateDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, FieldEditViewModel fieldVm)
        {
            if (id != fieldVm.FieldId) return NotFound();

            // Validations removed - directly update
            var field = MapToEntity(fieldVm);
            var result = await _fieldService.UpdateAsync(field);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await PopulateDropdowns(fieldVm);
                return View(fieldVm);
            }

            TempData["SuccessMessage"] = "Field updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            long? deletedByUserId = currentUser?.Id;
            
            var result = await _fieldService.DeleteAsync(id, deletedByUserId);
            if (!result.Success)
            {
                return Json(new { success = false, message = result.Message });
            }
            return Json(new { success = true, message = "Field deleted successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> Filter([FromBody] Dictionary<string, string> filters)
        {
            try
            {
                var result = await _fieldService.GetFilteredAsync(filters ?? new Dictionary<string, string>());
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var vm = result.Data?.Select(MapToItemViewModel).ToList() ?? new List<FieldItemViewModel>();
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

                var result = await _fieldService.GetFilteredAsync(filters);
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var fields = result.Data ?? new List<Field>();
                var vm = fields.Select(MapToItemViewModel).ToList();

                var allColumns = new List<(string Header, string PropertyName)>
                {
                    ("Field Name", "FieldName"),
                    ("Field Code", "FieldCode"),
                    ("Area Amount", "AreaAmount"),
                    ("Area Type", "AreaType"),
                    ("Farm", "Farm"),
                    ("Soil Type", "SoilType"),
                    ("Irrigation Available", "IrrigationAvailable"),
                    ("Is Active", "IsActive")
                };

                var visibleColumns = allColumns.Where(col => !hiddenColumns.Contains(col.PropertyName)).ToList();
                var columnHeaders = visibleColumns.Select(col => col.Header).ToList();
                var columnIndices = visibleColumns.Select(col => allColumns.IndexOf(allColumns.First(c => c.PropertyName == col.PropertyName))).ToList();

                var stream = _exportToExcel.GenerateExcel(
                    moduleName: "Fields",
                    worksheetName: "Fields",
                    columnHeaders: columnHeaders,
                    data: vm,
                    rowMapper: item =>
                    {
                        var allValues = new List<object>
                        {
                            item.FieldName ?? "",
                            item.FieldCode ?? "",
                            item.AreaAmount?.ToString("N2") ?? "",
                            item.AreaTypeName ?? "N/A",
                            !string.IsNullOrEmpty(item.FarmLicenseNumber) ? item.FarmLicenseNumber : $"Farm #{item.FarmId}",
                            item.SoilType ?? "",
                            item.IrrigationAvailable ? "Yes" : "No",
                            item.IsActive ? "Active" : "Inactive"
                        };
                        return columnIndices.Select(idx => allValues[idx]).ToList();
                    }
                );

                var fileName = $"Fields_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error generating Excel: {ex.Message}" });
            }
        }

        private static FieldItemViewModel MapToItemViewModel(Field entity)
        {
            return new FieldItemViewModel
            {
                FieldId = entity.FieldId,
                FieldName = entity.FieldName,
                FieldCode = entity.FieldCode,
                AreaAmount = entity.AreaAmount,
                AreaTypeName = entity.AreaType?.AreaTypeName,
                FarmId = entity.FarmId,
                FarmLicenseNumber = entity.Farm?.LicenseNumber,
                SoilType = entity.SoilType,
                IrrigationAvailable = entity.IrrigationAvailable,
                IsActive = entity.IsActive
            };
        }

        private static FieldEditViewModel MapToEditViewModel(Field entity, bool isDetailsView = false)
        {
            return new FieldEditViewModel
            {
                FieldId = entity.FieldId,
                FieldName = entity.FieldName,
                FieldCode = entity.FieldCode,
                FarmId = entity.FarmId,
                AreaTypeId = entity.AreaTypeId,
                AreaAmount = entity.AreaAmount,
                CreatedByUserId = entity.CreatedByUserId,
                SoilType = entity.SoilType,
                SlopePercentage = entity.SlopePercentage,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                IrrigationAvailable = entity.IrrigationAvailable,
                IsActive = entity.IsActive,
                BoundaryCoordinates = entity.BoundaryCoordinates,
                Notes = entity.Notes,
                IsDetailsView = isDetailsView
            };
        }

        private static Field MapToEntity(FieldEditViewModel vm)
        {
            return new Field
            {
                FieldId = vm.FieldId,
                FieldName = vm.FieldName,
                FieldCode = vm.FieldCode,
                FarmId = vm.FarmId ?? 0,
                AreaTypeId = vm.AreaTypeId.HasValue ? (int)vm.AreaTypeId.Value : 0,
                AreaAmount = vm.AreaAmount ?? 0,
                CreatedByUserId = vm.CreatedByUserId,
                SoilType = vm.SoilType,
                SlopePercentage = vm.SlopePercentage,
                Latitude = vm.Latitude,
                Longitude = vm.Longitude,
                IrrigationAvailable = vm.IrrigationAvailable,
                IsActive = vm.IsActive,
                BoundaryCoordinates = vm.BoundaryCoordinates,
                Notes = vm.Notes
            };
        }

        private async Task PopulateDropdowns(FieldEditViewModel vm)
        {
            var dropdowns = await _fieldService.GetDropdownDataAsync();
            if (dropdowns.Success)
            {
                vm.Farms = dropdowns.Data.Farms as IEnumerable<SelectListItem> ?? Enumerable.Empty<SelectListItem>();
                vm.AreaTypes = dropdowns.Data.AreaTypes as IEnumerable<SelectListItem> ?? Enumerable.Empty<SelectListItem>();
                vm.Users = dropdowns.Data.Users as IEnumerable<SelectListItem> ?? Enumerable.Empty<SelectListItem>();
            }
            else
            {
                vm.Farms = Enumerable.Empty<SelectListItem>();
                vm.AreaTypes = Enumerable.Empty<SelectListItem>();
                vm.Users = Enumerable.Empty<SelectListItem>();
                TempData["Error"] = dropdowns.Message;
            }
        }
    }
}

