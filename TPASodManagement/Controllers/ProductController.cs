using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.Product;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly UserManager<TpaSodManagementUser> _userManager;
        private readonly SodDbContext _context;

        public ProductController(
            IProductService productService, 
            UserManager<TpaSodManagementUser> userManager,
            SodDbContext context)
        {
            _productService = productService;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _productService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(new List<ProductItemViewModel>());
            }
            var vm = result.Data?.Select(MapToItemViewModel).ToList() ?? new List<ProductItemViewModel>();
            return View(vm);
        }

        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var result = await _productService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var vm = MapToEditViewModel(result.Data, isDetailsView: true);
            await PopulateDropdowns(vm);
            ViewBag.IsDetailsView = true;
            ViewBag.Title = "Product Details";
            return View("Edit", vm);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new ProductEditViewModel { IsActive = true };
            await PopulateDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductEditViewModel productVm)
        {
            // Auto-set CreatedByUserId - get current logged in user's TpaSodManagementUser
            if (productVm.CreatedByUserId == null || productVm.CreatedByUserId == 0)
            {
                var currentIdentityUser = await _userManager.GetUserAsync(HttpContext.User);
                if (currentIdentityUser != null)
                {
                    productVm.CreatedByUserId = currentIdentityUser.Id;
                }
                else
                {
                    TempData["Error"] = "User not authenticated. Please login again.";
                    await PopulateDropdowns(productVm);
                    return View(productVm);
                }
            }

            // Set CreatedDate if not set
            if (productVm.CreatedDate == null || productVm.CreatedDate == default)
            {
                productVm.CreatedDate = DateTimeOffset.UtcNow;
            }

            // Remove all ModelState errors - validations removed
            ModelState.Clear();

            var entity = MapToEntity(productVm);
            var result = await _productService.CreateAsync(entity);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await PopulateDropdowns(productVm);
                return View(productVm);
            }

            TempData["SuccessMessage"] = "Product created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var result = await _productService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var vm = MapToEditViewModel(result.Data);
            await PopulateDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, ProductEditViewModel productVm)
        {
            if (id != productVm.ProductId) return NotFound();

            // Auto-set CreatedByUserId if missing
            if (productVm.CreatedByUserId == null || productVm.CreatedByUserId == 0)
            {
                var currentIdentityUser = await _userManager.GetUserAsync(HttpContext.User);
                if (currentIdentityUser != null)
                {
                    productVm.CreatedByUserId = currentIdentityUser.Id;
                }
            }

            // Remove all ModelState errors - validations removed
            ModelState.Clear();

            var entity = MapToEntity(productVm);
            var result = await _productService.UpdateAsync(entity);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await PopulateDropdowns(productVm);
                return View(productVm);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _productService.DeleteAsync(id);
            if (!result.Success)
            {
                return Json(new { success = false, message = result.Message });
            }
            return Json(new { success = true, message = "Product deleted successfully." });
        }

        private static ProductItemViewModel MapToItemViewModel(Product entity)
        {
            return new ProductItemViewModel
            {
                ProductId = entity.ProductId,
                ProductCode = entity.ProductCode,
                ProductName = entity.ProductName,
                UnitOfMeasure = entity.UnitOfMeasure,
                StandardPrice = entity.StandardPrice,
                RequiresCertificate = entity.RequiresCertificate,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreatedDate = entity.CreatedDate,
                CertificateTypeName = entity.CertificateType?.CertificateTypeName,
                CreatedByUserName = entity.CreatedByUser?.UserName,
                CurrencyCode = entity.Currency?.CurrencyCode,
                ProductCategoryName = entity.ProductCategory?.CategoryName
            };
        }

        private static ProductEditViewModel MapToEditViewModel(Product entity, bool isDetailsView = false)
        {
            return new ProductEditViewModel
            {
                ProductId = entity.ProductId,
                ProductCode = entity.ProductCode,
                ProductName = entity.ProductName,
                UnitOfMeasure = entity.UnitOfMeasure,
                StandardPrice = entity.StandardPrice,
                RequiresCertificate = entity.RequiresCertificate,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CertificateTypeId = entity.CertificateTypeId,
                CreatedByUserId = entity.CreatedByUserId,
                CurrencyId = entity.CurrencyId,
                ProductCategoryId = entity.ProductCategoryId,
                CreatedDate = entity.CreatedDate,
                IsDetailsView = isDetailsView
            };
        }

        private static Product MapToEntity(ProductEditViewModel vm)
        {
            return new Product
            {
                ProductId = vm.ProductId,
                ProductCode = vm.ProductCode,
                ProductName = vm.ProductName,
                UnitOfMeasure = vm.UnitOfMeasure,
                StandardPrice = vm.StandardPrice,
                RequiresCertificate = vm.RequiresCertificate,
                Description = vm.Description,
                IsActive = vm.IsActive,
                CertificateTypeId = vm.CertificateTypeId,
                CreatedByUserId = vm.CreatedByUserId ?? 0,
                CurrencyId = vm.CurrencyId ?? 0,
                ProductCategoryId = vm.ProductCategoryId ?? 0,
                CreatedDate = vm.CreatedDate ?? DateTimeOffset.UtcNow
            };
        }

        private async Task PopulateDropdowns(ProductEditViewModel vm)
        {
            var dropdowns = await _productService.GetDropdownDataAsync();
            if (dropdowns.Success)
            {
                vm.CertificateTypes = dropdowns.Data.CertificateTypes ?? Enumerable.Empty<SelectListItem>();
                vm.Users = dropdowns.Data.Users ?? Enumerable.Empty<SelectListItem>();
                vm.Currencies = dropdowns.Data.Currencies ?? Enumerable.Empty<SelectListItem>();
                vm.Categories = dropdowns.Data.Categories ?? Enumerable.Empty<SelectListItem>();
            }
            else
            {
                vm.CertificateTypes = Enumerable.Empty<SelectListItem>();
                vm.Users = Enumerable.Empty<SelectListItem>();
                vm.Currencies = Enumerable.Empty<SelectListItem>();
                vm.Categories = Enumerable.Empty<SelectListItem>();
                TempData["Error"] = dropdowns.Message;
            }
        }
    }
}
