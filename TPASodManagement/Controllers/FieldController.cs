using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class FieldController : Controller
    {
        private readonly IFieldService _fieldService;

        public FieldController(IFieldService fieldService)
        {
            _fieldService = fieldService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _fieldService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(new List<Field>());
            }
            return View(result.Data);
        }

        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var result = await _fieldService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var dropdowns = await _fieldService.GetDropdownDataAsync();
            if (dropdowns.Success)
            {
                ViewData["FarmId"] = dropdowns.Data.Farms;
                ViewData["AreaTypeId"] = dropdowns.Data.AreaTypes;
                ViewData["CreatedByUserId"] = dropdowns.Data.Users;
                ViewBag.FarmId = dropdowns.Data.Farms;
                ViewBag.AreaTypeId = dropdowns.Data.AreaTypes;
                ViewBag.CreatedByUserId = dropdowns.Data.Users;
            }

            ViewBag.IsDetailsView = true;
            ViewBag.Title = "Field Details";
            return View("Edit", result.Data);
        }

        public async Task<IActionResult> Create()
        {
            // Clear ModelState errors on GET request (page refresh)
            ModelState.Clear();
            
            var dropdowns = await _fieldService.GetDropdownDataAsync();
            if (dropdowns.Success)
            {
                ViewData["FarmId"] = dropdowns.Data.Farms;
                ViewData["AreaTypeId"] = dropdowns.Data.AreaTypes;
                ViewData["CreatedByUserId"] = dropdowns.Data.Users;
                ViewBag.FarmId = dropdowns.Data.Farms;
                ViewBag.AreaTypeId = dropdowns.Data.AreaTypes;
                ViewBag.CreatedByUserId = dropdowns.Data.Users;
            }
            else
            {
                TempData["Error"] = dropdowns.Message;
            }
            return View(); // Return empty View (no model)
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Field field)
        {
            // Validations removed - directly save
            var result = await _fieldService.CreateAsync(field);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                var dropdowns = await _fieldService.GetDropdownDataAsync();
                if (dropdowns.Success)
                {
                    ViewBag.FarmId = dropdowns.Data.Farms;
                    ViewBag.AreaTypeId = dropdowns.Data.AreaTypes;
                    ViewBag.CreatedByUserId = dropdowns.Data.Users;
                }
                return View(field);
            }

            TempData["SuccessMessage"] = "Field created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            ModelState.Clear();
            var result = await _fieldService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var dropdowns = await _fieldService.GetDropdownDataAsync();
            if (dropdowns.Success)
            {
                ViewData["FarmId"] = dropdowns.Data.Farms;
                ViewData["AreaTypeId"] = dropdowns.Data.AreaTypes;
                ViewData["CreatedByUserId"] = dropdowns.Data.Users;
                ViewBag.FarmId = dropdowns.Data.Farms;
                ViewBag.AreaTypeId = dropdowns.Data.AreaTypes;
                ViewBag.CreatedByUserId = dropdowns.Data.Users;
            }

            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, Field field)
        {
            if (id != field.FieldId) return NotFound();

            // Validations removed - directly update
            var result = await _fieldService.UpdateAsync(field);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await Edit(id);
                return View(field);
            }

            TempData["SuccessMessage"] = "Field updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _fieldService.DeleteAsync(id);
            if (!result.Success)
            {
                return Json(new { success = false, message = result.Message });
            }
            return Json(new { success = true, message = "Field deleted successfully." });
        }
    }
}

