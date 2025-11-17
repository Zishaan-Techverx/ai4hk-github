using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _productService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(new List<Product>());
            }
            return View(result.Data);
        }

        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var result = await _productService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            return View(result.Data);
        }

        public async Task<IActionResult> Create()
        {
            var dropdowns = await _productService.GetDropdownDataAsync();
            if (dropdowns.Success)
            {
                ViewData["CertificateTypeId"] = dropdowns.Data.CertificateTypes;
                ViewData["CreatedByUserId"] = dropdowns.Data.Users;
                ViewData["CurrencyId"] = dropdowns.Data.Currencies;
                ViewData["ProductCategoryId"] = dropdowns.Data.Categories;
            }
            else
            {
                TempData["Error"] = dropdowns.Message;
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid)
            {
                await Create(); // reload dropdowns
                return View(product);
            }

            var result = await _productService.CreateAsync(product);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await Create(); // reload dropdowns
                return View(product);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var result = await _productService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var dropdowns = await _productService.GetDropdownDataAsync();
            if (dropdowns.Success)
            {
                ViewData["CertificateTypeId"] = dropdowns.Data.CertificateTypes;
                ViewData["CreatedByUserId"] = dropdowns.Data.Users;
                ViewData["CurrencyId"] = dropdowns.Data.Currencies;
                ViewData["ProductCategoryId"] = dropdowns.Data.Categories;
            }

            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, Product product)
        {
            if (id != product.ProductId) return NotFound();

            if (!ModelState.IsValid)
            {
                await Edit(id); // reload dropdowns
                return View(product);
            }

            var result = await _productService.UpdateAsync(product);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await Edit(id); // reload dropdowns
                return View(product);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null) return NotFound();

            var result = await _productService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            return View(result.Data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var result = await _productService.DeleteAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
