using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.ProductCategory;
using OfficeOpenXml;

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

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            // Set filter columns for the partial view
            ViewBag.FilterColumns = new Dictionary<string, string>
            {
                { "CategoryCode", "Category Code" },
                { "CategoryName", "Category Name" },
                { "Description", "Description" },
                { "IsActive", "Is Active" }
            };
            ViewBag.ModuleName = "Product Categories";
            ViewBag.BooleanColumns = new HashSet<string> { "IsActive" };

            var result = await _productCategoryService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                ViewBag.PageNumber = 1;
                ViewBag.TotalPages = 1;
                ViewBag.TotalCount = 0;
                ViewBag.PageSize = pageSize;
                return View(new List<ProductCategoryItemViewModel>());
            }

            var allCategories = result.Data ?? new List<ProductCategory>();
            var totalCount = allCategories.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Apply pagination
            var paginatedCategories = allCategories
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = paginatedCategories.Select(MapToItemViewModel).ToList();

            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;

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

        [HttpPost]
        public async Task<IActionResult> Filter([FromBody] Dictionary<string, string> filters)
        {
            try
            {
                var result = await _productCategoryService.GetFilteredAsync(filters ?? new Dictionary<string, string>());
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var vm = result.Data?.Select(MapToItemViewModel).ToList() ?? new List<ProductCategoryItemViewModel>();
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
                var result = await _productCategoryService.GetFilteredAsync(filters ?? new Dictionary<string, string>());
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var categories = result.Data ?? new List<ProductCategory>();
                var vm = categories.Select(MapToItemViewModel).ToList();

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("ProductCategories");

                    // Headers
                    worksheet.Cells[1, 1].Value = "Category Code";
                    worksheet.Cells[1, 2].Value = "Category Name";
                    worksheet.Cells[1, 3].Value = "Description";
                    worksheet.Cells[1, 4].Value = "Is Active";

                    // Style headers
                    using (var range = worksheet.Cells[1, 1, 1, 4])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    // Data
                    for (int i = 0; i < vm.Count; i++)
                    {
                        var row = i + 2;
                        worksheet.Cells[row, 1].Value = vm[i].CategoryCode;
                        worksheet.Cells[row, 2].Value = vm[i].CategoryName;
                        worksheet.Cells[row, 3].Value = vm[i].Description;
                        worksheet.Cells[row, 4].Value = vm[i].IsActive ? "Active" : "Inactive";
                    }

                    worksheet.Cells.AutoFitColumns();

                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;

                    var fileName = $"ProductCategories_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error generating Excel file: {ex.Message}" });
            }
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
