using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.Currency;

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

        public async Task<IActionResult> Index()
        {
            // Clear any previous success messages from other controllers
            TempData.Remove("SuccessMessage");

            var result = await _currencyService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
            return View(new List<CurrencyItemViewModel>());
            }
        var vm = result.Data?.Select(MapToItemViewModel).ToList() ?? new List<CurrencyItemViewModel>();
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