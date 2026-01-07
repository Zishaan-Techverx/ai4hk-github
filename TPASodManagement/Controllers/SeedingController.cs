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
using TpaSodManagement.ViewModels.Seeding;
using OfficeOpenXml;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class SeedingController : Controller
    {
        private readonly ISeedingService _seedingService;

        public SeedingController(ISeedingService seedingService)
        {
            _seedingService = seedingService;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            // Set filter columns for the partial view
            ViewBag.FilterColumns = new Dictionary<string, string>
            {
                { "AreaAmount", "Area Amount" },
                { "SeedingDate", "Seeding Date" },
                { "SeedingMethod", "Seeding Method" },
                { "SeedRatePerUnit", "Seed Rate Per Unit" },
                { "WeatherConditions", "Weather Conditions" },
                { "SoilTemperature", "Soil Temperature" },
                { "SoilMoisture", "Soil Moisture" },
                { "Notes", "Notes" },
                { "CreatedDate", "Created Date" },
                { "AreaTypeName", "Area Type" },
                { "FarmLicenseNumber", "Farm" },
                { "FieldName", "Field" },
                { "TagRangeCode", "Tag Range" },
                { "UserName", "User" }
            };
            ViewBag.ModuleName = "Seedings";
            ViewBag.BooleanColumns = new HashSet<string>();
            ViewBag.DateColumns = new HashSet<string> { "SeedingDate", "CreatedDate" };

            var result = await _seedingService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                ViewBag.PageNumber = 1;
                ViewBag.TotalPages = 1;
                ViewBag.TotalCount = 0;
                ViewBag.PageSize = pageSize;
                return View(new List<SeedingItemViewModel>());
            }

            var allSeedings = result.Data ?? new List<Seeding>();
            var totalCount = allSeedings.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Apply pagination
            var paginatedSeedings = allSeedings
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = paginatedSeedings.Select(MapToItemViewModel).ToList();

            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;

            return View(vm);
        }

        // GET: Seeding/Details/{id}
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var result = await _seedingService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var vm = MapToEditViewModel(result.Data, isDetailsView: true);
            await PopulateDropdowns(vm);
            ViewBag.IsDetailsView = true;
            ViewBag.Title = "Seeding Details";
            return View("Edit", vm);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new SeedingEditViewModel();
            await PopulateDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SeedingEditViewModel seedingVm)
        {
            // Set automatic fields
            seedingVm.CreatedDate = DateTimeOffset.UtcNow;
            
            // Remove all ModelState errors - validations removed (same as SaleController and ProductController)
            ModelState.Clear();
            
            // Validations removed - directly save
            var entity = MapToEntity(seedingVm);
            var result = await _seedingService.CreateAsync(entity);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await PopulateDropdowns(seedingVm);
                return View(seedingVm);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var result = await _seedingService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var vm = MapToEditViewModel(result.Data);
            await PopulateDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, SeedingEditViewModel seedingVm)
        {
            if (id != seedingVm.SeedingId) return NotFound();

            // Remove all ModelState errors - validations removed (same as ProductController)
            ModelState.Clear();
            
            // Validations removed - directly update
            var entity = MapToEntity(seedingVm);
            var result = await _seedingService.UpdateAsync(entity);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await PopulateDropdowns(seedingVm);
                return View(seedingVm);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _seedingService.DeleteAsync(id);
            if (!result.Success)
            {
                return Json(new { success = false, message = result.Message });
            }
            return Json(new { success = true, message = "Seeding deleted successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> Filter([FromBody] Dictionary<string, string> filters)
        {
            try
            {
                var result = await _seedingService.GetFilteredAsync(filters ?? new Dictionary<string, string>());
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var vm = result.Data?.Select(MapToItemViewModel).ToList() ?? new List<SeedingItemViewModel>();
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
                var result = await _seedingService.GetFilteredAsync(filters ?? new Dictionary<string, string>());
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var seedings = result.Data ?? new List<Seeding>();
                var vm = seedings.Select(MapToItemViewModel).ToList();

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Seedings");

                    // Headers
                    worksheet.Cells[1, 1].Value = "Area Amount";
                    worksheet.Cells[1, 2].Value = "Seeding Date";
                    worksheet.Cells[1, 3].Value = "Seeding Method";
                    worksheet.Cells[1, 4].Value = "Seed Rate Per Unit";
                    worksheet.Cells[1, 5].Value = "Weather Conditions";
                    worksheet.Cells[1, 6].Value = "Soil Temperature";
                    worksheet.Cells[1, 7].Value = "Soil Moisture";
                    worksheet.Cells[1, 8].Value = "Notes";
                    worksheet.Cells[1, 9].Value = "Area Type";
                    worksheet.Cells[1, 10].Value = "Farm";
                    worksheet.Cells[1, 11].Value = "Field";
                    worksheet.Cells[1, 12].Value = "Tag Range";
                    worksheet.Cells[1, 13].Value = "User";

                    // Style headers
                    using (var range = worksheet.Cells[1, 1, 1, 13])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    // Data
                    for (int i = 0; i < vm.Count; i++)
                    {
                        var row = i + 2;
                        worksheet.Cells[row, 1].Value = vm[i].AreaAmount;
                        worksheet.Cells[row, 2].Value = vm[i].SeedingDate.ToString();
                        worksheet.Cells[row, 3].Value = vm[i].SeedingMethod;
                        worksheet.Cells[row, 4].Value = vm[i].SeedRatePerUnit;
                        worksheet.Cells[row, 5].Value = vm[i].WeatherConditions;
                        worksheet.Cells[row, 6].Value = vm[i].SoilTemperature;
                        worksheet.Cells[row, 7].Value = vm[i].SoilMoisture;
                        worksheet.Cells[row, 8].Value = vm[i].Notes;
                        worksheet.Cells[row, 9].Value = vm[i].AreaTypeName ?? $"AreaType #{vm[i].AreaTypeId}";
                        worksheet.Cells[row, 10].Value = vm[i].FarmDisplay ?? $"Farm #{vm[i].FarmId}";
                        worksheet.Cells[row, 11].Value = vm[i].FieldName ?? "N/A";
                        worksheet.Cells[row, 12].Value = vm[i].TagRangeCode ?? $"TagRange #{vm[i].TagRangeId}";
                        worksheet.Cells[row, 13].Value = vm[i].UserName ?? "N/A";
                    }

                    worksheet.Cells.AutoFitColumns();

                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;

                    var fileName = $"Seedings_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error generating Excel file: {ex.Message}" });
            }
        }

        private static SeedingItemViewModel MapToItemViewModel(Seeding entity)
        {
            return new SeedingItemViewModel
            {
                SeedingId = entity.SeedingId,
                AreaAmount = entity.AreaAmount,
                SeedingDate = entity.SeedingDate,
                SeedingMethod = entity.SeedingMethod,
                SeedRatePerUnit = entity.SeedRatePerUnit,
                WeatherConditions = entity.WeatherConditions,
                SoilTemperature = entity.SoilTemperature,
                SoilMoisture = entity.SoilMoisture,
                Notes = entity.Notes,
                CreatedDate = entity.CreatedDate,
                AreaTypeId = entity.AreaTypeId,
                AreaTypeName = entity.AreaType?.AreaTypeName,
                FarmId = entity.FarmId,
                FarmDisplay = !string.IsNullOrEmpty(entity.Farm?.LicenseNumber) ? entity.Farm.LicenseNumber : null,
                FieldId = entity.FieldId,
                FieldName = entity.Field?.FieldName,
                TagRangeId = entity.TagRangeId,
                TagRangeCode = entity.TagRange?.TagRangeCode,
                UserName = entity.User?.UserName
            };
        }

        private static SeedingEditViewModel MapToEditViewModel(Seeding entity, bool isDetailsView = false)
        {
            return new SeedingEditViewModel
            {
                SeedingId = entity.SeedingId,
                AreaAmount = entity.AreaAmount,
                AreaTypeId = entity.AreaTypeId,
                FarmId = entity.FarmId,
                FieldId = entity.FieldId,
                TagRangeId = entity.TagRangeId,
                UserId = entity.UserId,
                SeedingDate = entity.SeedingDate,
                SeedingMethod = entity.SeedingMethod,
                SeedRatePerUnit = entity.SeedRatePerUnit,
                WeatherConditions = entity.WeatherConditions,
                SoilTemperature = entity.SoilTemperature,
                SoilMoisture = entity.SoilMoisture,
                Notes = entity.Notes,
                CreatedDate = entity.CreatedDate,
                IsDetailsView = isDetailsView
            };
        }

        private static Seeding MapToEntity(SeedingEditViewModel vm)
        {
            var fallbackDate = DateOnly.FromDateTime(DateTime.UtcNow);
            return new Seeding
            {
                SeedingId = vm.SeedingId,
                AreaAmount = vm.AreaAmount ?? 0,
                AreaTypeId = vm.AreaTypeId ?? 0,
                FarmId = vm.FarmId ?? 0,
                FieldId = vm.FieldId,
                TagRangeId = vm.TagRangeId ?? 0,
                UserId = vm.UserId ?? 0,
                SeedingDate = vm.SeedingDate ?? fallbackDate,
                SeedingMethod = vm.SeedingMethod,
                SeedRatePerUnit = vm.SeedRatePerUnit ?? 0,
                WeatherConditions = vm.WeatherConditions,
                SoilTemperature = vm.SoilTemperature ?? 0,
                SoilMoisture = vm.SoilMoisture,
                Notes = vm.Notes,
                CreatedDate = vm.CreatedDate ?? DateTimeOffset.UtcNow
            };
        }

        private async Task PopulateDropdowns(SeedingEditViewModel vm)
        {
            var dropdowns = await _seedingService.GetDropdownDataAsync();
            if (dropdowns.Success)
            {
                vm.AreaTypes = dropdowns.Data.AreaTypes ?? Enumerable.Empty<SelectListItem>();
                vm.Farms = dropdowns.Data.Farms ?? Enumerable.Empty<SelectListItem>();
                vm.Fields = dropdowns.Data.Fields ?? Enumerable.Empty<SelectListItem>();
                vm.TagRanges = dropdowns.Data.TagRanges ?? Enumerable.Empty<SelectListItem>();
                vm.Users = dropdowns.Data.Users ?? Enumerable.Empty<SelectListItem>();
            }
            else
            {
                vm.AreaTypes = vm.AreaTypes ?? Enumerable.Empty<SelectListItem>();
                vm.Farms = vm.Farms ?? Enumerable.Empty<SelectListItem>();
                vm.Fields = vm.Fields ?? Enumerable.Empty<SelectListItem>();
                vm.TagRanges = vm.TagRanges ?? Enumerable.Empty<SelectListItem>();
                vm.Users = vm.Users ?? Enumerable.Empty<SelectListItem>();
                TempData["Error"] = dropdowns.Message;
            }
        }
    }
}
