using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.ProductCategory;
using TpaSodManagement.Utilities;
using TpaSodManagement.Database.Entities;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class ProductCategoryController : Controller
    {
        private readonly IProductCategoryService _productCategoryService;
        private readonly IExportToExcel _exportToExcel;

        public ProductCategoryController(IProductCategoryService productCategoryService, IExportToExcel exportToExcel)
        {
            _productCategoryService = productCategoryService;
            _exportToExcel = exportToExcel;
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

                var result = await _productCategoryService.GetFilteredAsync(filters);
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var categories = result.Data ?? new List<ProductCategory>();
                var vm = categories.Select(MapToItemViewModel).ToList();

                var allColumns = new List<(string Header, string PropertyName)>
                {
                    ("Category Code", "CategoryCode"),
                    ("Category Name", "CategoryName"),
                    ("Description", "Description"),
                    ("Is Active", "IsActive")
                };

                var visibleColumns = allColumns.Where(col => !hiddenColumns.Contains(col.PropertyName)).ToList();
                var columnHeaders = visibleColumns.Select(col => col.Header).ToList();
                var columnIndices = visibleColumns.Select(col => allColumns.IndexOf(allColumns.First(c => c.PropertyName == col.PropertyName))).ToList();

                var stream = _exportToExcel.GenerateExcel(
                    moduleName: "ProductCategories",
                    worksheetName: "ProductCategories",
                    columnHeaders: columnHeaders,
                    data: vm,
                    rowMapper: item =>
                    {
                        var allValues = new List<object>
                        {
                            item.CategoryCode ?? "",
                            item.CategoryName ?? "",
                            item.Description ?? "",
                            item.IsActive ? "Active" : "Inactive"
                        };
                        return columnIndices.Select(idx => allValues[idx]).ToList();
                    }
                );

                var fileName = $"ProductCategories_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error generating Excel: {ex.Message}" });
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
