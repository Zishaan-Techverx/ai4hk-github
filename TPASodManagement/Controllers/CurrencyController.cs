using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.Currency;
using OfficeOpenXml;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class CurrencyController : Controller
    {
        private readonly ICurrencyService _currencyService;

        public CurrencyController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
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
            ViewBag.BooleanColumns = new HashSet<string> { "IsActive" };

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
            var result = await _currencyService.DeleteAsync(id);
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
        public async Task<IActionResult> Print([FromBody] Dictionary<string, string> filters)
        {
            try
            {
                // Get all filtered records (no pagination)
                var result = await _currencyService.GetFilteredAsync(filters ?? new Dictionary<string, string>());
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var currencies = result.Data ?? new List<Currency>();
                var vm = currencies.Select(MapToItemViewModel).ToList();

                // Set EPPlus license context (non-commercial use)
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                // Generate Excel file using EPPlus
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Currencies");

                    // Set header row
                    worksheet.Cells[1, 1].Value = "Currency Code";
                    worksheet.Cells[1, 2].Value = "Currency Name";
                    worksheet.Cells[1, 3].Value = "Currency Symbol";
                    worksheet.Cells[1, 4].Value = "Decimal Places";
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
                        worksheet.Cells[row, 1].Value = item.CurrencyCode ?? "";
                        worksheet.Cells[row, 2].Value = item.CurrencyName ?? "";
                        worksheet.Cells[row, 3].Value = item.CurrencySymbol ?? "";
                        worksheet.Cells[row, 4].Value = item.DecimalPlaces;
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

                    var fileName = $"Currencies_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error generating Excel: {ex.Message}" });
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