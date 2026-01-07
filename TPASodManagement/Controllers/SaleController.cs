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
using Microsoft.AspNetCore.Identity;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.ViewModels.Sale;
using OfficeOpenXml;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class SaleController : Controller
    {
        private readonly ISaleService _saleService;
        private readonly UserManager<TpaSodManagementUser> _userManager;
        private readonly ILogger<SaleController> _logger; 

        public SaleController(
            ISaleService saleService,
            UserManager<TpaSodManagementUser> userManager,
            ILogger<SaleController> logger)
        {
            _saleService = saleService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            // Set filter columns for the partial view
            ViewBag.FilterColumns = new Dictionary<string, string>
            {
                { "SaleNumber", "Sale Number" },
                { "InvoiceNumber", "Invoice Number" },
                { "PurchaseOrderNumber", "Purchase Order Number" },
                { "SaleDate", "Sale Date" },
                { "DueDate", "Due Date" },
                { "SubtotalAmount", "Subtotal Amount" },
                { "TaxAmount", "Tax Amount" },
                { "DiscountAmount", "Discount Amount" },
                { "TotalAmount", "Total Amount" },
                { "PaymentTermsDays", "Payment Terms Days" },
                { "Notes", "Notes" },
                { "CreatedDate", "Created Date" },
                { "UpdatedDate", "Updated Date" },
                { "CurrencyName", "Currency" },
                { "CustomerDisplay", "Customer" },
                { "FarmLicenseNumber", "Farm" },
                { "SaleTypeName", "Sale Type" },
                { "StatusName", "Status" },
                { "UpdatedByUserName", "Updated By User" },
                { "UserName", "User" }
            };
            ViewBag.ModuleName = "Sales";
            ViewBag.BooleanColumns = new HashSet<string>();
            ViewBag.DateColumns = new HashSet<string> { "SaleDate", "DueDate", "CreatedDate", "UpdatedDate" };

            var result = await _saleService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                ViewBag.PageNumber = 1;
                ViewBag.TotalPages = 1;
                ViewBag.TotalCount = 0;
                ViewBag.PageSize = pageSize;
                return View(new List<SaleItemViewModel>());
            }

            var allSales = result.Data ?? new List<Sale>();
            var totalCount = allSales.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Apply pagination
            var paginatedSales = allSales
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = paginatedSales.Select(MapToItemViewModel).ToList();

            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;

            return View(vm);
        }

        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var result = await _saleService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var vm = MapToEditViewModel(result.Data, isDetailsView: true);
            await PopulateDropdowns(vm);
            ViewBag.IsDetailsView = true;
            ViewBag.Title = "Sale Details";
            return View("Edit", vm);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new SaleEditViewModel();
            await PopulateDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaleEditViewModel saleVm)
        {
            // Set automatic fields
            saleVm.CreatedDate = DateTimeOffset.UtcNow;
            saleVm.UpdatedDate = DateTimeOffset.UtcNow;
            
            // Set UpdatedByUserId to current logged-in user or UserId
            if (saleVm.UpdatedByUserId == null || saleVm.UpdatedByUserId == 0)
            {
                if (saleVm.UserId > 0)
                {
                    saleVm.UpdatedByUserId = saleVm.UserId;
                }
                else
                {
                    // Get current logged in user
                    var currentUser = await _userManager.GetUserAsync(HttpContext.User);
                    if (currentUser != null)
                    {
                        saleVm.UpdatedByUserId = currentUser.Id;
                        saleVm.UserId = currentUser.Id; // Also set UserId if not set
                    }
                }
            }
            
            // Remove all ModelState errors - validations removed (same as ProductController)
            ModelState.Clear();
            
            // Validations removed - directly save
            var entity = MapToEntity(saleVm);
            var result = await _saleService.CreateAsync(entity);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await PopulateDropdowns(saleVm);
                return View(saleVm);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var result = await _saleService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var vm = MapToEditViewModel(result.Data);
            await PopulateDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, SaleEditViewModel saleVm)
        {
            if (id != saleVm.SaleId) return NotFound();

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(saleVm);
                return View(saleVm);
            }

            var entity = MapToEntity(saleVm);
            var result = await _saleService.UpdateAsync(entity);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await PopulateDropdowns(saleVm);
                return View(saleVm);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _saleService.DeleteAsync(id);
            if (!result.Success)
            {
                return Json(new { success = false, message = result.Message });
            }
            return Json(new { success = true, message = "Sale deleted successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> Filter([FromBody] Dictionary<string, string> filters)
        {
            try
            {
                var result = await _saleService.GetFilteredAsync(filters ?? new Dictionary<string, string>());
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var vm = result.Data?.Select(MapToItemViewModel).ToList() ?? new List<SaleItemViewModel>();
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
                var result = await _saleService.GetFilteredAsync(filters ?? new Dictionary<string, string>());
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var sales = result.Data ?? new List<Sale>();
                var vm = sales.Select(MapToItemViewModel).ToList();

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Sales");

                    // Headers
                    worksheet.Cells[1, 1].Value = "Sale Number";
                    worksheet.Cells[1, 2].Value = "Invoice Number";
                    worksheet.Cells[1, 3].Value = "Purchase Order Number";
                    worksheet.Cells[1, 4].Value = "Sale Date";
                    worksheet.Cells[1, 5].Value = "Due Date";
                    worksheet.Cells[1, 6].Value = "Subtotal Amount";
                    worksheet.Cells[1, 7].Value = "Tax Amount";
                    worksheet.Cells[1, 8].Value = "Discount Amount";
                    worksheet.Cells[1, 9].Value = "Total Amount";
                    worksheet.Cells[1, 10].Value = "Payment Terms Days";
                    worksheet.Cells[1, 11].Value = "Notes";
                    worksheet.Cells[1, 12].Value = "Currency";
                    worksheet.Cells[1, 13].Value = "Customer";
                    worksheet.Cells[1, 14].Value = "Farm";
                    worksheet.Cells[1, 15].Value = "Sale Type";
                    worksheet.Cells[1, 16].Value = "Status";
                    worksheet.Cells[1, 17].Value = "Updated By User";
                    worksheet.Cells[1, 18].Value = "User";

                    // Style headers
                    using (var range = worksheet.Cells[1, 1, 1, 18])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    // Data
                    for (int i = 0; i < vm.Count; i++)
                    {
                        var row = i + 2;
                        worksheet.Cells[row, 1].Value = vm[i].SaleNumber;
                        worksheet.Cells[row, 2].Value = vm[i].InvoiceNumber;
                        worksheet.Cells[row, 3].Value = vm[i].PurchaseOrderNumber;
                        worksheet.Cells[row, 4].Value = vm[i].SaleDate.ToString();
                        worksheet.Cells[row, 5].Value = vm[i].DueDate?.ToString() ?? "";
                        worksheet.Cells[row, 6].Value = vm[i].SubtotalAmount;
                        worksheet.Cells[row, 7].Value = vm[i].TaxAmount;
                        worksheet.Cells[row, 8].Value = vm[i].DiscountAmount;
                        worksheet.Cells[row, 9].Value = vm[i].TotalAmount;
                        worksheet.Cells[row, 10].Value = vm[i].PaymentTermsDays;
                        worksheet.Cells[row, 11].Value = vm[i].Notes;
                        worksheet.Cells[row, 12].Value = vm[i].CurrencyName ?? "N/A";
                        worksheet.Cells[row, 13].Value = vm[i].CustomerDisplay ?? $"Customer #{vm[i].CustomerId}";
                        worksheet.Cells[row, 14].Value = vm[i].FarmDisplay ?? $"Farm #{vm[i].FarmId}";
                        worksheet.Cells[row, 15].Value = vm[i].SaleTypeName ?? "N/A";
                        worksheet.Cells[row, 16].Value = vm[i].StatusName ?? "N/A";
                        worksheet.Cells[row, 17].Value = vm[i].UpdatedByUserName ?? "N/A";
                        worksheet.Cells[row, 18].Value = vm[i].UserName ?? "N/A";
                    }

                    worksheet.Cells.AutoFitColumns();

                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;

                    var fileName = $"Sales_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error generating Excel file: {ex.Message}" });
            }
        }

        private static SaleItemViewModel MapToItemViewModel(Sale entity)
        {
            return new SaleItemViewModel
            {
                SaleId = entity.SaleId,
                SaleNumber = entity.SaleNumber,
                InvoiceNumber = entity.InvoiceNumber,
                PurchaseOrderNumber = entity.PurchaseOrderNumber,
                SaleDate = entity.SaleDate,
                DueDate = entity.DueDate,
                SubtotalAmount = entity.SubtotalAmount,
                TaxAmount = entity.TaxAmount,
                DiscountAmount = entity.DiscountAmount,
                TotalAmount = entity.TotalAmount,
                PaymentTermsDays = entity.PaymentTermsDays,
                Notes = entity.Notes,
                CreatedDate = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate,
                CurrencyName = entity.Currency?.CurrencyName,
                CustomerId = entity.CustomerId,
                CustomerDisplay = entity.Customer?.Person != null
                    ? $"{entity.Customer.Person.FirstName} {entity.Customer.Person.LastName}".Trim()
                    : !string.IsNullOrEmpty(entity.Customer?.CustomerCode) ? entity.Customer.CustomerCode : null,
                FarmId = entity.FarmId,
                FarmDisplay = !string.IsNullOrEmpty(entity.Farm?.LicenseNumber) ? entity.Farm.LicenseNumber : null,
                SaleTypeName = entity.SaleType?.SaleTypeName,
                StatusName = entity.Status?.StatusName,
                UpdatedByUserName = entity.UpdatedByUser?.UserName,
                UserName = entity.User?.UserName
            };
        }

        private static SaleEditViewModel MapToEditViewModel(Sale entity, bool isDetailsView = false)
        {
            return new SaleEditViewModel
            {
                SaleId = entity.SaleId,
                SaleNumber = entity.SaleNumber,
                InvoiceNumber = entity.InvoiceNumber,
                PurchaseOrderNumber = entity.PurchaseOrderNumber,
                SaleDate = entity.SaleDate,
                DueDate = entity.DueDate,
                SubtotalAmount = entity.SubtotalAmount,
                TaxAmount = entity.TaxAmount,
                DiscountAmount = entity.DiscountAmount,
                TotalAmount = entity.TotalAmount,
                PaymentTermsDays = entity.PaymentTermsDays,
                Notes = entity.Notes,
                UserId = entity.UserId,
                FarmId = entity.FarmId,
                CustomerId = entity.CustomerId,
                SaleTypeId = entity.SaleTypeId,
                StatusId = entity.StatusId,
                CurrencyId = entity.CurrencyId,
                UpdatedByUserId = entity.UpdatedByUserId,
                CreatedDate = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate,
                IsDetailsView = isDetailsView
            };
        }

        private static Sale MapToEntity(SaleEditViewModel vm)
        {
            var fallbackDate = DateOnly.FromDateTime(DateTime.UtcNow);
            return new Sale
            {
                SaleId = vm.SaleId,
                SaleNumber = vm.SaleNumber ?? string.Empty,
                InvoiceNumber = vm.InvoiceNumber,
                PurchaseOrderNumber = vm.PurchaseOrderNumber,
                SaleDate = vm.SaleDate ?? fallbackDate,
                DueDate = vm.DueDate ?? vm.SaleDate ?? fallbackDate,
                SubtotalAmount = vm.SubtotalAmount ?? 0m,
                TaxAmount = vm.TaxAmount ?? 0m,
                DiscountAmount = vm.DiscountAmount ?? 0m,
                TotalAmount = vm.TotalAmount ?? 0m,
                PaymentTermsDays = vm.PaymentTermsDays,
                Notes = vm.Notes,
                UserId = vm.UserId ?? 0,
                FarmId = vm.FarmId ?? 0,
                CustomerId = vm.CustomerId ?? 0,
                SaleTypeId = vm.SaleTypeId ?? 0,
                StatusId = vm.StatusId ?? 0,
                CurrencyId = vm.CurrencyId ?? 0,
                UpdatedByUserId = vm.UpdatedByUserId ?? 0,
                CreatedDate = vm.CreatedDate ?? DateTimeOffset.UtcNow,
                UpdatedDate = vm.UpdatedDate ?? DateTimeOffset.UtcNow
            };
        }

        private async Task PopulateDropdowns(SaleEditViewModel vm)
        {
            var dropdowns = await _saleService.GetDropdownDataAsync();
            if (dropdowns.Success && dropdowns.Data != null)
            {
                vm.Users = dropdowns.Data.ContainsKey("UserId") ? dropdowns.Data["UserId"] : Enumerable.Empty<SelectListItem>();
                vm.Farms = dropdowns.Data.ContainsKey("FarmId") ? dropdowns.Data["FarmId"] : Enumerable.Empty<SelectListItem>();
                vm.Customers = dropdowns.Data.ContainsKey("CustomerId") ? dropdowns.Data["CustomerId"] : Enumerable.Empty<SelectListItem>();
                vm.SaleTypes = dropdowns.Data.ContainsKey("SaleTypeId") ? dropdowns.Data["SaleTypeId"] : Enumerable.Empty<SelectListItem>();
                vm.Statuses = dropdowns.Data.ContainsKey("StatusId") ? dropdowns.Data["StatusId"] : Enumerable.Empty<SelectListItem>();
                vm.Currencies = dropdowns.Data.ContainsKey("CurrencyId") ? dropdowns.Data["CurrencyId"] : Enumerable.Empty<SelectListItem>();
            }
            else
            {
                vm.Users = vm.Users ?? Enumerable.Empty<SelectListItem>();
                vm.Farms = vm.Farms ?? Enumerable.Empty<SelectListItem>();
                vm.Customers = vm.Customers ?? Enumerable.Empty<SelectListItem>();
                vm.SaleTypes = vm.SaleTypes ?? Enumerable.Empty<SelectListItem>();
                vm.Statuses = vm.Statuses ?? Enumerable.Empty<SelectListItem>();
                vm.Currencies = vm.Currencies ?? Enumerable.Empty<SelectListItem>();
                TempData["Error"] = dropdowns.Message;
            }
        }
    }
}
