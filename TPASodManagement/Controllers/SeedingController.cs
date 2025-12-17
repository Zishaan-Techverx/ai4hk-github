using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;
using System;
using TpaSodManagement.ViewModels.Seeding;

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
                return View(new List<SeedingItemViewModel>());
            }
            var vm = result.Data?.Select(MapToItemViewModel).ToList() ?? new List<SeedingItemViewModel>();
            return View(vm);
        }

        // GET: Seeding/Details/{id}
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var result = await _seedingService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var vm = MapToEditViewModel(result.Data, isDetailsView: true);
            await PopulateDropdowns(vm);
            ViewBag.IsDetailsView = true;
            ViewBag.Title = "Seeding Details";
            return View("Edit", vm);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new SeedingEditViewModel();
            await PopulateDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SeedingEditViewModel seedingVm)
        {
            // Set automatic fields
            seedingVm.CreatedDate = DateTimeOffset.UtcNow;
            
            // Remove all ModelState errors - validations removed (same as SaleController and ProductController)
            ModelState.Clear();
            
            // Validations removed - directly save
            var entity = MapToEntity(seedingVm);
            var result = await _seedingService.CreateAsync(entity);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await PopulateDropdowns(seedingVm);
                return View(seedingVm);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var result = await _seedingService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var vm = MapToEditViewModel(result.Data);
            await PopulateDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, SeedingEditViewModel seedingVm)
        {
            if (id != seedingVm.SeedingId) return NotFound();

            // Remove all ModelState errors - validations removed (same as ProductController)
            ModelState.Clear();
            
            // Validations removed - directly update
            var entity = MapToEntity(seedingVm);
            var result = await _seedingService.UpdateAsync(entity);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await PopulateDropdowns(seedingVm);
                return View(seedingVm);
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

        private static SeedingItemViewModel MapToItemViewModel(Seeding entity)
        {
            return new SeedingItemViewModel
            {
                SeedingId = entity.SeedingId,
                AreaAmount = entity.AreaAmount,
                SeedingDate = entity.SeedingDate,
                SeedingMethod = entity.SeedingMethod,
                SeedRatePerUnit = entity.SeedRatePerUnit,
                WeatherConditions = entity.WeatherConditions,
                SoilTemperature = entity.SoilTemperature,
                SoilMoisture = entity.SoilMoisture,
                Notes = entity.Notes,
                CreatedDate = entity.CreatedDate,
                AreaTypeId = entity.AreaTypeId,
                AreaTypeName = entity.AreaType?.AreaTypeName,
                FarmId = entity.FarmId,
                FarmDisplay = !string.IsNullOrEmpty(entity.Farm?.LicenseNumber) ? entity.Farm.LicenseNumber : null,
                FieldId = entity.FieldId,
                FieldName = entity.Field?.FieldName,
                TagRangeId = entity.TagRangeId,
                TagRangeCode = entity.TagRange?.TagRangeCode,
                UserName = entity.User?.UserName
            };
        }

        private static SeedingEditViewModel MapToEditViewModel(Seeding entity, bool isDetailsView = false)
        {
            return new SeedingEditViewModel
            {
                SeedingId = entity.SeedingId,
                AreaAmount = entity.AreaAmount,
                AreaTypeId = entity.AreaTypeId,
                FarmId = entity.FarmId,
                FieldId = entity.FieldId,
                TagRangeId = entity.TagRangeId,
                UserId = entity.UserId,
                SeedingDate = entity.SeedingDate,
                SeedingMethod = entity.SeedingMethod,
                SeedRatePerUnit = entity.SeedRatePerUnit,
                WeatherConditions = entity.WeatherConditions,
                SoilTemperature = entity.SoilTemperature,
                SoilMoisture = entity.SoilMoisture,
                Notes = entity.Notes,
                CreatedDate = entity.CreatedDate,
                IsDetailsView = isDetailsView
            };
        }

        private static Seeding MapToEntity(SeedingEditViewModel vm)
        {
            var fallbackDate = DateOnly.FromDateTime(DateTime.UtcNow);
            return new Seeding
            {
                SeedingId = vm.SeedingId,
                AreaAmount = vm.AreaAmount ?? 0,
                AreaTypeId = vm.AreaTypeId ?? 0,
                FarmId = vm.FarmId ?? 0,
                FieldId = vm.FieldId,
                TagRangeId = vm.TagRangeId ?? 0,
                UserId = vm.UserId ?? 0,
                SeedingDate = vm.SeedingDate ?? fallbackDate,
                SeedingMethod = vm.SeedingMethod,
                SeedRatePerUnit = vm.SeedRatePerUnit ?? 0,
                WeatherConditions = vm.WeatherConditions,
                SoilTemperature = vm.SoilTemperature ?? 0,
                SoilMoisture = vm.SoilMoisture,
                Notes = vm.Notes,
                CreatedDate = vm.CreatedDate ?? DateTimeOffset.UtcNow
            };
        }

        private async Task PopulateDropdowns(SeedingEditViewModel vm)
        {
            var dropdowns = await _seedingService.GetDropdownDataAsync();
            if (dropdowns.Success)
            {
                vm.AreaTypes = dropdowns.Data.AreaTypes ?? Enumerable.Empty<SelectListItem>();
                vm.Farms = dropdowns.Data.Farms ?? Enumerable.Empty<SelectListItem>();
                vm.Fields = dropdowns.Data.Fields ?? Enumerable.Empty<SelectListItem>();
                vm.TagRanges = dropdowns.Data.TagRanges ?? Enumerable.Empty<SelectListItem>();
                vm.Users = dropdowns.Data.Users ?? Enumerable.Empty<SelectListItem>();
            }
            else
            {
                vm.AreaTypes = vm.AreaTypes ?? Enumerable.Empty<SelectListItem>();
                vm.Farms = vm.Farms ?? Enumerable.Empty<SelectListItem>();
                vm.Fields = vm.Fields ?? Enumerable.Empty<SelectListItem>();
                vm.TagRanges = vm.TagRanges ?? Enumerable.Empty<SelectListItem>();
                vm.Users = vm.Users ?? Enumerable.Empty<SelectListItem>();
                TempData["Error"] = dropdowns.Message;
            }
        }
    }
}
