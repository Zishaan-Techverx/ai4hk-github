using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.Farm;

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
                return View(new List<FarmItemViewModel>());
            }
            var vm = result.Data?.Select(MapToItemViewModel).ToList() ?? new List<FarmItemViewModel>();
            return View(vm);
        }

        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var result = await _farmService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var vm = MapToEditViewModel(result.Data, isDetailsView: true);
            await PopulateDropdowns(vm, result.Data.OrganizationId, result.Data.AreaTypeId);
            ViewBag.IsDetailsView = true;
            ViewBag.Title = "Farm Details";
            return View("Edit", vm);
        }

        public async Task<IActionResult> Create()
        {
            // Clear any existing ModelState errors on page load
            ModelState.Clear();
            
            var vm = new FarmEditViewModel();
            await PopulateDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FarmEditViewModel farmVm)
        {
            // Validations removed - directly save
            var farm = MapToEntity(farmVm);
            var result = await _farmService.CreateAsync(farm);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                await PopulateDropdowns(farmVm);
                return View(farmVm);
            }

            TempData["SuccessMessage"] = "Farm created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var result = await _farmService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var vm = MapToEditViewModel(result.Data);
            await PopulateDropdowns(vm, result.Data.OrganizationId, result.Data.AreaTypeId);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, FarmEditViewModel farmVm)
        {
            if (id != farmVm.FarmId) return NotFound();

            // Validations removed - directly update
            var farm = MapToEntity(farmVm);
            var result = await _farmService.UpdateAsync(farm);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await PopulateDropdowns(farmVm, farmVm.OrganizationId, farmVm.AreaTypeId);
                return View(farmVm);
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

        private static FarmItemViewModel MapToItemViewModel(Farm entity)
        {
            return new FarmItemViewModel
            {
                FarmId = entity.FarmId,
                TotalArea = entity.TotalArea,
                OrganicCertified = entity.OrganicCertified,
                LicenseNumber = entity.LicenseNumber,
                CertificationDetails = entity.CertificationDetails,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                ElevationMeters = entity.ElevationMeters,
                SoilType = entity.SoilType,
                IrrigationType = entity.IrrigationType,
                ClimateZone = entity.ClimateZone,
                AreaTypeName = entity.AreaType?.AreaTypeName,
                OrganizationName = entity.Organization?.OrganizationName
            };
        }

        private static FarmEditViewModel MapToEditViewModel(Farm entity, bool isDetailsView = false)
        {
            return new FarmEditViewModel
            {
                FarmId = entity.FarmId,
                TotalArea = entity.TotalArea,
                OrganicCertified = entity.OrganicCertified,
                LicenseNumber = entity.LicenseNumber,
                CertificationDetails = entity.CertificationDetails,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                ElevationMeters = entity.ElevationMeters,
                SoilType = entity.SoilType,
                IrrigationType = entity.IrrigationType,
                ClimateZone = entity.ClimateZone,
                AreaTypeId = entity.AreaTypeId,
                OrganizationId = entity.OrganizationId,
                IsDetailsView = isDetailsView
            };
        }

        private static Farm MapToEntity(FarmEditViewModel vm)
        {
            return new Farm
            {
                FarmId = vm.FarmId,
                TotalArea = vm.TotalArea,
                OrganicCertified = vm.OrganicCertified,
                LicenseNumber = vm.LicenseNumber,
                CertificationDetails = vm.CertificationDetails,
                Latitude = vm.Latitude,
                Longitude = vm.Longitude,
                ElevationMeters = vm.ElevationMeters,
                SoilType = vm.SoilType,
                IrrigationType = vm.IrrigationType,
                ClimateZone = vm.ClimateZone,
                AreaTypeId = vm.AreaTypeId.HasValue ? (int?)vm.AreaTypeId.Value : null,
                OrganizationId = vm.OrganizationId ?? 0
            };
        }

        private async Task PopulateDropdowns(FarmEditViewModel vm, long? organizationId = null, long? areaTypeId = null)
        {
            var dropdowns = await _farmService.GetDropdownDataAsync(organizationId, areaTypeId.HasValue ? (int?)areaTypeId.Value : null);
            if (dropdowns.Success && dropdowns.Data.AreaTypes != null && dropdowns.Data.Organizations != null)
            {
                vm.AreaTypes = dropdowns.Data.AreaTypes;
                vm.Organizations = dropdowns.Data.Organizations;
            }
            else
            {
                vm.AreaTypes = Enumerable.Empty<SelectListItem>();
                vm.Organizations = Enumerable.Empty<SelectListItem>();
                TempData["Error"] = dropdowns.Message;
            }
        }
    }
}
