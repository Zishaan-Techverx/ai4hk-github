using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.Field;
using OfficeOpenXml;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class FieldController : Controller
    {
        private readonly IFieldService _fieldService;

        public FieldController(IFieldService fieldService)
        {
            _fieldService = fieldService;
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
            ViewBag.BooleanColumns = new HashSet<string> { "IrrigationAvailable", "IsActive" };

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
            var result = await _fieldService.DeleteAsync(id);
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
        public async Task<IActionResult> Print([FromBody] Dictionary<string, string> filters)
        {
            try
            {
                // Get all filtered records (no pagination)
                var result = await _fieldService.GetFilteredAsync(filters ?? new Dictionary<string, string>());
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var fields = result.Data ?? new List<Field>();
                var vm = fields.Select(MapToItemViewModel).ToList();

                // Set EPPlus license context (non-commercial use)
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Fields");

                    // Headers
                    worksheet.Cells[1, 1].Value = "Field Name";
                    worksheet.Cells[1, 2].Value = "Field Code";
                    worksheet.Cells[1, 3].Value = "Area Amount";
                    worksheet.Cells[1, 4].Value = "Area Type";
                    worksheet.Cells[1, 5].Value = "Farm";
                    worksheet.Cells[1, 6].Value = "Soil Type";
                    worksheet.Cells[1, 7].Value = "Irrigation Available";
                    worksheet.Cells[1, 8].Value = "Is Active";

                    // Style headers
                    using (var range = worksheet.Cells[1, 1, 1, 8])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    // Data
                    for (int i = 0; i < vm.Count; i++)
                    {
                        var row = i + 2;
                        worksheet.Cells[row, 1].Value = vm[i].FieldName;
                        worksheet.Cells[row, 2].Value = vm[i].FieldCode;
                        worksheet.Cells[row, 3].Value = vm[i].AreaAmount;
                        worksheet.Cells[row, 4].Value = vm[i].AreaTypeName ?? "N/A";
                        worksheet.Cells[row, 5].Value = !string.IsNullOrEmpty(vm[i].FarmLicenseNumber) ? vm[i].FarmLicenseNumber : $"Farm #{vm[i].FarmId}";
                        worksheet.Cells[row, 6].Value = vm[i].SoilType;
                        worksheet.Cells[row, 7].Value = vm[i].IrrigationAvailable ? "Yes" : "No";
                        worksheet.Cells[row, 8].Value = vm[i].IsActive ? "Active" : "Inactive";
                    }

                    // Auto-fit columns
                    worksheet.Cells.AutoFitColumns();

                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;

                    var fileName = $"Fields_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error generating Excel file: {ex.Message}" });
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
                CreatedByUserId = long.TryParse(entity.CreatedByUserId, out var userId) ? userId : (long?)null,
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
                CreatedByUserId = vm.CreatedByUserId?.ToString() ?? string.Empty,
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

