using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.Product;
using TpaSodManagement.Utilities;
using TpaSodManagement.Database.Entities;
using TpaSodManagement.Database;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly UserManager<TpaSodManagementUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IExportToExcel _exportToExcel;

        public ProductController(
            IProductService productService, 
            UserManager<TpaSodManagementUser> userManager,
            ApplicationDbContext context,
            IExportToExcel exportToExcel)
        {
            _productService = productService;
            _userManager = userManager;
            _context = context;
            _exportToExcel = exportToExcel;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            // Set filter columns for the partial view
            ViewBag.FilterColumns = new Dictionary<string, string>
            {
                { "ProductName", "Product Name" },
                { "ProductCategoryName", "Product Category" },
                { "ProductCode", "Product Code" },
                { "RequiresCertificate", "Requires Certificate" },
                { "UnitOfMeasure", "Unit Of Measure" },
                { "StandardPrice", "Standard Price" },
                { "CertificateTypeName", "Certificate Type" },
                { "CreatedByUserName", "Created By User" },
                { "CurrencyCode", "Currency" },
                { "Description", "Description" },
                { "IsActive", "Is Active" }
            };
            ViewBag.ModuleName = "Products";
            ViewBag.BooleanColumns = new HashSet<string> { "RequiresCertificate" };
            ViewBag.TriStateColumns = new HashSet<string> { "IsActive" };
            ViewBag.DateColumns = new HashSet<string>();

            var result = await _productService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                ViewBag.PageNumber = 1;
                ViewBag.TotalPages = 1;
                ViewBag.TotalCount = 0;
                ViewBag.PageSize = pageSize;
                return View(new List<ProductItemViewModel>());
            }

            var allProducts = result.Data ?? new List<Product>();
            var totalCount = allProducts.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Apply pagination
            var paginatedProducts = allProducts
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = paginatedProducts.Select(MapToItemViewModel).ToList();

            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;

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
            var currentUser = await _userManager.GetUserAsync(User);
            long? deletedByUserId = currentUser?.Id;
            
            var result = await _productService.DeleteAsync(id, deletedByUserId);
            if (!result.Success)
            {
                return Json(new { success = false, message = result.Message });
            }
            return Json(new { success = true, message = "Product deleted successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> Filter([FromBody] Dictionary<string, string> filters)
        {
            try
            {
                var result = await _productService.GetFilteredAsync(filters ?? new Dictionary<string, string>());
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var vm = result.Data?.Select(MapToItemViewModel).ToList() ?? new List<ProductItemViewModel>();
                return Json(new { success = true, data = vm });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error filtering data: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Print([FromBody] JsonElement requestData)
        {
            try
            {
                // Extract filters and hiddenColumns from request
                Dictionary<string, string> filters = new Dictionary<string, string>();
                List<string> hiddenColumns = new List<string>();

                if (requestData.ValueKind == JsonValueKind.Object)
                {
                    // Extract filters
                    if (requestData.TryGetProperty("filters", out var filtersElement))
                    {
                        filters = JsonSerializer.Deserialize<Dictionary<string, string>>(filtersElement.GetRawText()) ?? new Dictionary<string, string>();
                    }
                    else
                    {
                        // Backward compatibility: if filters are sent directly (old format)
                        var directFilters = JsonSerializer.Deserialize<Dictionary<string, string>>(requestData.GetRawText());
                        if (directFilters != null && !directFilters.ContainsKey("hiddenColumns"))
                        {
                            filters = directFilters;
                        }
                    }

                    // Extract hiddenColumns
                    if (requestData.TryGetProperty("hiddenColumns", out var hiddenColumnsElement))
                    {
                        hiddenColumns = JsonSerializer.Deserialize<List<string>>(hiddenColumnsElement.GetRawText()) ?? new List<string>();
                    }
                }

                var result = await _productService.GetFilteredAsync(filters);
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var products = result.Data ?? new List<Product>();
                var vm = products.Select(MapToItemViewModel).ToList();

                var allColumns = new List<(string Header, string PropertyName)>
                {
                    ("Product Name", "ProductName"),
                    ("Product Category", "ProductCategory"),
                    ("Product Code", "ProductCode"),
                    ("Requires Certificate", "RequiresCertificate"),
                    ("Unit Of Measure", "UnitOfMeasure"),
                    ("Standard Price", "StandardPrice"),
                    ("Certificate Type", "CertificateType"),
                    ("Created By User", "CreatedByUser"),
                    ("Currency", "Currency"),
                    ("Description", "Description"),
                    ("Is Active", "IsActive")
                };

                var visibleColumns = allColumns.Where(col => !hiddenColumns.Contains(col.PropertyName)).ToList();
                var columnHeaders = visibleColumns.Select(col => col.Header).ToList();
                var columnIndices = visibleColumns.Select(col => allColumns.IndexOf(allColumns.First(c => c.PropertyName == col.PropertyName))).ToList();

                var stream = _exportToExcel.GenerateExcel(
                    moduleName: "Products",
                    worksheetName: "Products",
                    columnHeaders: columnHeaders,
                    data: vm,
                    rowMapper: item =>
                    {
                        var allValues = new List<object>
                        {
                            item.ProductName ?? "",
                            item.ProductCategoryName ?? "N/A",
                            item.ProductCode ?? "",
                            item.RequiresCertificate ? "Yes" : "No",
                            item.UnitOfMeasure ?? "",
                            item.StandardPrice?.ToString("N2") ?? "",
                            item.CertificateTypeName ?? "N/A",
                            item.CreatedByUserName ?? "N/A",
                            item.CurrencyCode ?? "N/A",
                            item.Description ?? "",
                            item.IsActive ? "Active" : "Inactive"
                        };
                        return columnIndices.Select(idx => allValues[idx]).ToList();
                    }
                );

                var fileName = $"Products_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error generating Excel: {ex.Message}" });
            }
        }

        private static ProductItemViewModel MapToItemViewModel(Product entity)
        {
            return new ProductItemViewModel
            {
                ProductId = entity.ProductId,
                ProductName = entity.ProductName,
                ProductCategoryName = entity.ProductCategory?.CategoryName,
                ProductCode = entity.ProductCode,
                RequiresCertificate = entity.RequiresCertificate,
                UnitOfMeasure = entity.UnitOfMeasure,
                StandardPrice = entity.StandardPrice,
                CertificateTypeName = entity.CertificateType?.CertificateTypeName,
                CreatedByUserName = entity.CreatedByUser?.UserName,
                CreatedDate = entity.CreatedDate,
                CurrencyCode = entity.Currency?.CurrencyCode,
                Description = entity.Description,
                IsActive = entity.IsActive
            };
        }

        private static ProductEditViewModel MapToEditViewModel(Product entity, bool isDetailsView = false)
        {
            return new ProductEditViewModel
            {
                ProductId = entity.ProductId,
                ProductName = entity.ProductName,
                ProductCategoryId = entity.ProductCategoryId,
                ProductCode = entity.ProductCode,
                RequiresCertificate = entity.RequiresCertificate,
                UnitOfMeasure = entity.UnitOfMeasure,
                StandardPrice = entity.StandardPrice,
                CertificateTypeId = entity.CertificateTypeId,
                CreatedByUserId = entity.CreatedByUserId,
                CurrencyId = entity.CurrencyId,
                Description = entity.Description,
                CreatedDate = entity.CreatedDate,
                IsDetailsView = isDetailsView,
                IsActive = entity.IsActive
            };
        }

        private static Product MapToEntity(ProductEditViewModel vm)
        {
            return new Product
            {
                ProductId = vm.ProductId,
                ProductName = vm.ProductName,
                ProductCategoryId = vm.ProductCategoryId ?? 0,
                ProductCode = vm.ProductCode,
                RequiresCertificate = vm.RequiresCertificate,
                UnitOfMeasure = vm.UnitOfMeasure,
                StandardPrice = vm.StandardPrice,
                CertificateTypeId = vm.CertificateTypeId,
                CreatedByUserId = vm.CreatedByUserId ?? 0,
                CurrencyId = vm.CurrencyId ?? 0,
                Description = vm.Description,
                CreatedDate = vm.CreatedDate ?? DateTimeOffset.UtcNow,
                IsActive = vm.IsActive
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
