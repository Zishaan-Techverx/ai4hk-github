using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;

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
                return View(new List<ProductCategory>());
            }
            return View(result.Data);
        }

        // GET: ProductCategory/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var result = await _productCategoryService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            ViewBag.IsDetailsView = true;
            ViewBag.Title = "Product Category Details";
            return View("Edit", result.Data);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCategory productCategory)
        {
            if (!ModelState.IsValid)
            {
                return View(productCategory);
            }

            var result = await _productCategoryService.CreateAsync(productCategory);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(productCategory);
            }

            TempData["SuccessMessage"] = "Product category created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var result = await _productCategoryService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            ViewBag.IsDetailsView = false;
            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductCategory productCategory)
        {
            if (id != productCategory.ProductCategoryId) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(productCategory);
            }

            var result = await _productCategoryService.UpdateAsync(productCategory);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(productCategory);
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
    }
}
