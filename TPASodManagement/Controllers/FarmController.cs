using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.Farm;
using TpaSodManagement.Utilities;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class FarmController : Controller
    {
        private readonly IFarmService _farmService;
        private readonly IExportToExcel _exportToExcel;

        public FarmController(IFarmService farmService, IExportToExcel exportToExcel)
        {
            _farmService = farmService;
            _exportToExcel = exportToExcel;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            // Set filter columns for the partial view
            ViewBag.FilterColumns = new Dictionary<string, string>
            {
                { "TotalArea", "Total Area" },
                { "OrganicCertified", "Organic Certified" },
                { "LicenseNumber", "License Number" },
                { "CertificationDetails", "Certification Details" },
                { "Latitude", "Latitude" },
                { "Longitude", "Longitude" },
                { "ElevationMeters", "Elevation Meters" },
                { "SoilType", "Soil Type" },
                { "IrrigationType", "Irrigation Type" },
                { "ClimateZone", "Climate Zone" },
                { "AreaType", "Area Type" },
                { "Organization", "Organization" }
            };
            ViewBag.ModuleName = "Farms";
            ViewBag.BooleanColumns = new HashSet<string> { "OrganicCertified" };

            var result = await _farmService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                ViewBag.PageNumber = 1;
                ViewBag.TotalPages = 1;
                ViewBag.TotalCount = 0;
                ViewBag.PageSize = pageSize;
                return View(new List<FarmItemViewModel>());
            }

            var allFarms = result.Data ?? new List<Farm>();
            var totalCount = allFarms.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Apply pagination
            var paginatedFarms = allFarms
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = paginatedFarms.Select(MapToItemViewModel).ToList();

            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;

            return View(vm);
        }

        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var result = await _farmService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var vm = MapToEditViewModel(result.Data, isDetailsView: true);
            await PopulateDropdowns(vm, result.Data.OrganizationId, result.Data.AreaTypeId);
            ViewBag.IsDetailsView = true;
            ViewBag.Title = "Farm Details";
            return View("Edit", vm);
        }

        public async Task<IActionResult> Create()
        {
            // Clear any existing ModelState errors on page load
            ModelState.Clear();
            
            var vm = new FarmEditViewModel();
            await PopulateDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FarmEditViewModel farmVm)
        {
            // Validations removed - directly save
            var farm = MapToEntity(farmVm);
            var result = await _farmService.CreateAsync(farm);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                await PopulateDropdowns(farmVm);
                return View(farmVm);
            }

            TempData["SuccessMessage"] = "Farm created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var result = await _farmService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var vm = MapToEditViewModel(result.Data);
            await PopulateDropdowns(vm, result.Data.OrganizationId, result.Data.AreaTypeId);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, FarmEditViewModel farmVm)
        {
            if (id != farmVm.FarmId) return NotFound();

            // Validations removed - directly update
            var farm = MapToEntity(farmVm);
            var result = await _farmService.UpdateAsync(farm);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await PopulateDropdowns(farmVm, farmVm.OrganizationId, farmVm.AreaTypeId);
                return View(farmVm);
            }

            TempData["SuccessMessage"] = "Farm updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _farmService.DeleteAsync(id);
            if (!result.Success)
            {
                return Json(new { success = false, message = result.Message });
            }
            return Json(new { success = true, message = "Farm deleted successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> Filter([FromBody] Dictionary<string, string> filters)
        {
            try
            {
                var result = await _farmService.GetFilteredAsync(filters ?? new Dictionary<string, string>());
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var vm = result.Data?.Select(MapToItemViewModel).ToList() ?? new List<FarmItemViewModel>();
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

                var result = await _farmService.GetFilteredAsync(filters);
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var farms = result.Data ?? new List<Farm>();
                var vm = farms.Select(MapToItemViewModel).ToList();

                var allColumns = new List<(string Header, string PropertyName)>
                {
                    ("Total Area", "TotalArea"),
                    ("Organic Certified", "OrganicCertified"),
                    ("License Number", "LicenseNumber"),
                    ("Certification Details", "CertificationDetails"),
                    ("Latitude", "Latitude"),
                    ("Longitude", "Longitude"),
                    ("Elevation Meters", "ElevationMeters"),
                    ("Soil Type", "SoilType"),
                    ("Irrigation Type", "IrrigationType"),
                    ("Climate Zone", "ClimateZone"),
                    ("Area Type", "AreaType"),
                    ("Organization", "Organization")
                };

                var visibleColumns = allColumns.Where(col => !hiddenColumns.Contains(col.PropertyName)).ToList();
                var columnHeaders = visibleColumns.Select(col => col.Header).ToList();
                var columnIndices = visibleColumns.Select(col => allColumns.IndexOf(allColumns.First(c => c.PropertyName == col.PropertyName))).ToList();

                var stream = _exportToExcel.GenerateExcel(
                    moduleName: "Farms",
                    worksheetName: "Farms",
                    columnHeaders: columnHeaders,
                    data: vm,
                    rowMapper: item =>
                    {
                        var allValues = new List<object>
                        {
                            item.TotalArea?.ToString("N2") ?? "",
                            item.OrganicCertified ? "Yes" : "No",
                            item.LicenseNumber ?? "",
                            item.CertificationDetails ?? "",
                            item.Latitude?.ToString("N6") ?? "",
                            item.Longitude?.ToString("N6") ?? "",
                            item.ElevationMeters ?? 0,
                            item.SoilType ?? "",
                            item.IrrigationType ?? "",
                            item.ClimateZone ?? "",
                            item.AreaTypeName ?? "",
                            item.OrganizationName ?? ""
                        };
                        return columnIndices.Select(idx => allValues[idx]).ToList();
                    }
                );

                var fileName = $"Farms_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error generating Excel: {ex.Message}" });
            }
        }

        private static FarmItemViewModel MapToItemViewModel(Farm entity)
        {
            return new FarmItemViewModel
            {
                FarmId = entity.FarmId,
                TotalArea = entity.TotalArea,
                OrganicCertified = entity.OrganicCertified,
                LicenseNumber = entity.LicenseNumber,
                CertificationDetails = entity.CertificationDetails,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                ElevationMeters = entity.ElevationMeters,
                SoilType = entity.SoilType,
                IrrigationType = entity.IrrigationType,
                ClimateZone = entity.ClimateZone,
                AreaTypeName = entity.AreaType?.AreaTypeName,
                OrganizationName = entity.Organization?.OrganizationName
            };
        }

        private static FarmEditViewModel MapToEditViewModel(Farm entity, bool isDetailsView = false)
        {
            return new FarmEditViewModel
            {
                FarmId = entity.FarmId,
                TotalArea = entity.TotalArea,
                OrganicCertified = entity.OrganicCertified,
                LicenseNumber = entity.LicenseNumber,
                CertificationDetails = entity.CertificationDetails,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                ElevationMeters = entity.ElevationMeters,
                SoilType = entity.SoilType,
                IrrigationType = entity.IrrigationType,
                ClimateZone = entity.ClimateZone,
                AreaTypeId = entity.AreaTypeId,
                OrganizationId = entity.OrganizationId,
                IsDetailsView = isDetailsView
            };
        }

        private static Farm MapToEntity(FarmEditViewModel vm)
        {
            return new Farm
            {
                FarmId = vm.FarmId,
                TotalArea = vm.TotalArea,
                OrganicCertified = vm.OrganicCertified,
                LicenseNumber = vm.LicenseNumber,
                CertificationDetails = vm.CertificationDetails,
                Latitude = vm.Latitude,
                Longitude = vm.Longitude,
                ElevationMeters = vm.ElevationMeters,
                SoilType = vm.SoilType,
                IrrigationType = vm.IrrigationType,
                ClimateZone = vm.ClimateZone,
                AreaTypeId = vm.AreaTypeId.HasValue ? (int?)vm.AreaTypeId.Value : null,
                OrganizationId = vm.OrganizationId ?? 0
            };
        }

        private async Task PopulateDropdowns(FarmEditViewModel vm, long? organizationId = null, long? areaTypeId = null)
        {
            var dropdowns = await _farmService.GetDropdownDataAsync(organizationId, areaTypeId.HasValue ? (int?)areaTypeId.Value : null);
            if (dropdowns.Success && dropdowns.Data.AreaTypes != null && dropdowns.Data.Organizations != null)
            {
                vm.AreaTypes = dropdowns.Data.AreaTypes;
                vm.Organizations = dropdowns.Data.Organizations;
            }
            else
            {
                vm.AreaTypes = Enumerable.Empty<SelectListItem>();
                vm.Organizations = Enumerable.Empty<SelectListItem>();
                TempData["Error"] = dropdowns.Message;
            }
        }
    }
}
