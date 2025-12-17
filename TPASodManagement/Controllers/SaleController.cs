using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;
using System;
using Microsoft.AspNetCore.Identity;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.ViewModels.Sale;

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

        public async Task<IActionResult> Index()
        {
            var result = await _saleService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(new List<SaleItemViewModel>());
            }
            var vm = result.Data?.Select(MapToItemViewModel).ToList() ?? new List<SaleItemViewModel>();
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
