using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Controllers
{
    public class SaleController : Controller
    {
        private readonly ISaleService _saleService;

        public SaleController(ISaleService saleService)
        {
            _saleService = saleService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _saleService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(new List<Sale>());
            }
            return View(result.Data);
        }

        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var result = await _saleService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            return View(result.Data);
        }

        public async Task<IActionResult> Create()
        {
            var dropdowns = await _saleService.GetDropdownDataAsync();
            if (dropdowns.Success && dropdowns.Data != null)
            {
                foreach (var kvp in dropdowns.Data)
                    ViewData[kvp.Key] = kvp.Value;
            }
            else
            {
                TempData["Error"] = dropdowns.Message;
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Sale sale)
        {
            if (!ModelState.IsValid)
            {
                await Create(); // Reload dropdowns
                return View(sale);
            }

            var result = await _saleService.CreateAsync(sale);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await Create(); // Reload dropdowns
                return View(sale);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var result = await _saleService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var dropdowns = await _saleService.GetDropdownDataAsync();
            if (dropdowns.Success && dropdowns.Data != null)
            {
                foreach (var kvp in dropdowns.Data)
                    ViewData[kvp.Key] = kvp.Value;
            }

            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, Sale sale)
        {
            if (id != sale.SaleId) return NotFound();
            if (!ModelState.IsValid)
            {
                await Edit(id); // reload dropdowns
                return View(sale);
            }

            var result = await _saleService.UpdateAsync(sale);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await Edit(id); // reload dropdowns
                return View(sale);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null) return NotFound();

            var result = await _saleService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            return View(result.Data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var result = await _saleService.DeleteAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
