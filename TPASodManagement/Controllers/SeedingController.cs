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
using TpaSodManagement.ViewModels.Seeding;
using TpaSodManagement.Utilities;
using TpaSodManagement.Database.Entities;
using Microsoft.AspNetCore.Identity;
using TpaSodManagement.Areas.Identity.Data;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class SeedingController : Controller
    {
        private readonly ISeedingService _seedingService;
        private readonly IExportToExcel _exportToExcel;
        private readonly IExportToPdf _exportToPdf;
        private readonly UserManager<TpaSodManagementUser> _userManager;

        public SeedingController(ISeedingService seedingService, IExportToExcel exportToExcel, IExportToPdf exportToPdf, UserManager<TpaSodManagementUser> userManager)
        {
            _seedingService = seedingService;
            _exportToExcel = exportToExcel;
            _exportToPdf = exportToPdf;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            // Set filter columns for the partial view
            ViewBag.FilterColumns = new Dictionary<string, string>
            {
                { "FarmName", "Farm" },
                { "AreaTypeName", "Area Type" },
                { "FieldName", "Field" },
                { "TagStartNumber", "Tag Start Number" },
                { "TagEndNumber", "Tag End Number" },
                { "AreaAmount", "Area Amount" },
                { "SeedingDate", "Seeding Date" },
                { "SeedingMethod", "Seeding Method" },
                { "SeedRatePerUnit", "Seed Rate Per Unit" },
                { "WeatherConditions", "Weather Conditions" },
                { "SoilTemperature", "Soil Temperature" },
                { "SoilMoisture", "Soil Moisture" },
                { "Notes", "Notes" },
                { "UserName", "User" },
                { "IsActive", "Is Active" }
            };
            ViewBag.ModuleName = "Seedings";
            ViewBag.BooleanColumns = new HashSet<string>();
            ViewBag.TriStateColumns = new HashSet<string> { "IsActive" };
            ViewBag.DateColumns = new HashSet<string> { "SeedingDate" };

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
            seedingVm.CreatedDate = DateTimeOffset.UtcNow;

            ValidateTagNumbers(seedingVm);
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(seedingVm);
                return View(seedingVm);
            }

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

            ValidateTagNumbers(seedingVm);
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(seedingVm);
                return View(seedingVm);
            }

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
            var currentUser = await _userManager.GetUserAsync(User);
            long? deletedByUserId = currentUser?.Id;
            
            var result = await _seedingService.DeleteAsync(id, deletedByUserId);
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

                var result = await _seedingService.GetFilteredAsync(filters);
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var seedings = result.Data ?? new List<Seeding>();
                var vm = seedings.Select(MapToItemViewModel).ToList();

                var allColumns = new List<(string Header, string PropertyName)>
                {
                    ("Farm", "Farm"),
                    ("Area Type", "AreaType"),
                    ("Field", "Field"),
                    ("Tag Start Number", "TagStartNumber"),
                    ("Tag End Number", "TagEndNumber"),
                    ("Area Amount", "AreaAmount"),
                    ("Seeding Date", "SeedingDate"),
                    ("Seeding Method", "SeedingMethod"),
                    ("Seed Rate Per Unit", "SeedRatePerUnit"),
                    ("Weather Conditions", "WeatherConditions"),
                    ("Soil Temperature", "SoilTemperature"),
                    ("Soil Moisture", "SoilMoisture"),
                    ("Notes", "Notes"),
                    ("User", "User"),
                    ("Is Active", "IsActive")
                };

                var visibleColumns = allColumns.Where(col => !hiddenColumns.Contains(col.PropertyName)).ToList();
                var columnHeaders = visibleColumns.Select(col => col.Header).ToList();
                var columnIndices = visibleColumns.Select(col => allColumns.IndexOf(allColumns.First(c => c.PropertyName == col.PropertyName))).ToList();

                var stream = _exportToExcel.GenerateExcel(
                    moduleName: "Seedings",
                    worksheetName: "Seedings",
                    columnHeaders: columnHeaders,
                    data: vm,
                    rowMapper: item =>
                    {
                        var allValues = new List<object>
                        {
                            item.FarmDisplay ?? "N/A",
                            item.AreaTypeName ?? $"AreaType #{item.AreaTypeId}",
                            item.FieldName ?? "N/A",
                            item.TagStartNumber,
                            item.TagEndNumber,
                            item.AreaAmount?.ToString("N2") ?? "",
                            item.SeedingDate.HasValue ? item.SeedingDate.Value.ToString("MM/dd/yyyy") : "",
                            item.SeedingMethod ?? "",
                            item.SeedRatePerUnit?.ToString("N2") ?? "",
                            item.WeatherConditions ?? "",
                            item.SoilTemperature?.ToString("N2") ?? "",
                            item.SoilMoisture != null ? Convert.ToDecimal(item.SoilMoisture).ToString("N2") : "",
                            item.Notes ?? "",
                            item.UserName ?? "N/A",
                            item.IsActive ? "Yes" : "No"
                        };
                        return columnIndices.Select(idx => allValues[idx]).ToList();
                    }
                );

                var fileName = $"Seedings_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
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
                    else
                    {
                        var directFilters = JsonSerializer.Deserialize<Dictionary<string, string>>(requestData.GetRawText());
                        if (directFilters != null && !directFilters.ContainsKey("hiddenColumns"))
                            filters = directFilters;
                    }
                    if (requestData.TryGetProperty("hiddenColumns", out var hiddenColumnsElement))
                        hiddenColumns = JsonSerializer.Deserialize<List<string>>(hiddenColumnsElement.GetRawText()) ?? new List<string>();
                    if (requestData.TryGetProperty("headerImageBase64", out var headerImgEl))
                    {
                        var b64 = headerImgEl.GetString();
                        if (!string.IsNullOrEmpty(b64))
                        {
                            try { headerImageBytes = Convert.FromBase64String(b64); } catch { /* ignore */ }
                        }
                    }
                }
                var result = await _seedingService.GetFilteredAsync(filters);
                if (!result.Success)
                    return Json(new { success = false, message = result.Message });
                var seedings = result.Data ?? new List<Seeding>();
                var vm = seedings.Select(MapToItemViewModel).ToList();
                var allColumns = new List<(string Header, string PropertyName)>
                {
                    ("Farm", "Farm"),
                    ("Area Type", "AreaType"),
                    ("Field", "Field"),
                    ("Tag Start Number", "TagStartNumber"),
                    ("Tag End Number", "TagEndNumber"),
                    ("Area Amount", "AreaAmount"),
                    ("Seeding Date", "SeedingDate"),
                    ("Seeding Method", "SeedingMethod"),
                    ("Seed Rate Per Unit", "SeedRatePerUnit"),
                    ("Weather Conditions", "WeatherConditions"),
                    ("Soil Temperature", "SoilTemperature"),
                    ("Soil Moisture", "SoilMoisture"),
                    ("Notes", "Notes"),
                    ("User", "User"),
                    ("Is Active", "IsActive")
                };
                var visibleColumns = allColumns.Where(col => !hiddenColumns.Contains(col.PropertyName)).ToList();
                var columnHeaders = visibleColumns.Select(col => col.Header).ToList();
                var columnIndices = visibleColumns.Select(col => allColumns.IndexOf(allColumns.First(c => c.PropertyName == col.PropertyName))).ToList();
                var stream = _exportToPdf.GeneratePdf("Seedings", columnHeaders, vm, item =>
                {
                    var allValues = new List<object>
                    {
                        item.FarmDisplay ?? "N/A",
                        item.AreaTypeName ?? $"AreaType #{item.AreaTypeId}",
                        item.FieldName ?? "N/A",
                        item.TagStartNumber,
                        item.TagEndNumber,
                        item.AreaAmount?.ToString("N2") ?? "",
                        item.SeedingDate.HasValue ? item.SeedingDate.Value.ToString("MM/dd/yyyy") : "",
                        item.SeedingMethod ?? "",
                        item.SeedRatePerUnit?.ToString("N2") ?? "",
                        item.WeatherConditions ?? "",
                        item.SoilTemperature?.ToString("N2") ?? "",
                        item.SoilMoisture != null ? Convert.ToDecimal(item.SoilMoisture).ToString("N2") : "",
                        item.Notes ?? "",
                        item.UserName ?? "N/A",
                        item.IsActive ? "Yes" : "No"
                    };
                    return columnIndices.Select(idx => allValues[idx]).ToList();
                }, headerImageBytes);
                var fileName = $"Seedings_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
                return File(stream, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error generating PDF: {ex.Message}" });
            }
        }

        private static SeedingItemViewModel MapToItemViewModel(Seeding entity)
        {
            return new SeedingItemViewModel
            {
                SeedingId = entity.SeedingId,
                FarmId = entity.FarmId,
                FarmDisplay = entity.Farm?.FarmName,
                AreaTypeId = entity.AreaTypeId,
                AreaTypeName = entity.AreaType?.AreaTypeName,
                FieldId = entity.FieldId,
                FieldName = entity.Field?.FieldName,
                TagStartNumber = entity.TagStartNumber,
                TagEndNumber = entity.TagEndNumber,
                AreaAmount = entity.AreaAmount,
                SeedingDate = entity.SeedingDate,
                SeedingMethod = entity.SeedingMethod,
                SeedRatePerUnit = entity.SeedRatePerUnit,
                WeatherConditions = entity.WeatherConditions,
                SoilTemperature = entity.SoilTemperature,
                SoilMoisture = entity.SoilMoisture,
                Notes = entity.Notes,
                CreatedDate = entity.CreatedDate,
                UserName = entity.User?.UserName,
                IsActive = entity.IsActive
            };
        }

        private static SeedingEditViewModel MapToEditViewModel(Seeding entity, bool isDetailsView = false)
        {
            return new SeedingEditViewModel
            {
                SeedingId = entity.SeedingId,
                FarmId = entity.FarmId,
                AreaTypeId = entity.AreaTypeId,
                FieldId = entity.FieldId,
                TagStartNumber = entity.TagStartNumber,
                TagEndNumber = entity.TagEndNumber,
                AreaAmount = entity.AreaAmount,
                UserId = entity.UserId,
                SeedingDate = entity.SeedingDate,
                SeedingMethod = entity.SeedingMethod,
                SeedRatePerUnit = entity.SeedRatePerUnit,
                WeatherConditions = entity.WeatherConditions,
                SoilTemperature = entity.SoilTemperature,
                SoilMoisture = entity.SoilMoisture,
                Notes = entity.Notes,
                CreatedDate = entity.CreatedDate,
                IsActive = entity.IsActive,
                IsDetailsView = isDetailsView
            };
        }

        private static Seeding MapToEntity(SeedingEditViewModel vm)
        {
            var fallbackDate = DateOnly.FromDateTime(DateTime.UtcNow);
            return new Seeding
            {
                SeedingId = vm.SeedingId,
                AreaAmount = vm.AreaAmount,
                AreaTypeId = vm.AreaTypeId ?? 0,
                FarmId = vm.FarmId ?? 0,
                FieldId = vm.FieldId,
                TagStartNumber = vm.TagStartNumber ?? 0,
                TagEndNumber = vm.TagEndNumber ?? 0,
                UserId = vm.UserId ?? 0,
                SeedingDate = vm.SeedingDate ?? fallbackDate,
                SeedingMethod = vm.SeedingMethod,
                SeedRatePerUnit = vm.SeedRatePerUnit ?? 0,
                WeatherConditions = vm.WeatherConditions,
                SoilTemperature = vm.SoilTemperature ?? 0,
                SoilMoisture = vm.SoilMoisture,
                Notes = vm.Notes,
                CreatedDate = vm.CreatedDate ?? DateTimeOffset.UtcNow,
                IsActive = vm.IsActive
            };
        }

        private async Task PopulateDropdowns(SeedingEditViewModel vm)
        {
            var dropdowns = await _seedingService.GetDropdownDataAsync();
            if (dropdowns.Success)
            {
                var areaTypeItems = (dropdowns.Data.AreaTypes ?? Enumerable.Empty<SelectListItem>()).ToList();
                vm.Farms = dropdowns.Data.Farms ?? Enumerable.Empty<SelectListItem>();
                vm.Fields = dropdowns.Data.Fields ?? Enumerable.Empty<SelectListItem>();
                vm.Users = dropdowns.Data.Users ?? Enumerable.Empty<SelectListItem>();

                // Default prefill Area Type to "Acres" when no value is selected.
                if (!vm.AreaTypeId.HasValue)
                {
                    var acresOption = areaTypeItems.FirstOrDefault(x =>
                        !string.IsNullOrWhiteSpace(x.Text) &&
                        x.Text.Trim().Contains("acre", StringComparison.OrdinalIgnoreCase));

                    if (acresOption != null)
                    {
                        if (int.TryParse(acresOption.Value, out var acresId))
                        {
                            vm.AreaTypeId = acresId;
                        }
                        else if (long.TryParse(acresOption.Value, out var acresIdLong))
                        {
                            vm.AreaTypeId = (int)acresIdLong;
                        }
                    }
                }

                if (vm.AreaTypeId.HasValue)
                {
                    // Create/edit/details scenario: explicitly mark selected option.
                    foreach (var item in areaTypeItems)
                    {
                        item.Selected = string.Equals(item.Value, vm.AreaTypeId.Value.ToString(), StringComparison.Ordinal);
                    }
                }

                vm.AreaTypes = areaTypeItems;
            }
            else
            {
                vm.AreaTypes = vm.AreaTypes ?? Enumerable.Empty<SelectListItem>();
                vm.Farms = vm.Farms ?? Enumerable.Empty<SelectListItem>();
                vm.Fields = vm.Fields ?? Enumerable.Empty<SelectListItem>();
                vm.Users = vm.Users ?? Enumerable.Empty<SelectListItem>();
                TempData["Error"] = dropdowns.Message;
            }
        }

        private void ValidateTagNumbers(SeedingEditViewModel vm)
        {
            if (!vm.TagStartNumber.HasValue)
            {
                ModelState.AddModelError(nameof(vm.TagStartNumber), "Tag Start Number is required");
            }
            else if (vm.TagStartNumber.Value < 0)
            {
                ModelState.AddModelError(nameof(vm.TagStartNumber), "Tag Start Number must be a positive integer");
            }

            if (!vm.TagEndNumber.HasValue)
            {
                ModelState.AddModelError(nameof(vm.TagEndNumber), "Tag End Number is required");
            }
            else if (vm.TagEndNumber.Value < 0)
            {
                ModelState.AddModelError(nameof(vm.TagEndNumber), "Tag End Number must be a positive integer");
            }

            if (vm.TagStartNumber.HasValue && vm.TagEndNumber.HasValue && vm.TagStartNumber.Value > vm.TagEndNumber.Value)
            {
                ModelState.AddModelError(nameof(vm.TagEndNumber), "Tag End Number must be greater than or equal to Tag Start Number.");
            }
        }
    }
}
