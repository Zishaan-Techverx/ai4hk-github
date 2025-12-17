using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.Field;

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
                return View(new List<FieldItemViewModel>());
            }
            var vm = result.Data?.Select(MapToItemViewModel).ToList() ?? new List<FieldItemViewModel>();
            return View(vm);
        }

        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var result = await _fieldService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var vm = MapToEditViewModel(result.Data, isDetailsView: true);
            await PopulateDropdowns(vm);
            ViewBag.IsDetailsView = true;
            ViewBag.Title = "Field Details";
            return View("Edit", vm);
        }

        public async Task<IActionResult> Create()
        {
            // Clear ModelState errors on GET request (page refresh)
            ModelState.Clear();
            
            var vm = new FieldEditViewModel { IsActive = true };
            await PopulateDropdowns(vm);
            return View(vm); // Return empty View (no model)
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FieldEditViewModel fieldVm)
        {
            // Validations removed - directly save
            var field = MapToEntity(fieldVm);
            var result = await _fieldService.CreateAsync(field);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await PopulateDropdowns(fieldVm);
                return View(fieldVm);
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

            var vm = MapToEditViewModel(result.Data);
            await PopulateDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, FieldEditViewModel fieldVm)
        {
            if (id != fieldVm.FieldId) return NotFound();

            // Validations removed - directly update
            var field = MapToEntity(fieldVm);
            var result = await _fieldService.UpdateAsync(field);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await PopulateDropdowns(fieldVm);
                return View(fieldVm);
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

        private static FieldItemViewModel MapToItemViewModel(Field entity)
        {
            return new FieldItemViewModel
            {
                FieldId = entity.FieldId,
                FieldName = entity.FieldName,
                FieldCode = entity.FieldCode,
                AreaAmount = entity.AreaAmount,
                AreaTypeName = entity.AreaType?.AreaTypeName,
                FarmId = entity.FarmId,
                FarmLicenseNumber = entity.Farm?.LicenseNumber,
                SoilType = entity.SoilType,
                IrrigationAvailable = entity.IrrigationAvailable,
                IsActive = entity.IsActive
            };
        }

        private static FieldEditViewModel MapToEditViewModel(Field entity, bool isDetailsView = false)
        {
            return new FieldEditViewModel
            {
                FieldId = entity.FieldId,
                FieldName = entity.FieldName,
                FieldCode = entity.FieldCode,
                FarmId = entity.FarmId,
                AreaTypeId = entity.AreaTypeId,
                AreaAmount = entity.AreaAmount,
                CreatedByUserId = long.TryParse(entity.CreatedByUserId, out var userId) ? userId : (long?)null,
                SoilType = entity.SoilType,
                SlopePercentage = entity.SlopePercentage,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                IrrigationAvailable = entity.IrrigationAvailable,
                IsActive = entity.IsActive,
                BoundaryCoordinates = entity.BoundaryCoordinates,
                Notes = entity.Notes,
                IsDetailsView = isDetailsView
            };
        }

        private static Field MapToEntity(FieldEditViewModel vm)
        {
            return new Field
            {
                FieldId = vm.FieldId,
                FieldName = vm.FieldName,
                FieldCode = vm.FieldCode,
                FarmId = vm.FarmId ?? 0,
                AreaTypeId = vm.AreaTypeId.HasValue ? (int)vm.AreaTypeId.Value : 0,
                AreaAmount = vm.AreaAmount ?? 0,
                CreatedByUserId = vm.CreatedByUserId?.ToString() ?? string.Empty,
                SoilType = vm.SoilType,
                SlopePercentage = vm.SlopePercentage,
                Latitude = vm.Latitude,
                Longitude = vm.Longitude,
                IrrigationAvailable = vm.IrrigationAvailable,
                IsActive = vm.IsActive,
                BoundaryCoordinates = vm.BoundaryCoordinates,
                Notes = vm.Notes
            };
        }

        private async Task PopulateDropdowns(FieldEditViewModel vm)
        {
            var dropdowns = await _fieldService.GetDropdownDataAsync();
            if (dropdowns.Success)
            {
                vm.Farms = dropdowns.Data.Farms as IEnumerable<SelectListItem> ?? Enumerable.Empty<SelectListItem>();
                vm.AreaTypes = dropdowns.Data.AreaTypes as IEnumerable<SelectListItem> ?? Enumerable.Empty<SelectListItem>();
                vm.Users = dropdowns.Data.Users as IEnumerable<SelectListItem> ?? Enumerable.Empty<SelectListItem>();
            }
            else
            {
                vm.Farms = Enumerable.Empty<SelectListItem>();
                vm.AreaTypes = Enumerable.Empty<SelectListItem>();
                vm.Users = Enumerable.Empty<SelectListItem>();
                TempData["Error"] = dropdowns.Message;
            }
        }
    }
}

