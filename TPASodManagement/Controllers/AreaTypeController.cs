using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.AreaType;

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
                return View(new List<AreaTypeItemViewModel>());
            }
            var vm = result.Data?.Select(MapToItemViewModel).ToList() ?? new List<AreaTypeItemViewModel>();
            return View(vm);
        }

        // GET: AreaType/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var result = await _areaTypeService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null)
                return NotFound();

            var vm = MapToEditViewModel(result.Data, isDetailsView: true);
            ViewBag.IsDetailsView = true;
            ViewBag.Title = "Area Type Details";
            return View("Edit", vm);
        }

        public IActionResult Create()
        {
            return View(new AreaTypeEditViewModel { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AreaTypeEditViewModel areaTypeVm)
        {
            if (ModelState.IsValid)
            {
                var areaType = MapToEntity(areaTypeVm);
                var result = await _areaTypeService.CreateAsync(areaType);
                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message;
                    return View(areaTypeVm);
                }

                TempData["SuccessMessage"] = "Area type created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(areaTypeVm);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var result = await _areaTypeService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null)
                return NotFound();

            var vm = MapToEditViewModel(result.Data);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AreaTypeEditViewModel areaTypeVm)
        {
            if (id != areaTypeVm.AreaTypeId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var areaType = MapToEntity(areaTypeVm);
                    var result = await _areaTypeService.UpdateAsync(areaType);
                    if (!result.Success)
                    {
                        TempData["ErrorMessage"] = result.Message;
                        return View(areaTypeVm);
                    }

                    TempData["SuccessMessage"] = "Area type updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    var exists = await _areaTypeService.ExisTpasync(areaTypeVm.AreaTypeId);
                    if (!exists.Data)
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(areaTypeVm);
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

        private static AreaTypeItemViewModel MapToItemViewModel(AreaType entity)
        {
            return new AreaTypeItemViewModel
            {
                AreaTypeId = entity.AreaTypeId,
                AreaTypeName = entity.AreaTypeName,
                UnitAbbreviation = entity.UnitAbbreviation,
                UnitSystem = entity.UnitSystem,
                ConversionToSquareMeters = entity.ConversionToSquareMeters,
                IsActive = entity.IsActive
            };
        }

        private static AreaTypeEditViewModel MapToEditViewModel(AreaType entity, bool isDetailsView = false)
        {
            return new AreaTypeEditViewModel
            {
                AreaTypeId = entity.AreaTypeId,
                AreaTypeName = entity.AreaTypeName,
                UnitAbbreviation = entity.UnitAbbreviation,
                UnitSystem = entity.UnitSystem,
                ConversionToSquareMeters = entity.ConversionToSquareMeters,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreatedDate = entity.CreatedDate,
                IsDetailsView = isDetailsView
            };
        }

        private static AreaType MapToEntity(AreaTypeEditViewModel vm)
        {
            return new AreaType
            {
                AreaTypeId = vm.AreaTypeId,
                AreaTypeName = vm.AreaTypeName,
                UnitAbbreviation = vm.UnitAbbreviation,
                UnitSystem = vm.UnitSystem,
                ConversionToSquareMeters = vm.ConversionToSquareMeters ?? 0,
                Description = vm.Description,
                IsActive = vm.IsActive,
                CreatedDate = vm.CreatedDate ?? DateTimeOffset.UtcNow
            };
        }
    }
}
