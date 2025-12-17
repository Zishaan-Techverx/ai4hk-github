using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.ProductCategory;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class ProductCategoryController : Controller
    {
        private readonly IProductCategoryService _productCategoryService;

        public ProductCategoryController(IProductCategoryService productCategoryService)
        {
            _productCategoryService = productCategoryService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _productCategoryService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(new List<ProductCategoryItemViewModel>());
            }
            var vm = result.Data?.Select(MapToItemViewModel).ToList() ?? new List<ProductCategoryItemViewModel>();
            return View(vm);
        }

        // GET: ProductCategory/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var result = await _productCategoryService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var vm = MapToEditViewModel(result.Data, isDetailsView: true);
            ViewBag.IsDetailsView = true;
            ViewBag.Title = "Product Category Details";
            return View("Edit", vm);
        }

        public IActionResult Create()
        {
            return View(new ProductCategoryEditViewModel { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCategoryEditViewModel productCategoryVm)
        {
            if (!ModelState.IsValid)
            {
                return View(productCategoryVm);
            }

            var entity = MapToEntity(productCategoryVm);
            var result = await _productCategoryService.CreateAsync(entity);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(productCategoryVm);
            }

            TempData["SuccessMessage"] = "Product category created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var result = await _productCategoryService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var vm = MapToEditViewModel(result.Data);
            ViewBag.IsDetailsView = false;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductCategoryEditViewModel productCategoryVm)
        {
            if (id != productCategoryVm.ProductCategoryId) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(productCategoryVm);
            }

            var entity = MapToEntity(productCategoryVm);
            var result = await _productCategoryService.UpdateAsync(entity);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(productCategoryVm);
            }

            TempData["SuccessMessage"] = "Product category updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productCategoryService.DeleteAsync(id);
            if (!result.Success)
            {
                return Json(new { success = false, message = result.Message });
            }
            return Json(new { success = true, message = "Product category deleted successfully." });
        }

        private static ProductCategoryItemViewModel MapToItemViewModel(ProductCategory entity)
        {
            return new ProductCategoryItemViewModel
            {
                ProductCategoryId = entity.ProductCategoryId,
                CategoryCode = entity.CategoryCode,
                CategoryName = entity.CategoryName,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreatedDate = entity.CreatedDate
            };
        }

        private static ProductCategoryEditViewModel MapToEditViewModel(ProductCategory entity, bool isDetailsView = false)
        {
            return new ProductCategoryEditViewModel
            {
                ProductCategoryId = entity.ProductCategoryId,
                CategoryCode = entity.CategoryCode,
                CategoryName = entity.CategoryName,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreatedDate = entity.CreatedDate,
                IsDetailsView = isDetailsView
            };
        }

        private static ProductCategory MapToEntity(ProductCategoryEditViewModel vm)
        {
            return new ProductCategory
            {
                ProductCategoryId = vm.ProductCategoryId,
                CategoryCode = vm.CategoryCode,
                CategoryName = vm.CategoryName,
                Description = vm.Description,
                IsActive = vm.IsActive,
                CreatedDate = vm.CreatedDate ?? DateTimeOffset.UtcNow
            };
        }
    }
}
