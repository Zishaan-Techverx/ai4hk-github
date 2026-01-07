using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.AreaType;
using OfficeOpenXml;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class AreaTypeController : Controller
    {
        private readonly IAreaTypeService _areaTypeService;

        public AreaTypeController(IAreaTypeService areaTypeService)
        {
            _areaTypeService = areaTypeService;
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
            ViewBag.BooleanColumns = new HashSet<string> { "IsActive" };

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
        public async Task<IActionResult> Print([FromBody] Dictionary<string, string> filters)
        {
            try
            {
                // Get all filtered records (no pagination)
                var result = await _areaTypeService.GetFilteredAsync(filters ?? new Dictionary<string, string>());
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var areaTypes = result.Data ?? new List<AreaType>();
                var vm = areaTypes.Select(MapToItemViewModel).ToList();

                // Set EPPlus license context (non-commercial use)
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                // Generate Excel file using EPPlus
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Area Types");

                    // Set header row
                    worksheet.Cells[1, 1].Value = "Area Type Name";
                    worksheet.Cells[1, 2].Value = "Unit Abbreviation";
                    worksheet.Cells[1, 3].Value = "Unit System";
                    worksheet.Cells[1, 4].Value = "Conversion To Square Meters";
                    worksheet.Cells[1, 5].Value = "Is Active";

                    // Style header row
                    using (var range = worksheet.Cells[1, 1, 1, 5])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }

                    // Add data rows
                    for (int i = 0; i < vm.Count; i++)
                    {
                        var row = i + 2;
                        var item = vm[i];
                        worksheet.Cells[row, 1].Value = item.AreaTypeName ?? "";
                        worksheet.Cells[row, 2].Value = item.UnitAbbreviation ?? "";
                        worksheet.Cells[row, 3].Value = item.UnitSystem ?? "";
                        worksheet.Cells[row, 4].Value = item.ConversionToSquareMeters?.ToString("N2") ?? "";
                        worksheet.Cells[row, 5].Value = item.IsActive ? "Yes" : "No";
                    }

                    // Auto-fit columns
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    // Add borders to data cells
                    if (vm.Count > 0)
                    {
                        using (var range = worksheet.Cells[1, 1, vm.Count + 1, 5])
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

                    var fileName = $"AreaTypes_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
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
            var result = await _areaTypeService.DeleteAsync(id);
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
