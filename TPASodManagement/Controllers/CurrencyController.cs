using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;

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
                return View(new List<Currency>());
            }
            return View(result.Data);
        }

        // GET: Currency/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var result = await _currencyService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null)
                return NotFound();

            ViewBag.IsDetailsView = true;
            ViewBag.Title = "Currency Details";
            return View("Edit", result.Data); // Same Edit view use karein
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Currency currency)
        {
            if (ModelState.IsValid)
            {
                var result = await _currencyService.CreateAsync(currency);
                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message;
                    return View(currency);
                }

                TempData["SuccessMessage"] = "Currency created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(currency);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var result = await _currencyService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null)
                return NotFound();

            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Currency currency)
        {
            if (id != currency.CurrencyId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _currencyService.UpdateAsync(currency);
                    if (!result.Success)
                    {
                        TempData["ErrorMessage"] = result.Message;
                        return View(currency);
                    }

                    TempData["SuccessMessage"] = "Currency updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    var exists = await _currencyService.ExisTpasync(currency.CurrencyId);
                    if (!exists.Data)
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(currency);
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
    }
}