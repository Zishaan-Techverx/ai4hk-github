using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Controllers
{
    public class FarmController : Controller
    {
        private readonly IFarmService _farmService;

        public FarmController(IFarmService farmService)
        {
            _farmService = farmService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _farmService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(new List<Farm>());
            }
            return View(result.Data);
        }

        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var result = await _farmService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            return View(result.Data);
        }

        public async Task<IActionResult> Create()
        {
            var result = await _farmService.GetDropdownDataAsync();
            if (result.Success && result.Data.AreaTypes != null && result.Data.Organizations != null)
            {
                ViewData["AreaTypeId"] = result.Data.AreaTypes;
                ViewData["OrganizationId"] = result.Data.Organizations;
            }
            else
            {
                TempData["Error"] = result.Message;
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Farm farm)
        {
            if (!ModelState.IsValid)
            {
                await Create(); // reload dropdowns
                return View(farm);
            }

            var result = await _farmService.CreateAsync(farm);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await Create(); // reload dropdowns
                return View(farm);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var result = await _farmService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var dropdowns = await _farmService.GetDropdownDataAsync();
            if (dropdowns.Success && dropdowns.Data.AreaTypes != null && dropdowns.Data.Organizations != null)
            {
                ViewData["AreaTypeId"] = dropdowns.Data.AreaTypes;
                ViewData["OrganizationId"] = dropdowns.Data.Organizations;
            }

            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, Farm farm)
        {
            if (id != farm.FarmId) return NotFound();

            if (!ModelState.IsValid)
            {
                await Edit(id); // reload dropdowns
                return View(farm);
            }

            var result = await _farmService.UpdateAsync(farm);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await Edit(id); // reload dropdowns
                return View(farm);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null) return NotFound();

            var result = await _farmService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            return View(result.Data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var result = await _farmService.DeleteAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
