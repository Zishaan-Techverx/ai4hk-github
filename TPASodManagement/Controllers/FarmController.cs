using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TpaSodManagement.Database;
using TpaSodManagement.Database.Seeders;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.Farm;
using TpaSodManagement.ViewModels.Address;
using TpaSodManagement.Utilities;
using TpaSodManagement.Database.Entities;
using Microsoft.AspNetCore.Identity;
using TpaSodManagement.Areas.Identity.Data;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class FarmController : Controller
    {
        private readonly IFarmService _farmService;
        private readonly IExportToExcel _exportToExcel;
        private readonly IExportToPdf _exportToPdf;
        private readonly UserManager<TpaSodManagementUser> _userManager;
        private readonly ApplicationDbContext _context;

        public FarmController(IFarmService farmService, IExportToExcel exportToExcel, IExportToPdf exportToPdf, UserManager<TpaSodManagementUser> userManager, ApplicationDbContext context)
        {
            _farmService = farmService;
            _exportToExcel = exportToExcel;
            _exportToPdf = exportToPdf;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            // Set filter columns for the partial view
            // Order matches table columns: FarmName, AreaType, TotalArea, ...
            ViewBag.FilterColumns = new Dictionary<string, string>
            {
                { "FarmName", "Farm Name" },
                { "Address", "Address" },
                { "AreaType", "Area Type" },
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
                { "IsActive", "Is Active" }
            };
            ViewBag.ModuleName = "Farms";
            ViewBag.BooleanColumns = new HashSet<string> { "OrganicCertified" };
            ViewBag.TriStateColumns = new HashSet<string> { "IsActive" };

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
            ViewBag.AddressFormatted = FormatAddress(result.Data.Address);
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
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(farmVm);
                return View(farmVm);
            }

            var farm = MapToEntity(farmVm);
            var result = await _farmService.CreateAsync(farm);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                await PopulateDropdowns(farmVm);
                return View(farmVm);
            }

            TempData["SuccessMessage"] = "Farm created successfully.";
            return RedirectToAction(nameof(CreateAddress), new { id = result.Data!.FarmId });
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

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(farmVm, farmVm.OrganizationId, farmVm.AreaTypeId);
                return View(farmVm);
            }

            var farm = MapToEntity(farmVm);
            var result = await _farmService.UpdateAsync(farm);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await PopulateDropdowns(farmVm, farmVm.OrganizationId, farmVm.AreaTypeId);
                return View(farmVm);
            }

            TempData["SuccessMessage"] = "Farm updated successfully.";
            return RedirectToAction(nameof(EditAddress), new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            long? deletedByUserId = currentUser?.Id;
            
            var result = await _farmService.DeleteAsync(id, deletedByUserId);
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
                    ("Farm Name", "FarmName"),
                    ("Address", "Address"),
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
                    ("Is Active", "IsActive")
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
                            item.FarmName ?? "",
                            item.Address ?? "",
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
                            item.IsActive ? "Yes" : "No"
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
                var result = await _farmService.GetFilteredAsync(filters);
                if (!result.Success)
                    return Json(new { success = false, message = result.Message });
                var farms = result.Data ?? new List<Farm>();
                var vm = farms.Select(MapToItemViewModel).ToList();
                var allColumns = new List<(string Header, string PropertyName)>
                {
                    ("Farm Name", "FarmName"),
                    ("Address", "Address"),
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
                    ("Is Active", "IsActive")
                };
                var visibleColumns = allColumns.Where(col => !hiddenColumns.Contains(col.PropertyName)).ToList();
                var columnHeaders = visibleColumns.Select(col => col.Header).ToList();
                var columnIndices = visibleColumns.Select(col => allColumns.IndexOf(allColumns.First(c => c.PropertyName == col.PropertyName))).ToList();
                var stream = _exportToPdf.GeneratePdf("Farms", columnHeaders, vm, item =>
                {
                    var allValues = new List<object>
                    {
                        item.FarmName ?? "",
                        item.Address ?? "",
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
                        item.IsActive ? "Yes" : "No"
                    };
                    return columnIndices.Select(idx => allValues[idx]).ToList();
                }, headerImageBytes);
                var fileName = $"Farms_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
                return File(stream, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error generating PDF: {ex.Message}" });
            }
        }

        private static string FormatAddress(Address? a)
        {
            if (a == null) return string.Empty;
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(a.AddressLine1)) parts.Add(a.AddressLine1);
            if (!string.IsNullOrWhiteSpace(a.AddressLine2)) parts.Add(a.AddressLine2);
            var cityState = new List<string>();
            if (!string.IsNullOrWhiteSpace(a.City)) cityState.Add(a.City);
            if (a.StateProvince != null && !string.IsNullOrWhiteSpace(a.StateProvince.StateName)) cityState.Add(a.StateProvince.StateName);
            if (!string.IsNullOrWhiteSpace(a.PostalCode)) cityState.Add(a.PostalCode);
            if (cityState.Count > 0) parts.Add(string.Join(", ", cityState));
            return string.Join(", ", parts);
        }

        private static FarmItemViewModel MapToItemViewModel(Farm entity)
        {
            return new FarmItemViewModel
            {
                FarmId = entity.FarmId,
                FarmName = entity.FarmName ?? string.Empty,
                Address = FormatAddress(entity.Address),
                TotalArea = entity.TotalArea,
                OrganicCertified = entity.OrganicCertified,
                LicenseNumber = entity.LicenseNumber,
                CertificationDetails = entity.CertificationDetails,
                IsActive = entity.IsActive,
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
                FarmName = entity.FarmName ?? string.Empty,
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
                AddressId = entity.AddressId,
                IsActive = entity.IsActive,
                IsDetailsView = isDetailsView
            };
        }

        private static Farm MapToEntity(FarmEditViewModel vm)
        {
            return new Farm
            {
                FarmId = vm.FarmId,
                FarmName = vm.FarmName ?? string.Empty,
                AddressId = vm.AddressId,
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
                OrganizationId = vm.OrganizationId ?? 0,
                IsActive = vm.IsActive
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

        private async Task PopulateAddressDropdowns(AddressFormViewModel vm)
        {
            vm.AddressTypes = await AddressTypeSeeder.GetAddressTypeSelectListAsync(_context, vm.AddressTypeId);
            vm.StateProvinces = await _context.StateProvinces
                .Where(sp => sp.DeletedDate == null)
                .OrderBy(sp => sp.StateName)
                .Select(sp => new SelectListItem { Value = sp.StateProvinceId.ToString(), Text = sp.StateName ?? "" })
                .ToListAsync();
        }

        public async Task<IActionResult> CreateAddress(long id)
        {
            var result = await _farmService.GetByIdAsync(id);
            if (!result.Success || result.Data == null) return NotFound();
            var farmTypeId = await AddressTypeSeeder.GetAddressTypeIdByNameAsync(_context, "Farm");
            var vm = new AddressFormViewModel
            {
                ParentEntityName = "Farm",
                ParentEntityId = id,
                AddressTypeId = farmTypeId ?? 0,
                IsAddressTypeReadOnly = true,
                IsActive = true
            };
            await PopulateAddressDropdowns(vm);
            return View("AddressForm", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAddress(long id, AddressFormViewModel vm)
        {
            if (id != vm.ParentEntityId) return NotFound();
            var result = await _farmService.GetByIdAsync(id);
            if (!result.Success || result.Data == null) return NotFound();
            var farm = result.Data;

            if (ModelState.IsValid)
            {
                var farmTypeId = await AddressTypeSeeder.GetAddressTypeIdByNameAsync(_context, "Farm");
                var address = new Address
                {
                    AddressLine1 = vm.AddressLine1,
                    AddressLine2 = vm.AddressLine2,
                    City = vm.City,
                    StateProvinceId = vm.StateProvinceId,
                    PostalCode = vm.PostalCode,
                    AddressTypeId = farmTypeId ?? vm.AddressTypeId,
                    Latitude = vm.Latitude,
                    Longitude = vm.Longitude,
                    IsPrimary = vm.IsPrimary,
                    IsVerified = vm.IsVerified,
                    IsActive = vm.IsActive
                };
                _context.Addresses.Add(address);
                await _context.SaveChangesAsync();
                farm.AddressId = address.AddressId;
                _context.Farms.Update(farm);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Farm and address saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            vm.ParentEntityName = "Farm";
            vm.ParentEntityId = id;
            vm.IsAddressTypeReadOnly = true;
            vm.AddressTypeId = (await AddressTypeSeeder.GetAddressTypeIdByNameAsync(_context, "Farm")) ?? vm.AddressTypeId;
            await PopulateAddressDropdowns(vm);
            return View("AddressForm", vm);
        }

        public async Task<IActionResult> EditAddress(long id)
        {
            var result = await _farmService.GetByIdAsync(id);
            if (!result.Success || result.Data == null) return NotFound();
            var farm = result.Data;
            var farmTypeId = await AddressTypeSeeder.GetAddressTypeIdByNameAsync(_context, "Farm");
            var vm = new AddressFormViewModel
            {
                ParentEntityName = "Farm",
                ParentEntityId = id,
                AddressTypeId = farmTypeId ?? 0,
                IsAddressTypeReadOnly = true,
                IsActive = true
            };
            if (farm.AddressId.HasValue && farm.Address != null)
            {
                var a = farm.Address;
                vm.AddressId = a.AddressId;
                vm.AddressLine1 = a.AddressLine1;
                vm.AddressLine2 = a.AddressLine2;
                vm.City = a.City;
                vm.StateProvinceId = a.StateProvinceId;
                vm.PostalCode = a.PostalCode;
                vm.AddressTypeId = farmTypeId ?? a.AddressTypeId;
                vm.Latitude = a.Latitude;
                vm.Longitude = a.Longitude;
                vm.IsPrimary = a.IsPrimary;
                vm.IsVerified = a.IsVerified;
                vm.IsActive = a.IsActive;
            }
            await PopulateAddressDropdowns(vm);
            return View("AddressForm", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAddress(long id, AddressFormViewModel vm)
        {
            if (id != vm.ParentEntityId) return NotFound();
            var result = await _farmService.GetByIdAsync(id);
            if (!result.Success || result.Data == null) return NotFound();
            var farm = result.Data;

            if (ModelState.IsValid)
            {
                Address address;
                if (vm.AddressId.HasValue)
                {
                    address = await _context.Addresses.FindAsync(vm.AddressId.Value);
                    if (address == null) return NotFound();
                    var farmTypeId = await AddressTypeSeeder.GetAddressTypeIdByNameAsync(_context, "Farm");
                    address.AddressLine1 = vm.AddressLine1;
                    address.AddressLine2 = vm.AddressLine2;
                    address.City = vm.City;
                    address.StateProvinceId = vm.StateProvinceId;
                    address.PostalCode = vm.PostalCode;
                    address.AddressTypeId = farmTypeId ?? vm.AddressTypeId;
                    address.Latitude = vm.Latitude;
                    address.Longitude = vm.Longitude;
                    address.IsPrimary = vm.IsPrimary;
                    address.IsVerified = vm.IsVerified;
                    address.IsActive = vm.IsActive;
                    _context.Addresses.Update(address);
                }
                else
                {
                    var farmTypeId = await AddressTypeSeeder.GetAddressTypeIdByNameAsync(_context, "Farm");
                    address = new Address
                    {
                        AddressLine1 = vm.AddressLine1,
                        AddressLine2 = vm.AddressLine2,
                        City = vm.City,
                        StateProvinceId = vm.StateProvinceId,
                        PostalCode = vm.PostalCode,
                        AddressTypeId = farmTypeId ?? vm.AddressTypeId,
                        Latitude = vm.Latitude,
                        Longitude = vm.Longitude,
                        IsPrimary = vm.IsPrimary,
                        IsVerified = vm.IsVerified,
                        IsActive = vm.IsActive
                    };
                    _context.Addresses.Add(address);
                    await _context.SaveChangesAsync();
                }
                await _context.SaveChangesAsync();
                farm.AddressId = address.AddressId;
                _context.Farms.Update(farm);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Address updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            vm.ParentEntityName = "Farm";
            vm.ParentEntityId = id;
            vm.IsAddressTypeReadOnly = true;
            vm.AddressTypeId = (await AddressTypeSeeder.GetAddressTypeIdByNameAsync(_context, "Farm")) ?? vm.AddressTypeId;
            await PopulateAddressDropdowns(vm);
            return View("AddressForm", vm);
        }
    }
}
