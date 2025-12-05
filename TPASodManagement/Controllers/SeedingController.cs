using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class SeedingController : Controller
    {
        private readonly ISeedingService _seedingService;

        public SeedingController(ISeedingService seedingService)
        {
            _seedingService = seedingService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _seedingService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(new List<Seeding>());
            }
            return View(result.Data);
        }

        // GET: Seeding/Details/{id}
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var result = await _seedingService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var dropdowns = await _seedingService.GetDropdownDataAsync();
            if (dropdowns.Success)
            {
                ViewData["AreaTypeId"] = dropdowns.Data.AreaTypes;
                ViewData["FarmId"] = dropdowns.Data.Farms;
                ViewData["FieldId"] = dropdowns.Data.Fields;
                ViewData["TagRangeId"] = dropdowns.Data.TagRanges;
                ViewData["UserId"] = dropdowns.Data.Users;
            }

            ViewBag.IsDetailsView = true;
            ViewBag.Title = "Seeding Details";
            return View("Edit", result.Data);
        }

        public async Task<IActionResult> Create()
        {
            var dropdowns = await _seedingService.GetDropdownDataAsync();
            if (dropdowns.Success)
            {
                ViewData["AreaTypeId"] = dropdowns.Data.AreaTypes;
                ViewData["FarmId"] = dropdowns.Data.Farms;
                ViewData["FieldId"] = dropdowns.Data.Fields;
                ViewData["TagRangeId"] = dropdowns.Data.TagRanges;
                ViewData["UserId"] = dropdowns.Data.Users;
            }
            else
            {
                TempData["Error"] = dropdowns.Message;
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Seeding seeding)
        {
            if (!ModelState.IsValid)
            {
                await Create(); // reload dropdowns
                return View(seeding);
            }

            var result = await _seedingService.CreateAsync(seeding);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await Create(); // reload dropdowns
                return View(seeding);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var result = await _seedingService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var dropdowns = await _seedingService.GetDropdownDataAsync();
            if (dropdowns.Success)
            {
                ViewData["AreaTypeId"] = dropdowns.Data.AreaTypes;
                ViewData["FarmId"] = dropdowns.Data.Farms;
                ViewData["FieldId"] = dropdowns.Data.Fields;
                ViewData["TagRangeId"] = dropdowns.Data.TagRanges;
                ViewData["UserId"] = dropdowns.Data.Users;
            }

            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, Seeding seeding)
        {
            if (id != seeding.SeedingId) return NotFound();

            if (!ModelState.IsValid)
            {
                await Edit(id); // reload dropdowns
                return View(seeding);
            }

            var result = await _seedingService.UpdateAsync(seeding);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await Edit(id); // reload dropdowns
                return View(seeding);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _seedingService.DeleteAsync(id);
            if (!result.Success)
            {
                return Json(new { success = false, message = result.Message });
            }
            return Json(new { success = true, message = "Seeding deleted successfully." });
        }
    }
}
