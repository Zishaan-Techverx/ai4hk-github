using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text;
using TpaSodManagement.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.ViewModels.Sale;
using TpaSodManagement.Utilities;
using TpaSodManagement.Database.Entities;
using TpaSodManagement.Database;
using Microsoft.EntityFrameworkCore;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class SaleController : Controller
    {
        private readonly ISaleService _saleService;
        private readonly UserManager<TpaSodManagementUser> _userManager;
        private readonly ILogger<SaleController> _logger;
        private readonly IExportToExcel _exportToExcel;
        private readonly IExportToPdf _exportToPdf;
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public SaleController(
            ISaleService saleService,
            UserManager<TpaSodManagementUser> userManager,
            ILogger<SaleController> logger,
            IExportToExcel exportToExcel,
            IExportToPdf exportToPdf,
            IWebHostEnvironment env,
            ApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _saleService = saleService;
            _userManager = userManager;
            _logger = logger;
            _exportToExcel = exportToExcel;
            _exportToPdf = exportToPdf;
            _env = env;
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            // Set filter columns for the partial view
            ViewBag.FilterColumns = new Dictionary<string, string>
            {
                { "SaleNumber", "Sale Number" },
                { "InvoiceNumber", "Invoice Number" },
                { "PurchaseOrderNumber", "Purchase Order Number" },
                { "SaleDate", "Sale Date" },
                { "DueDate", "Due Date" },
                { "SubtotalAmount", "Subtotal Amount" },
                { "TaxAmount", "Tax Amount" },
                { "DiscountAmount", "Discount Amount" },
                { "TotalAmount", "Total Amount" },
                { "PaymentTermsDays", "Payment Terms Days" },
                { "Notes", "Notes" },
                { "CurrencyName", "Currency" },
                { "CustomerDisplay", "Customer" },
                { "FieldName", "Field" },
                { "SaleTypeName", "Sale Type" },
                { "StatusName", "Status" },
                { "UpdatedByUserName", "Updated By User" },
                { "UserName", "User" },
                { "IsActive", "Is Active" }
            };
            ViewBag.ModuleName = "Sales";
            ViewBag.BooleanColumns = new HashSet<string>();
            ViewBag.TriStateColumns = new HashSet<string> { "IsActive" };
            ViewBag.DateColumns = new HashSet<string> { "SaleDate", "DueDate" };

            var result = await _saleService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                ViewBag.PageNumber = 1;
                ViewBag.TotalPages = 1;
                ViewBag.TotalCount = 0;
                ViewBag.PageSize = pageSize;
                return View(new List<SaleItemViewModel>());
            }

            var allSales = result.Data ?? new List<Sale>();
            var totalCount = allSales.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Apply pagination
            var paginatedSales = allSales
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = paginatedSales.Select(MapToItemViewModel).ToList();

            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;

            return View(vm);
        }

        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var result = await _saleService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var vm = MapToEditViewModel(result.Data, isDetailsView: true);
            await PopulateDropdowns(vm);
            ViewBag.IsDetailsView = true;
            ViewBag.Title = "Sale Details";
            return View("Edit", vm);
        }

        public async Task<IActionResult> Certificate(long? id)
        {
            if (id == null) return NotFound();

            var result = await _saleService.GetByIdForCertificateAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var vm = MapToCertificateViewModel(result.Data);
            if (!vm.HasSelectedField)
            {
                TempData["ErrorMessage"] = "This sale has no field selected. Please edit the sale and select a field first.";
                return RedirectToAction(nameof(Edit), new { id });
            }
            return View(vm);
        }

        /// <summary>Returns the certificate as PDF (same data and placement as preview) for download.</summary>
        public async Task<IActionResult> DownloadCertificate(long? id)
        {
            if (id == null) return NotFound();

            var result = await _saleService.GetByIdForCertificateAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var vm = MapToCertificateViewModel(result.Data);
            if (!vm.HasSelectedField)
            {
                TempData["ErrorMessage"] = "This sale has no field selected. Please edit the sale and select a field first.";
                return RedirectToAction(nameof(Edit), new { id });
            }
            if (!vm.HasCertificateTemplate)
            {
                TempData["ErrorMessage"] = "No certificate template is configured for the selected field type.";
                return RedirectToAction(nameof(Certificate), new { id });
            }

            var certDir = Path.Combine(_env.WebRootPath, "Public Data", "Certificates");
            var filePath = Path.Combine(certDir, vm.CertificateImageFileName);
            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogWarning("Certificate image not found: {Path}", filePath);
                TempData["ErrorMessage"] = "Certificate template file is missing. Please contact support.";
                return RedirectToAction(nameof(Certificate), new { id });
            }

            var imageBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var pdfBytes = CertificatePdfGenerator.Generate(vm, imageBytes);

            var downloadName = $"Certificate_Sale_{(vm.SaleNumber != null ? vm.SaleNumber.Replace(" ", "_") : id?.ToString() ?? "Certificate")}.pdf";
            return File(pdfBytes, "application/pdf", downloadName);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new SaleEditViewModel();
            await PopulateDropdowns(vm);
            var isSuperAdmin = await _currentUserService.IsCurrentUserSuperAdminAsync();

            // Non-SuperAdmin: User is read-only, pre-filled from current user
            if (!isSuperAdmin)
            {
                var currentUser = await _userManager.GetUserAsync(HttpContext.User);
                if (currentUser != null)
                {
                    vm.UserId = currentUser.Id;

                    // User display name (FirstName LastName (UserName))
                    string? userDisplayName = null;
                    if (currentUser.PersonId.HasValue)
                    {
                        var person = await _context.People.FindAsync(currentUser.PersonId.Value);
                        if (person != null && !string.IsNullOrEmpty(person.FirstName) && !string.IsNullOrEmpty(person.LastName))
                            userDisplayName = $"{person.FirstName} {person.LastName} ({currentUser.UserName})";
                    }
                    ViewBag.CurrentUserDisplayName = userDisplayName ?? currentUser.UserName ?? $"User #{currentUser.Id}";

                    ViewBag.IsUserFarmReadOnly = true;
                }
            }
            else
            {
                ViewBag.IsUserFarmReadOnly = false;
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaleEditViewModel saleVm)
        {
            var isSuperAdmin = await _currentUserService.IsCurrentUserSuperAdminAsync();
            // Non-SuperAdmin: set User from current user (read-only in UI)
            if (!isSuperAdmin)
            {
                var currentUser = await _userManager.GetUserAsync(HttpContext.User);
                if (currentUser != null)
                {
                    saleVm.UserId = currentUser.Id;
                }
            }

            // Set automatic fields
            saleVm.CreatedDate = DateTimeOffset.UtcNow;
            saleVm.UpdatedDate = DateTimeOffset.UtcNow;
            
            // Set UpdatedByUserId to current logged-in user or UserId
            if (saleVm.UpdatedByUserId == null || saleVm.UpdatedByUserId == 0)
            {
                if (saleVm.UserId > 0)
                {
                    saleVm.UpdatedByUserId = saleVm.UserId;
                }
                else
                {
                    // Get current logged in user
                    var currentUser = await _userManager.GetUserAsync(HttpContext.User);
                    if (currentUser != null)
                    {
                        saleVm.UpdatedByUserId = currentUser.Id;
                        saleVm.UserId = currentUser.Id; // Also set UserId if not set
                    }
                }
            }
            
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(saleVm);
                return View(saleVm);
            }

            var entity = MapToEntity(saleVm);
            var result = await _saleService.CreateAsync(entity);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await PopulateDropdowns(saleVm);
                return View(saleVm);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var result = await _saleService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var vm = MapToEditViewModel(result.Data);
            await PopulateDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, SaleEditViewModel saleVm)
        {
            if (id != saleVm.SaleId) return NotFound();

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(saleVm);
                return View(saleVm);
            }

            var entity = MapToEntity(saleVm);
            var result = await _saleService.UpdateAsync(entity);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await PopulateDropdowns(saleVm);
                return View(saleVm);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            long? deletedByUserId = currentUser?.Id;
            
            var result = await _saleService.DeleteAsync(id, deletedByUserId);
            if (!result.Success)
            {
                return Json(new { success = false, message = result.Message });
            }
            return Json(new { success = true, message = "Sale deleted successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> Filter([FromBody] Dictionary<string, string> filters)
        {
            try
            {
                var result = await _saleService.GetFilteredAsync(filters ?? new Dictionary<string, string>());
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var vm = result.Data?.Select(MapToItemViewModel).ToList() ?? new List<SaleItemViewModel>();
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

                var result = await _saleService.GetFilteredAsync(filters);
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var sales = result.Data ?? new List<Sale>();
                var vm = sales.Select(MapToItemViewModel).ToList();

                var allColumns = new List<(string Header, string PropertyName)>
                {
                    ("Sale Number", "SaleNumber"),
                    ("Invoice Number", "InvoiceNumber"),
                    ("Purchase Order Number", "PurchaseOrderNumber"),
                    ("Sale Date", "SaleDate"),
                    ("Due Date", "DueDate"),
                    ("Subtotal Amount", "SubtotalAmount"),
                    ("Tax Amount", "TaxAmount"),
                    ("Discount Amount", "DiscountAmount"),
                    ("Total Amount", "TotalAmount"),
                    ("Payment Terms Days", "PaymentTermsDays"),
                    ("Notes", "Notes"),
                    ("Currency", "Currency"),
                    ("Customer", "Customer"),
                    ("Field", "Field"),
                    ("Sale Type", "SaleType"),
                    ("Status", "Status"),
                    ("Updated By User", "UpdatedByUser"),
                    ("User", "User"),
                    ("Is Active", "IsActive")
                };

                var visibleColumns = allColumns.Where(col => !hiddenColumns.Contains(col.PropertyName)).ToList();
                var columnHeaders = visibleColumns.Select(col => col.Header).ToList();
                var columnIndices = visibleColumns.Select(col => allColumns.IndexOf(allColumns.First(c => c.PropertyName == col.PropertyName))).ToList();

                var stream = _exportToExcel.GenerateExcel(
                    moduleName: "Sales",
                    worksheetName: "Sales",
                    columnHeaders: columnHeaders,
                    data: vm,
                    rowMapper: item =>
                    {
                        var allValues = new List<object>
                        {
                            item.SaleNumber ?? "",
                            item.InvoiceNumber ?? "",
                            item.PurchaseOrderNumber ?? "",
                            item.SaleDate.HasValue ? item.SaleDate.Value.ToString("MM/dd/yyyy") : "",
                            item.DueDate?.ToString("MM/dd/yyyy") ?? "",
                            item.SubtotalAmount?.ToString("N2") ?? "",
                            item.TaxAmount?.ToString("N2") ?? "",
                            item.DiscountAmount?.ToString("N2") ?? "",
                            item.TotalAmount?.ToString("N2") ?? "",
                            item.PaymentTermsDays ?? 0,
                            item.Notes ?? "",
                            item.CurrencyName ?? "N/A",
                            item.CustomerDisplay ?? $"Customer #{item.CustomerId}",
                            item.FieldDisplay ?? "N/A",
                            item.SaleTypeName ?? "N/A",
                            item.StatusName ?? "N/A",
                            item.UpdatedByUserName ?? "N/A",
                            item.UserName ?? "N/A",
                            item.IsActive ? "Yes" : "No"
                        };
                        return columnIndices.Select(idx => allValues[idx]).ToList();
                    }
                );

                var fileName = $"Sales_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
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
                    else
                    {
                        var directFilters = JsonSerializer.Deserialize<Dictionary<string, string>>(requestData.GetRawText());
                        if (directFilters != null && !directFilters.ContainsKey("hiddenColumns"))
                            filters = directFilters;
                    }
                    if (requestData.TryGetProperty("hiddenColumns", out var hiddenColumnsElement))
                        hiddenColumns = JsonSerializer.Deserialize<List<string>>(hiddenColumnsElement.GetRawText()) ?? new List<string>();
                    if (requestData.TryGetProperty("headerImageBase64", out var headerImgEl))
                    {
                        var b64 = headerImgEl.GetString();
                        if (!string.IsNullOrEmpty(b64))
                        {
                            try { headerImageBytes = Convert.FromBase64String(b64); } catch { /* ignore */ }
                        }
                    }
                }
                var result = await _saleService.GetFilteredAsync(filters);
                if (!result.Success)
                    return Json(new { success = false, message = result.Message });
                var sales = result.Data ?? new List<Sale>();
                var vm = sales.Select(MapToItemViewModel).ToList();
                var allColumns = new List<(string Header, string PropertyName)>
                {
                    ("Sale Number", "SaleNumber"),
                    ("Invoice Number", "InvoiceNumber"),
                    ("Purchase Order Number", "PurchaseOrderNumber"),
                    ("Sale Date", "SaleDate"),
                    ("Due Date", "DueDate"),
                    ("Subtotal Amount", "SubtotalAmount"),
                    ("Tax Amount", "TaxAmount"),
                    ("Discount Amount", "DiscountAmount"),
                    ("Total Amount", "TotalAmount"),
                    ("Payment Terms Days", "PaymentTermsDays"),
                    ("Notes", "Notes"),
                    ("Currency", "Currency"),
                    ("Customer", "Customer"),
                    ("Field", "Field"),
                    ("Sale Type", "SaleType"),
                    ("Status", "Status"),
                    ("Updated By User", "UpdatedByUser"),
                    ("User", "User"),
                    ("Is Active", "IsActive")
                };
                var visibleColumns = allColumns.Where(col => !hiddenColumns.Contains(col.PropertyName)).ToList();
                var columnHeaders = visibleColumns.Select(col => col.Header).ToList();
                var columnIndices = visibleColumns.Select(col => allColumns.IndexOf(allColumns.First(c => c.PropertyName == col.PropertyName))).ToList();
                var stream = _exportToPdf.GeneratePdf("Sales", columnHeaders, vm, item =>
                {
                    var allValues = new List<object>
                    {
                        item.SaleNumber ?? "",
                        item.InvoiceNumber ?? "",
                        item.PurchaseOrderNumber ?? "",
                        item.SaleDate.HasValue ? item.SaleDate.Value.ToString("MM/dd/yyyy") : "",
                        item.DueDate?.ToString("MM/dd/yyyy") ?? "",
                        item.SubtotalAmount?.ToString("N2") ?? "",
                        item.TaxAmount?.ToString("N2") ?? "",
                        item.DiscountAmount?.ToString("N2") ?? "",
                        item.TotalAmount?.ToString("N2") ?? "",
                        item.PaymentTermsDays ?? 0,
                        item.Notes ?? "",
                        item.CurrencyName ?? "N/A",
                        item.CustomerDisplay ?? $"Customer #{item.CustomerId}",
                        item.FieldDisplay ?? "N/A",
                        item.SaleTypeName ?? "N/A",
                        item.StatusName ?? "N/A",
                        item.UpdatedByUserName ?? "N/A",
                        item.UserName ?? "N/A",
                        item.IsActive ? "Yes" : "No"
                    };
                    return columnIndices.Select(idx => allValues[idx]).ToList();
                }, headerImageBytes);
                var fileName = $"Sales_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
                return File(stream, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error generating PDF: {ex.Message}" });
            }
        }

        private static SaleItemViewModel MapToItemViewModel(Sale entity)
        {
            return new SaleItemViewModel
            {
                SaleId = entity.SaleId,
                SaleNumber = entity.SaleNumber,
                InvoiceNumber = entity.InvoiceNumber,
                PurchaseOrderNumber = entity.PurchaseOrderNumber,
                SaleDate = entity.SaleDate,
                DueDate = entity.DueDate,
                SubtotalAmount = entity.SubtotalAmount,
                TaxAmount = entity.TaxAmount,
                DiscountAmount = entity.DiscountAmount,
                TotalAmount = entity.TotalAmount,
                PaymentTermsDays = entity.PaymentTermsDays,
                Notes = entity.Notes,
                CreatedDate = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate,
                CurrencyName = entity.Currency?.CurrencyName,
                CustomerId = entity.CustomerId,
                CustomerDisplay = entity.Customer?.Person != null
                    ? $"{entity.Customer.Person.FirstName} {entity.Customer.Person.LastName}".Trim()
                    : !string.IsNullOrEmpty(entity.Customer?.CustomerCode) ? entity.Customer.CustomerCode : null,
                FieldId = entity.FieldId,
                FieldDisplay = entity.Field?.FieldName,
                FarmDisplay = entity.Field?.Farm?.FarmName,
                SaleTypeName = entity.SaleType?.SaleTypeName,
                StatusName = entity.Status?.StatusName,
                UpdatedByUserName = entity.UpdatedByUser?.UserName,
                UserName = entity.User?.UserName,
                IsActive = entity.IsActive
            };
        }

        private static SaleEditViewModel MapToEditViewModel(Sale entity, bool isDetailsView = false)
        {
            return new SaleEditViewModel
            {
                SaleId = entity.SaleId,
                InvoiceNumber = entity.InvoiceNumber,
                SaleDate = entity.SaleDate,
                Notes = entity.Notes,
                UserId = entity.UserId,
                CustomerId = entity.CustomerId,
                FieldId = entity.FieldId,
                SaleTypeId = entity.SaleTypeId,
                UpdatedByUserId = entity.UpdatedByUserId,
                CreatedDate = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate,
                IsActive = entity.IsActive,
                IsDetailsView = isDetailsView
            };
        }

        private static SaleCertificateViewModel MapToCertificateViewModel(Sale sale)
        {
            var farm = sale.Field?.Farm;
            var customer = sale.Customer;
            var fieldTypeName = sale.Field?.FieldType?.FieldTypeName ?? string.Empty;
            var normalizedFieldTypeIdentifier = NormalizeCertificateIdentifier(fieldTypeName);

            // Licensed grower: farm name only
            var licensedGrower = !string.IsNullOrWhiteSpace(farm?.FarmName) ? farm.FarmName.Trim() : "—";

            // Farm address: full address separately
            var farmAddressParts = new List<string>();
            if (farm?.Address != null)
            {
                var a = farm.Address;
                var line1 = a.AddressLine1?.Trim();
                if (!string.IsNullOrEmpty(line1)) farmAddressParts.Add(line1);
                if (!string.IsNullOrWhiteSpace(a.AddressLine2)) farmAddressParts.Add(a.AddressLine2.Trim());
                var cityStateZip = new List<string>();
                if (!string.IsNullOrWhiteSpace(a.City)) cityStateZip.Add(a.City.Trim());
                if (a.StateProvince?.StateName != null) cityStateZip.Add(a.StateProvince.StateName.Trim());
                if (!string.IsNullOrWhiteSpace(a.PostalCode)) cityStateZip.Add(a.PostalCode.Trim());
                if (cityStateZip.Count > 0) farmAddressParts.Add(string.Join(", ", cityStateZip));
            }
            var farmAddress = farmAddressParts.Count > 0 ? string.Join(", ", farmAddressParts) : "—";

            // Customer: FirstName MiddleName LastName or Organization name
            string customerName = "—";
            if (customer?.Person != null)
            {
                var p = customer.Person;
                customerName = string.Join(" ", new[] { p.FirstName?.Trim(), p.MiddleName?.Trim(), p.LastName?.Trim() }.Where(s => !string.IsNullOrEmpty(s))).Trim();
            }
            else if (customer?.Organization != null && !string.IsNullOrWhiteSpace(customer.Organization.OrganizationName))
            {
                customerName = customer.Organization.OrganizationName.Trim();
            }

            // Customer address: from Customer.Address (not Person/Organization)
            var customerAddressParts = new List<string>();
            if (customer?.Address != null)
            {
                var a = customer.Address;
                var line1 = a.AddressLine1?.Trim();
                if (!string.IsNullOrEmpty(line1)) customerAddressParts.Add(line1);
                if (!string.IsNullOrWhiteSpace(a.AddressLine2)) customerAddressParts.Add(a.AddressLine2.Trim());
                var cityStateZip = new List<string>();
                if (!string.IsNullOrWhiteSpace(a.City)) cityStateZip.Add(a.City.Trim());
                if (a.StateProvince?.StateName != null) cityStateZip.Add(a.StateProvince.StateName.Trim());
                if (!string.IsNullOrWhiteSpace(a.PostalCode)) cityStateZip.Add(a.PostalCode.Trim());
                if (cityStateZip.Count > 0) customerAddressParts.Add(string.Join(", ", cityStateZip));
            }
            var customerAddress = customerAddressParts.Count > 0 ? string.Join(", ", customerAddressParts) : "—";

            // Certificate image: choose by organization (name preferred) or type (HGT, RTF, RTFHGT)
            const string certRtf = "RTF Sod Certificate.jpg";
            const string certHgt = "HGT Sod Certificate.jpg";
            const string certRtfHgt = "RTF+HGT Sod Certificate.jpg";

            string? certFile = normalizedFieldTypeIdentifier.Contains("RTFHGT")
                ? certRtfHgt
                : normalizedFieldTypeIdentifier.Contains("HGT")
                    ? certHgt
                    : normalizedFieldTypeIdentifier.Contains("RTF")
                        ? certRtf
                        : null;
            var hasCertificateTemplate = !string.IsNullOrEmpty(certFile);
            var certPath = hasCertificateTemplate ? $"Public Data/Certificates/{certFile}" : string.Empty;

            return new SaleCertificateViewModel
            {
                SaleId = sale.SaleId,
                SaleNumber = sale.SaleNumber ?? "",
                LicensedGrower = licensedGrower,
                FarmAddress = farmAddress,
                DateCertificateIssued = sale.SaleDate.ToString("MMMM d, yyyy"),
                AreaSold = sale.TotalAmount?.ToString("N2") ?? "0.00",
                InvoiceNumbers = sale.InvoiceNumber ?? "—",
                Customer = customerName,
                CustomerAddress = customerAddress,
                CertificateImagePath = certPath,
                CertificateImageFileName = certFile ?? string.Empty,
                HasCertificateTemplate = hasCertificateTemplate,
                HasSelectedField = sale.FieldId > 0
            };
        }

        private static string NormalizeCertificateIdentifier(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var sb = new StringBuilder(value.Length);
            foreach (var ch in value)
            {
                if (char.IsLetterOrDigit(ch))
                    sb.Append(char.ToUpperInvariant(ch));
            }

            return sb.ToString();
        }

        private static Sale MapToEntity(SaleEditViewModel vm)
        {
            var fallbackDate = DateOnly.FromDateTime(DateTime.UtcNow);
            return new Sale
            {
                SaleId = vm.SaleId,
                SaleNumber = null,
                InvoiceNumber = vm.InvoiceNumber,
                PurchaseOrderNumber = null,
                SaleDate = vm.SaleDate ?? fallbackDate,
                DueDate = null,
                SubtotalAmount = null,
                TaxAmount = null,
                DiscountAmount = null,
                TotalAmount = null,
                PaymentTermsDays = null,
                Notes = vm.Notes,
                UserId = vm.UserId ?? 0,
                CustomerId = vm.CustomerId ?? 0,
                FieldId = vm.FieldId ?? 0,
                SaleTypeId = vm.SaleTypeId ?? 0,
                StatusId = null,
                CurrencyId = null,
                UpdatedByUserId = vm.UpdatedByUserId ?? 0,
                CreatedDate = vm.CreatedDate ?? DateTimeOffset.UtcNow,
                UpdatedDate = vm.UpdatedDate ?? DateTimeOffset.UtcNow,
                IsActive = vm.IsActive
            };
        }

        private async Task PopulateDropdowns(SaleEditViewModel vm)
        {
            var dropdowns = await _saleService.GetDropdownDataAsync();
            if (dropdowns.Success && dropdowns.Data != null)
            {
                vm.Users = dropdowns.Data.ContainsKey("UserId") ? dropdowns.Data["UserId"] : Enumerable.Empty<SelectListItem>();
                vm.Customers = dropdowns.Data.ContainsKey("CustomerId") ? dropdowns.Data["CustomerId"] : Enumerable.Empty<SelectListItem>();
                vm.Fields = dropdowns.Data.ContainsKey("FieldId") ? dropdowns.Data["FieldId"] : Enumerable.Empty<SelectListItem>();
                vm.SaleTypes = dropdowns.Data.ContainsKey("SaleTypeId") ? dropdowns.Data["SaleTypeId"] : Enumerable.Empty<SelectListItem>();
            }
            else
            {
                vm.Users = vm.Users ?? Enumerable.Empty<SelectListItem>();
                vm.Customers = vm.Customers ?? Enumerable.Empty<SelectListItem>();
                vm.Fields = vm.Fields ?? Enumerable.Empty<SelectListItem>();
                vm.SaleTypes = vm.SaleTypes ?? Enumerable.Empty<SelectListItem>();
                TempData["Error"] = dropdowns.Message;
            }
        }
    }
}
