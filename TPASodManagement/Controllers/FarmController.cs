using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Controllers
{
    [Authorize]
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

            var dropdowns = await _farmService.GetDropdownDataAsync();
            if (dropdowns.Success && dropdowns.Data.AreaTypes != null && dropdowns.Data.Organizations != null)
            {
                ViewBag.AreaTypeId = dropdowns.Data.AreaTypes;
                ViewBag.OrganizationId = dropdowns.Data.Organizations;
            }

            ViewBag.IsDetailsView = true;
            ViewBag.Title = "Farm Details";
            return View("Edit", result.Data);
        }

        public async Task<IActionResult> Create()
        {
            // Clear any existing ModelState errors on page load
            ModelState.Clear();
            
            var result = await _farmService.GetDropdownDataAsync();
            if (result.Success && result.Data.AreaTypes != null && result.Data.Organizations != null)
            {
                ViewBag.AreaTypeId = result.Data.AreaTypes;
                ViewBag.OrganizationId = result.Data.Organizations;
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
            // Validations removed - directly save
            var result = await _farmService.CreateAsync(farm);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                var dropdowns = await _farmService.GetDropdownDataAsync();
                if (dropdowns.Success && dropdowns.Data.AreaTypes != null && dropdowns.Data.Organizations != null)
                {
                    ViewBag.AreaTypeId = dropdowns.Data.AreaTypes;
                    ViewBag.OrganizationId = dropdowns.Data.Organizations;
                }
                return View(farm);
            }

            TempData["SuccessMessage"] = "Farm created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var result = await _farmService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            // Pass the farm's OrganizationId and AreaTypeId to preserve selected values
            var dropdowns = await _farmService.GetDropdownDataAsync(
                result.Data.OrganizationId, 
                result.Data.AreaTypeId
            );
            if (dropdowns.Success && dropdowns.Data.AreaTypes != null && dropdowns.Data.Organizations != null)
            {
                ViewBag.AreaTypeId = dropdowns.Data.AreaTypes;
                ViewBag.OrganizationId = dropdowns.Data.Organizations;
            }

            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, Farm farm)
        {
            if (id != farm.FarmId) return NotFound();

            // Validations removed - directly update
            var result = await _farmService.UpdateAsync(farm);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                var dropdowns = await _farmService.GetDropdownDataAsync(farm.OrganizationId, farm.AreaTypeId);
                if (dropdowns.Success && dropdowns.Data.AreaTypes != null && dropdowns.Data.Organizations != null)
                {
                    ViewBag.AreaTypeId = dropdowns.Data.AreaTypes;
                    ViewBag.OrganizationId = dropdowns.Data.Organizations;
                }
                return View(farm);
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
    }
}
