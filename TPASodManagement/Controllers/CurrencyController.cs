using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.Currency;
using TpaSodManagement.Utilities;
using TpaSodManagement.Database.Entities;
using Microsoft.AspNetCore.Identity;
using TpaSodManagement.Areas.Identity.Data;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class CurrencyController : Controller
    {
        private readonly ICurrencyService _currencyService;
        private readonly IExportToExcel _exportToExcel;
        private readonly IExportToPdf _exportToPdf;
        private readonly UserManager<TpaSodManagementUser> _userManager;

        public CurrencyController(ICurrencyService currencyService, IExportToExcel exportToExcel, IExportToPdf exportToPdf, UserManager<TpaSodManagementUser> userManager)
        {
            _currencyService = currencyService;
            _exportToExcel = exportToExcel;
            _exportToPdf = exportToPdf;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            // Clear any previous success messages from other controllers
            TempData.Remove("SuccessMessage");

            // Set filter columns for the partial view
            ViewBag.FilterColumns = new Dictionary<string, string>
            {
                { "CurrencyCode", "Currency Code" },
                { "CurrencyName", "Currency Name" },
                { "CurrencySymbol", "Currency Symbol" },
                { "DecimalPlaces", "Decimal Places" },
                { "IsActive", "Is Active" }
            };
            ViewBag.ModuleName = "Currencies";
            ViewBag.BooleanColumns = new HashSet<string>();
            ViewBag.TriStateColumns = new HashSet<string> { "IsActive" };

            var result = await _currencyService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                ViewBag.PageNumber = 1;
                ViewBag.TotalPages = 1;
                ViewBag.TotalCount = 0;
                ViewBag.PageSize = pageSize;
                return View(new List<CurrencyItemViewModel>());
            }

            var allCurrencies = result.Data ?? new List<Currency>();
            var totalCount = allCurrencies.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Apply pagination
            var paginatedCurrencies = allCurrencies
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = paginatedCurrencies.Select(MapToItemViewModel).ToList();

            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;

            return View(vm);
        }

        // GET: Currency/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var result = await _currencyService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null)
                return NotFound();

        var vm = MapToEditViewModel(result.Data, isDetailsView: true);
        ViewBag.IsDetailsView = true;
        ViewBag.Title = "Currency Details";
        return View("Edit", vm); // Same Edit view use karein
        }

        public IActionResult Create()
        {
        return View(new CurrencyEditViewModel { IsActive = true, DecimalPlaces = 2 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CurrencyEditViewModel currencyVm)
        {
            if (ModelState.IsValid)
            {
            var currency = MapToEntity(currencyVm);
            var result = await _currencyService.CreateAsync(currency);
                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message;
                return View(currencyVm);
                }

                TempData["SuccessMessage"] = "Currency created successfully.";
                return RedirectToAction(nameof(Index));
            }
        return View(currencyVm);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var result = await _currencyService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null)
                return NotFound();

        var vm = MapToEditViewModel(result.Data);
        return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CurrencyEditViewModel currencyVm)
        {
        if (id != currencyVm.CurrencyId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                var currency = MapToEntity(currencyVm);
                var result = await _currencyService.UpdateAsync(currency);
                    if (!result.Success)
                    {
                        TempData["ErrorMessage"] = result.Message;
                    return View(currencyVm);
                    }

                    TempData["SuccessMessage"] = "Currency updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                var exists = await _currencyService.ExisTpasync(currencyVm.CurrencyId);
                    if (!exists.Data)
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
        return View(currencyVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            long? deletedByUserId = currentUser?.Id;
            
            var result = await _currencyService.DeleteAsync(id, deletedByUserId);
            if (!result.Success)
            {
                return Json(new { success = false, message = result.Message });
            }
            return Json(new { success = true, message = "Currency deleted successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> Filter([FromBody] Dictionary<string, string> filters)
        {
            try
            {
                var result = await _currencyService.GetFilteredAsync(filters ?? new Dictionary<string, string>());
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var vm = result.Data?.Select(MapToItemViewModel).ToList() ?? new List<CurrencyItemViewModel>();
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

                // Get all filtered records (no pagination)
                var result = await _currencyService.GetFilteredAsync(filters);
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var currencies = result.Data ?? new List<Currency>();
                var vm = currencies.Select(MapToItemViewModel).ToList();

                // Define all column headers with their corresponding property names
                var allColumns = new List<(string Header, string PropertyName)>
                {
                    ("Currency Code", "CurrencyCode"),
                    ("Currency Name", "CurrencyName"),
                    ("Currency Symbol", "CurrencySymbol"),
                    ("Decimal Places", "DecimalPlaces"),
                    ("Is Active", "IsActive")
                };

                // Filter out hidden columns
                var visibleColumns = allColumns.Where(col => !hiddenColumns.Contains(col.PropertyName)).ToList();

                // Create filtered column headers and row mapper
                var columnHeaders = visibleColumns.Select(col => col.Header).ToList();
                var columnIndices = visibleColumns.Select(col => allColumns.IndexOf(allColumns.First(c => c.PropertyName == col.PropertyName))).ToList();

                // Generate Excel with only visible columns
                var stream = _exportToExcel.GenerateExcel(
                    moduleName: "Currencies",
                    worksheetName: "Currencies",
                    columnHeaders: columnHeaders,
                    data: vm,
                    rowMapper: item =>
                    {
                        var allValues = new List<object>
                        {
                            item.CurrencyCode ?? "",
                            item.CurrencyName ?? "",
                            item.CurrencySymbol ?? "",
                            item.DecimalPlaces,
                            item.IsActive ? "Yes" : "No"
                        };
                        // Return only visible column values
                        return columnIndices.Select(idx => allValues[idx]).ToList();
                    }
                );

                var fileName = $"Currencies_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
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
                var result = await _currencyService.GetFilteredAsync(filters);
                if (!result.Success)
                    return Json(new { success = false, message = result.Message });
                var currencies = result.Data ?? new List<Currency>();
                var vm = currencies.Select(MapToItemViewModel).ToList();
                var allColumns = new List<(string Header, string PropertyName)>
                {
                    ("Currency Code", "CurrencyCode"),
                    ("Currency Name", "CurrencyName"),
                    ("Currency Symbol", "CurrencySymbol"),
                    ("Decimal Places", "DecimalPlaces"),
                    ("Is Active", "IsActive")
                };
                var visibleColumns = allColumns.Where(col => !hiddenColumns.Contains(col.PropertyName)).ToList();
                var columnHeaders = visibleColumns.Select(col => col.Header).ToList();
                var columnIndices = visibleColumns.Select(col => allColumns.IndexOf(allColumns.First(c => c.PropertyName == col.PropertyName))).ToList();
                var stream = _exportToPdf.GeneratePdf("Currencies", columnHeaders, vm, item =>
                {
                    var allValues = new List<object>
                    {
                        item.CurrencyCode ?? "",
                        item.CurrencyName ?? "",
                        item.CurrencySymbol ?? "",
                        item.DecimalPlaces,
                        item.IsActive ? "Yes" : "No"
                    };
                    return columnIndices.Select(idx => allValues[idx]).ToList();
                }, headerImageBytes);
                var fileName = $"Currencies_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
                return File(stream, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error generating PDF: {ex.Message}" });
            }
        }

    private static CurrencyItemViewModel MapToItemViewModel(Currency entity)
    {
        return new CurrencyItemViewModel
        {
            CurrencyId = entity.CurrencyId,
            CurrencyCode = entity.CurrencyCode ?? string.Empty,
            CurrencyName = entity.CurrencyName ?? string.Empty,
            CurrencySymbol = entity.CurrencySymbol,
            DecimalPlaces = entity.DecimalPlaces,
            IsActive = entity.IsActive
        };
    }

    private static CurrencyEditViewModel MapToEditViewModel(Currency entity, bool isDetailsView = false)
    {
        return new CurrencyEditViewModel
        {
            CurrencyId = entity.CurrencyId,
            CurrencyCode = entity.CurrencyCode ?? string.Empty,
            CurrencyName = entity.CurrencyName ?? string.Empty,
            CurrencySymbol = entity.CurrencySymbol,
            DecimalPlaces = entity.DecimalPlaces,
            IsActive = entity.IsActive,
            CreatedDate = entity.CreatedDate,
            IsDetailsView = isDetailsView
        };
    }

    private static Currency MapToEntity(CurrencyEditViewModel vm)
    {
        return new Currency
        {
            CurrencyId = vm.CurrencyId,
            CurrencyCode = vm.CurrencyCode,
            CurrencyName = vm.CurrencyName,
            CurrencySymbol = vm.CurrencySymbol,
                DecimalPlaces = (byte)vm.DecimalPlaces,
            IsActive = vm.IsActive,
            CreatedDate = vm.CreatedDate ?? DateTimeOffset.UtcNow
        };
    }
    }
}