using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class AreaTypeController : Controller
    {
        private readonly IAreaTypeService _areaTypeService;

        public AreaTypeController(IAreaTypeService areaTypeService)
        {
            _areaTypeService = areaTypeService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _areaTypeService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(new List<AreaType>());
            }
            return View(result.Data);
        }

        // GET: AreaType/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var result = await _areaTypeService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null)
                return NotFound();

            ViewBag.IsDetailsView = true;
            ViewBag.Title = "Area Type Details";
            return View("Edit", result.Data);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AreaType areaType)
        {
            if (ModelState.IsValid)
            {
                var result = await _areaTypeService.CreateAsync(areaType);
                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message;
                    return View(areaType);
                }

                TempData["SuccessMessage"] = "Area type created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(areaType);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var result = await _areaTypeService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null)
                return NotFound();

            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AreaType areaType)
        {
            if (id != areaType.AreaTypeId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _areaTypeService.UpdateAsync(areaType);
                    if (!result.Success)
                    {
                        TempData["ErrorMessage"] = result.Message;
                        return View(areaType);
                    }

                    TempData["SuccessMessage"] = "Area type updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    var exists = await _areaTypeService.ExisTpasync(areaType.AreaTypeId);
                    if (!exists.Data)
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(areaType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _areaTypeService.DeleteAsync(id);
            if (!result.Success)
            {
                return Json(new { success = false, message = result.Message });
            }
            return Json(new { success = true, message = "Area type deleted successfully." });
        }
    }
}
