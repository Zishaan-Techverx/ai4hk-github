using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.Product;
using OfficeOpenXml;

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

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            // Set filter columns for the partial view
            ViewBag.FilterColumns = new Dictionary<string, string>
            {
                { "ProductCode", "Product Code" },
                { "ProductName", "Product Name" },
                { "UnitOfMeasure", "Unit Of Measure" },
                { "StandardPrice", "Standard Price" },
                { "RequiresCertificate", "Requires Certificate" },
                { "Description", "Description" },
                { "IsActive", "Is Active" },
                { "CreatedDate", "Created Date" },
                { "CertificateTypeName", "Certificate Type" },
                { "CreatedByUserName", "Created By User" },
                { "CurrencyCode", "Currency" },
                { "ProductCategoryName", "Product Category" }
            };
            ViewBag.ModuleName = "Products";
            ViewBag.BooleanColumns = new HashSet<string> { "RequiresCertificate", "IsActive" };
            ViewBag.DateColumns = new HashSet<string> { "CreatedDate" };

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
            var result = await _productService.DeleteAsync(id);
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
        public async Task<IActionResult> Print([FromBody] Dictionary<string, string> filters)
        {
            try
            {
                var result = await _productService.GetFilteredAsync(filters ?? new Dictionary<string, string>());
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var products = result.Data ?? new List<Product>();
                var vm = products.Select(MapToItemViewModel).ToList();

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Products");

                    // Headers
                    worksheet.Cells[1, 1].Value = "Product Code";
                    worksheet.Cells[1, 2].Value = "Product Name";
                    worksheet.Cells[1, 3].Value = "Unit Of Measure";
                    worksheet.Cells[1, 4].Value = "Standard Price";
                    worksheet.Cells[1, 5].Value = "Requires Certificate";
                    worksheet.Cells[1, 6].Value = "Description";
                    worksheet.Cells[1, 7].Value = "Is Active";
                    worksheet.Cells[1, 8].Value = "Certificate Type";
                    worksheet.Cells[1, 9].Value = "Created By User";
                    worksheet.Cells[1, 10].Value = "Currency";
                    worksheet.Cells[1, 11].Value = "Product Category";

                    // Style headers
                    using (var range = worksheet.Cells[1, 1, 1, 11])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    // Data
                    for (int i = 0; i < vm.Count; i++)
                    {
                        var row = i + 2;
                        worksheet.Cells[row, 1].Value = vm[i].ProductCode;
                        worksheet.Cells[row, 2].Value = vm[i].ProductName;
                        worksheet.Cells[row, 3].Value = vm[i].UnitOfMeasure;
                        worksheet.Cells[row, 4].Value = vm[i].StandardPrice;
                        worksheet.Cells[row, 5].Value = vm[i].RequiresCertificate ? "Yes" : "No";
                        worksheet.Cells[row, 6].Value = vm[i].Description;
                        worksheet.Cells[row, 7].Value = vm[i].IsActive ? "Active" : "Inactive";
                        worksheet.Cells[row, 8].Value = vm[i].CertificateTypeName ?? "N/A";
                        worksheet.Cells[row, 9].Value = vm[i].CreatedByUserName ?? "N/A";
                        worksheet.Cells[row, 10].Value = vm[i].CurrencyCode ?? "N/A";
                        worksheet.Cells[row, 11].Value = vm[i].ProductCategoryName ?? "N/A";
                    }

                    worksheet.Cells.AutoFitColumns();

                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;

                    var fileName = $"Products_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error generating Excel file: {ex.Message}" });
            }
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
