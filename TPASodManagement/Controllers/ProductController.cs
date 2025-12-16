using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;

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
                return View(new List<Product>());
            }
            return View(result.Data);
        }

        public async Task<IActionResult> Details(long? id)
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
                
                // Add ViewBag for view compatibility
                ViewBag.CertificateTypeId = dropdowns.Data.CertificateTypes;
                ViewBag.CreatedByUserId = dropdowns.Data.Users;
                ViewBag.CurrencyId = dropdowns.Data.Currencies;
                ViewBag.ProductCategoryId = dropdowns.Data.Categories;
            }

            ViewBag.IsDetailsView = true;
            ViewBag.Title = "Product Details";
            return View("Edit", result.Data);
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
                
                // Add ViewBag for view compatibility (Customer module pattern)
                ViewBag.CertificateTypeId = dropdowns.Data.CertificateTypes;
                ViewBag.CreatedByUserId = dropdowns.Data.Users;
                ViewBag.CurrencyId = dropdowns.Data.Currencies;
                ViewBag.ProductCategoryId = dropdowns.Data.Categories;
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
            // Manually extract dropdown values if model binding failed
            if (product.ProductCategoryId == 0 && Request.Form.TryGetValue("ProductCategoryId", out var categoryIdValue))
            {
                if (int.TryParse(categoryIdValue.ToString(), out int categoryId) && categoryId > 0)
                {
                    product.ProductCategoryId = categoryId;
                    ModelState.Remove("ProductCategoryId");
                }
            }

            // Auto-set CreatedByUserId - get current logged in user's TpaSodManagementUser
            if (product.CreatedByUserId == 0)
            {
                if (Request.Form.TryGetValue("CreatedByUserId", out var userIdValue))
                {
                    if (long.TryParse(userIdValue.ToString(), out long userId) && userId > 0)
                    {
                        product.CreatedByUserId = userId;
                    }
                }
                else
                {
                    // Get current logged in TpaSodManagementUser
                    var currentIdentityUser = await _userManager.GetUserAsync(HttpContext.User);
                    
                    if (currentIdentityUser != null)
                    {
                        product.CreatedByUserId = currentIdentityUser.Id;
                    }
                    else
                    {
                        TempData["Error"] = "User not authenticated. Please login again.";
                        var dropdowns = await _productService.GetDropdownDataAsync();
                        if (dropdowns.Success)
                        {
                            ViewData["CertificateTypeId"] = dropdowns.Data.CertificateTypes;
                            ViewData["CreatedByUserId"] = dropdowns.Data.Users;
                            ViewData["CurrencyId"] = dropdowns.Data.Currencies;
                            ViewData["ProductCategoryId"] = dropdowns.Data.Categories;
                            ViewBag.CertificateTypeId = dropdowns.Data.CertificateTypes;
                            ViewBag.CreatedByUserId = dropdowns.Data.Users;
                            ViewBag.CurrencyId = dropdowns.Data.Currencies;
                            ViewBag.ProductCategoryId = dropdowns.Data.Categories;
                        }
                        return View(product);
                    }
                }
            }

            // Set CreatedDate if not set
            if (product.CreatedDate == default)
            {
                product.CreatedDate = DateTimeOffset.UtcNow;
            }

            // Remove all ModelState errors - validations removed
            ModelState.Clear();

            // Validations removed - directly save
            var result = await _productService.CreateAsync(product);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                var dropdowns = await _productService.GetDropdownDataAsync();
                if (dropdowns.Success)
                {
                    ViewData["CertificateTypeId"] = dropdowns.Data.CertificateTypes;
                    ViewData["CreatedByUserId"] = dropdowns.Data.Users;
                    ViewData["CurrencyId"] = dropdowns.Data.Currencies;
                    ViewData["ProductCategoryId"] = dropdowns.Data.Categories;
                    ViewBag.CertificateTypeId = dropdowns.Data.CertificateTypes;
                    ViewBag.CreatedByUserId = dropdowns.Data.Users;
                    ViewBag.CurrencyId = dropdowns.Data.Currencies;
                    ViewBag.ProductCategoryId = dropdowns.Data.Categories;
                }
                return View(product);
            }

            TempData["SuccessMessage"] = "Product created successfully.";
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
                
                // Add ViewBag for view compatibility
                ViewBag.CertificateTypeId = dropdowns.Data.CertificateTypes;
                ViewBag.CreatedByUserId = dropdowns.Data.Users;
                ViewBag.CurrencyId = dropdowns.Data.Currencies;
                ViewBag.ProductCategoryId = dropdowns.Data.Categories;
            }

            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, Product product)
        {
            if (id != product.ProductId) return NotFound();

            // Manually extract dropdown values if model binding failed
            if (product.ProductCategoryId == 0 && Request.Form.TryGetValue("ProductCategoryId", out var categoryIdValue))
            {
                if (int.TryParse(categoryIdValue.ToString(), out int categoryId) && categoryId > 0)
                {
                    product.ProductCategoryId = categoryId;
                    ModelState.Remove("ProductCategoryId"); // Remove ModelState error
                }
            }

            if (product.CreatedByUserId == 0 && Request.Form.TryGetValue("CreatedByUserId", out var userIdValue))
            {
                if (long.TryParse(userIdValue.ToString(), out long userId) && userId > 0)
                {
                    product.CreatedByUserId = userId;
                    ModelState.Remove("CreatedByUserId"); // Remove ModelState error
                }
            }

            // Remove all ModelState errors - validations removed
            ModelState.Clear();

            // Validations removed - directly update
            var result = await _productService.UpdateAsync(product);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await Edit(id); // reload dropdowns
                return View(product);
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
    }
}
