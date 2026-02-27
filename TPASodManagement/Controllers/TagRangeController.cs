using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Linq;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.TagRange;
using TpaSodManagement.Utilities;
using TpaSodManagement.Database.Entities;
using Microsoft.AspNetCore.Identity;
using TpaSodManagement.Areas.Identity.Data;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class TagRangeController : Controller
    {
        private readonly ITagRangeService _tagRangeService;
        private readonly IExportToExcel _exportToExcel;
        private readonly IExportToPdf _exportToPdf;
        private readonly UserManager<TpaSodManagementUser> _userManager;

        public TagRangeController(ITagRangeService tagRangeService, IExportToExcel exportToExcel, IExportToPdf exportToPdf, UserManager<TpaSodManagementUser> userManager)
        {
            _tagRangeService = tagRangeService;
            _exportToExcel = exportToExcel;
            _exportToPdf = exportToPdf;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            ViewBag.FilterColumns = new Dictionary<string, string>
            {
                { "TagRangeCode", "Tag Range Code" },
                { "TagStartNumber", "Tag Start Number" },
                { "TagEndNumber", "Tag End Number" },
                { "TagPrefix", "Tag Prefix" },
                { "TagSuffix", "Tag Suffix" },
                { "TotalTags", "Total Tags" },
                { "IsActive", "Is Active" }
            };
            ViewBag.ModuleName = "Tag Ranges";
            ViewBag.BooleanColumns = new HashSet<string>();
            ViewBag.TriStateColumns = new HashSet<string> { "IsActive" };

            var result = await _tagRangeService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                ViewBag.PageNumber = 1;
                ViewBag.TotalPages = 1;
                ViewBag.TotalCount = 0;
                ViewBag.PageSize = pageSize;
                return View(new List<TagRangeItemViewModel>());
            }

            var allTagRanges = result.Data ?? new List<TagRange>();
            var totalCount = allTagRanges.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var paginated = allTagRanges
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = paginated.Select(MapToItemViewModel).ToList();

            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Filter([FromBody] Dictionary<string, string> filters)
        {
            try
            {
                var result = await _tagRangeService.GetFilteredAsync(filters ?? new Dictionary<string, string>());
                if (!result.Success)
                    return Json(new { success = false, message = result.Message });
                var vm = result.Data?.Select(MapToItemViewModel).ToList() ?? new List<TagRangeItemViewModel>();
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
                Dictionary<string, string> filters = new Dictionary<string, string>();
                List<string> hiddenColumns = new List<string>();
                if (requestData.ValueKind == JsonValueKind.Object)
                {
                    if (requestData.TryGetProperty("filters", out var filtersElement))
                        filters = JsonSerializer.Deserialize<Dictionary<string, string>>(filtersElement.GetRawText()) ?? new Dictionary<string, string>();
                    if (requestData.TryGetProperty("hiddenColumns", out var hiddenColumnsElement))
                        hiddenColumns = JsonSerializer.Deserialize<List<string>>(hiddenColumnsElement.GetRawText()) ?? new List<string>();
                }
                var result = await _tagRangeService.GetFilteredAsync(filters);
                if (!result.Success)
                    return Json(new { success = false, message = result.Message });
                var tagRanges = result.Data ?? new List<TagRange>();
                var vm = tagRanges.Select(MapToItemViewModel).ToList();
                var allColumns = new List<(string Header, string PropertyName)>
                {
                    ("Tag Range Code", "TagRangeCode"),
                    ("Tag Start Number", "TagStartNumber"),
                    ("Tag End Number", "TagEndNumber"),
                    ("Tag Prefix", "TagPrefix"),
                    ("Tag Suffix", "TagSuffix"),
                    ("Total Tags", "TotalTags"),
                    ("Is Active", "IsActive")
                };
                var visibleColumns = allColumns.Where(col => !hiddenColumns.Contains(col.PropertyName)).ToList();
                var columnHeaders = visibleColumns.Select(col => col.Header).ToList();
                var columnIndices = visibleColumns.Select(col => allColumns.IndexOf(allColumns.First(c => c.PropertyName == col.PropertyName))).ToList();
                var stream = _exportToExcel.GenerateExcel(
                    moduleName: "TagRanges",
                    worksheetName: "Tag Ranges",
                    columnHeaders: columnHeaders,
                    data: vm,
                    rowMapper: item =>
                    {
                        var allValues = new List<object>
                        {
                            item.TagRangeCode ?? "",
                            item.TagStartNumber.ToString(),
                            item.TagEndNumber.ToString(),
                            item.TagPrefix ?? "",
                            item.TagSuffix ?? "",
                            item.TotalTags.ToString(),
                            item.IsActive ? "Yes" : "No"
                        };
                        return columnIndices.Select(idx => allValues[idx]).ToList();
                    }
                );
                var fileName = $"TagRanges_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error generating Excel: {ex.Message}" });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ExportPdf([FromBody] JsonElement requestData)
        {
            try
            {
                Dictionary<string, string> filters = new Dictionary<string, string>();
                List<string> hiddenColumns = new List<string>();
                byte[]? headerImageBytes = null;
                if (requestData.ValueKind == JsonValueKind.Object)
                {
                    if (requestData.TryGetProperty("filters", out var filtersElement))
                        filters = JsonSerializer.Deserialize<Dictionary<string, string>>(filtersElement.GetRawText()) ?? new Dictionary<string, string>();
                    if (requestData.TryGetProperty("hiddenColumns", out var hiddenColumnsElement))
                        hiddenColumns = JsonSerializer.Deserialize<List<string>>(hiddenColumnsElement.GetRawText()) ?? new List<string>();
                    if (requestData.TryGetProperty("headerImageBase64", out var headerImgEl))
                    {
                        var b64 = headerImgEl.GetString();
                        if (!string.IsNullOrEmpty(b64))
                        {
                            try { headerImageBytes = Convert.FromBase64String(b64); } catch { }
                        }
                    }
                }
                var result = await _tagRangeService.GetFilteredAsync(filters);
                if (!result.Success)
                    return Json(new { success = false, message = result.Message });
                var tagRanges = result.Data ?? new List<TagRange>();
                var vm = tagRanges.Select(MapToItemViewModel).ToList();
                var allColumns = new List<(string Header, string PropertyName)>
                {
                    ("Tag Range Code", "TagRangeCode"),
                    ("Tag Start Number", "TagStartNumber"),
                    ("Tag End Number", "TagEndNumber"),
                    ("Tag Prefix", "TagPrefix"),
                    ("Tag Suffix", "TagSuffix"),
                    ("Total Tags", "TotalTags"),
                    ("Is Active", "IsActive")
                };
                var visibleColumns = allColumns.Where(col => !hiddenColumns.Contains(col.PropertyName)).ToList();
                var columnHeaders = visibleColumns.Select(col => col.Header).ToList();
                var columnIndices = visibleColumns.Select(col => allColumns.IndexOf(allColumns.First(c => c.PropertyName == col.PropertyName))).ToList();
                var stream = _exportToPdf.GeneratePdf("Tag Ranges", columnHeaders, vm, item =>
                {
                    var allValues = new List<object>
                    {
                        item.TagRangeCode ?? "",
                        item.TagStartNumber.ToString(),
                        item.TagEndNumber.ToString(),
                        item.TagPrefix ?? "",
                        item.TagSuffix ?? "",
                        item.TotalTags.ToString(),
                        item.IsActive ? "Yes" : "No"
                    };
                    return columnIndices.Select(idx => allValues[idx]).ToList();
                }, headerImageBytes);
                var fileName = $"TagRanges_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
                return File(stream, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error generating PDF: {ex.Message}" });
            }
        }

        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
                return NotFound();
            var result = await _tagRangeService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null)
                return NotFound();
            var vm = MapToEditViewModel(result.Data, isDetailsView: true);
            ViewBag.IsDetailsView = true;
            ViewBag.Title = "Tag Range Details";
            return View("Edit", vm);
        }

        public IActionResult Create()
        {
            return View(new TagRangeEditViewModel { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TagRangeEditViewModel tagRangeVm)
        {
            if (ModelState.IsValid)
            {
                var tagRange = MapToEntity(tagRangeVm);
                var result = await _tagRangeService.CreateAsync(tagRange);
                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message;
                    return View(tagRangeVm);
                }
                TempData["SuccessMessage"] = "Tag range created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(tagRangeVm);
        }

        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
                return NotFound();
            var result = await _tagRangeService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null)
                return NotFound();
            var vm = MapToEditViewModel(result.Data);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, TagRangeEditViewModel tagRangeVm)
        {
            if (id != tagRangeVm.TagRangeId)
                return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    var tagRange = MapToEntity(tagRangeVm);
                    var result = await _tagRangeService.UpdateAsync(tagRange);
                    if (!result.Success)
                    {
                        TempData["ErrorMessage"] = result.Message;
                        return View(tagRangeVm);
                    }
                    TempData["SuccessMessage"] = "Tag range updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    var exists = await _tagRangeService.ExisTpasync(tagRangeVm.TagRangeId);
                    if (!exists.Data)
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tagRangeVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            long? deletedByUserId = currentUser?.Id;
            var result = await _tagRangeService.DeleteAsync(id, deletedByUserId);
            if (!result.Success)
                return Json(new { success = false, message = result.Message });
            return Json(new { success = true, message = "Tag range deleted successfully." });
        }

        private static TagRangeItemViewModel MapToItemViewModel(TagRange entity)
        {
            return new TagRangeItemViewModel
            {
                TagRangeId = entity.TagRangeId,
                TagRangeCode = entity.TagRangeCode ?? "",
                TagStartNumber = entity.TagStartNumber,
                TagEndNumber = entity.TagEndNumber,
                TagPrefix = entity.TagPrefix,
                TagSuffix = entity.TagSuffix,
                TotalTags = entity.TotalTags,
                IsActive = entity.IsActive
            };
        }

        private static TagRangeEditViewModel MapToEditViewModel(TagRange entity, bool isDetailsView = false)
        {
            return new TagRangeEditViewModel
            {
                TagRangeId = entity.TagRangeId,
                TagRangeCode = entity.TagRangeCode ?? "",
                TagStartNumber = entity.TagStartNumber,
                TagEndNumber = entity.TagEndNumber,
                TagPrefix = entity.TagPrefix,
                TagSuffix = entity.TagSuffix,
                TotalTags = entity.TotalTags,
                IsActive = entity.IsActive,
                CreatedDate = entity.CreatedDate,
                IsDetailsView = isDetailsView
            };
        }

        private static TagRange MapToEntity(TagRangeEditViewModel vm)
        {
            return new TagRange
            {
                TagRangeId = vm.TagRangeId,
                TagRangeCode = vm.TagRangeCode,
                TagStartNumber = vm.TagStartNumber,
                TagEndNumber = vm.TagEndNumber,
                TagPrefix = vm.TagPrefix,
                TagSuffix = vm.TagSuffix,
                TotalTags = (int)Math.Max(0, vm.TagEndNumber - vm.TagStartNumber + 1),
                IsActive = vm.IsActive,
                CreatedDate = vm.CreatedDate ?? DateTimeOffset.UtcNow
            };
        }
    }
}
