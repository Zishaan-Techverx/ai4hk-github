using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TpaSodManagement.Database;
using TpaSodManagement.Database.Seeders;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.Customer;
using TpaSodManagement.ViewModels.Address;
using TpaSodManagement.Utilities;
using TpaSodManagement.Database.Entities;
using Microsoft.AspNetCore.Identity;
using TpaSodManagement.Areas.Identity.Data;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly IExportToExcel _exportToExcel;
        private readonly IExportToPdf _exportToPdf;
        private readonly UserManager<TpaSodManagementUser> _userManager;
        private readonly ApplicationDbContext _context;

        public CustomerController(ICustomerService customerService, IExportToExcel exportToExcel, IExportToPdf exportToPdf, UserManager<TpaSodManagementUser> userManager, ApplicationDbContext context)
        {
            _customerService = customerService;
            _exportToExcel = exportToExcel;
            _exportToPdf = exportToPdf;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            // Set filter columns for the partial view
            ViewBag.FilterColumns = new Dictionary<string, string>
            {
                { "Organization", "Organization" },
                { "CustomerType", "Customer Type" },
                { "Person", "Person" },
                { "Address", "Address" },
                { "CustomerCode", "Customer Code" },
                { "CreditLimit", "Credit Limit" },
                { "PaymentTermsDays", "Payment Terms Days" },
                { "TaxExempt", "Tax Exempt" },
                { "Notes", "Notes" },
                { "IsActive", "Is Active" }
            };
            ViewBag.ModuleName = "Customers";
            ViewBag.BooleanColumns = new HashSet<string> { "TaxExempt" };
            ViewBag.TriStateColumns = new HashSet<string> { "IsActive" };
            ViewBag.DateColumns = new HashSet<string>();

            var result = await _customerService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                ViewBag.PageNumber = 1;
                ViewBag.TotalPages = 1;
                ViewBag.TotalCount = 0;
                ViewBag.PageSize = pageSize;
                return View(new List<CustomerItemViewModel>());
            }

            var allCustomers = result.Data ?? new List<Customer>();
            var totalCount = allCustomers.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Apply pagination
            var paginatedCustomers = allCustomers
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = paginatedCustomers.Select(MapToItemViewModel).ToList();

            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;

            return View(vm);
        }

        // GET: Customer/Details/{id}
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var result = await _customerService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var vm = MapToEditViewModel(result.Data, isDetailsView: true);
            ViewBag.AddressFormatted = FormatAddress(result.Data.Address);
            await PopulateDropdowns(vm);
            ViewBag.IsDetailsView = true;
            ViewBag.Title = "Customer Details";
            return View("Edit", vm);
        }

        public async Task<IActionResult> Create(string? returnUrl = null)
        {
            var vm = new CustomerEditViewModel { IsActive = true };
            await PopulateDropdowns(vm);
            ViewBag.ReturnUrl = returnUrl;

            // Non-SuperAdmin: Customer Type and Organization are read-only, pre-filled from current user's org
            if (!User.IsInRole("SuperAdmin"))
            {
                var currentUser = await _userManager.GetUserAsync(HttpContext.User);
                if (currentUser != null && currentUser.OrganizationId.HasValue)
                {
                    var org = await _context.Organizations
                        .FirstOrDefaultAsync(o => o.OrganizationId == currentUser.OrganizationId.Value);
                    if (org != null)
                    {
                        vm.OrganizationId = org.OrganizationId;
                        var fieldTypeName = await GetOrganizationFieldTypeNameAsync(org.OrganizationId);
                        if (!string.IsNullOrEmpty(fieldTypeName))
                        {
                            var customerType = await _context.CustomerTypes
                                .Where(ct => ct.DeletedDate == null && ct.IsActive)
                                .FirstOrDefaultAsync(ct => MapFieldTypeToCustomerTypeName(fieldTypeName) == ct.CustomerTypeName);
                            if (customerType != null)
                                vm.CustomerTypeId = customerType.CustomerTypeId;
                        }
                    }
                }
            }
            await SetCreateReadOnlyViewBagAsync(vm);

            return View(vm);
        }

        /// <summary>
        /// Maps FieldTypeName to CustomerTypeName (e.g. HGT_Sod -> HGTSod, RTF_HGT_Sod -> RTFHGTSod).
        /// </summary>
        private static string MapFieldTypeToCustomerTypeName(string fieldTypeName)
        {
            var normalized = fieldTypeName?.Replace("_", "").Replace(" ", "") ?? "";
            return normalized;
        }

        private async Task<string> GetOrganizationFieldTypeNameAsync(long organizationId)
        {
            return await _context.Fields
                .Where(f => f.DeletedDate == null && f.Farm != null && f.Farm.OrganizationId == organizationId)
                .Include(f => f.FieldType)
                .OrderBy(f => f.FieldId)
                .Select(f => f.FieldType.FieldTypeName)
                .FirstOrDefaultAsync() ?? string.Empty;
        }

        private async Task SetCreateReadOnlyViewBagAsync(CustomerEditViewModel vm)
        {
            if (!User.IsInRole("SuperAdmin"))
            {
                var currentUser = await _userManager.GetUserAsync(HttpContext.User);
                if (currentUser != null && currentUser.OrganizationId.HasValue)
                {
                    var org = await _context.Organizations
                        .FirstOrDefaultAsync(o => o.OrganizationId == currentUser.OrganizationId.Value);
                    if (org != null)
                    {
                        ViewBag.CurrentOrganizationDisplayName = org.OrganizationName ?? $"Organization #{org.OrganizationId}";
                        var fieldTypeName = await GetOrganizationFieldTypeNameAsync(org.OrganizationId);
                        if (!string.IsNullOrEmpty(fieldTypeName))
                        {
                            var customerType = await _context.CustomerTypes
                                .Where(ct => ct.DeletedDate == null && ct.IsActive)
                                .FirstOrDefaultAsync(ct => MapFieldTypeToCustomerTypeName(fieldTypeName) == ct.CustomerTypeName);
                            ViewBag.CurrentCustomerTypeDisplayName = customerType?.CustomerTypeName ?? fieldTypeName;
                        }
                        else
                        {
                            ViewBag.CurrentCustomerTypeDisplayName = "-- No Field Type --";
                        }
                        ViewBag.IsCustomerTypeOrgReadOnly = true;
                        return;
                    }
                }
            }
            ViewBag.IsCustomerTypeOrgReadOnly = false;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerEditViewModel customerVm, string? returnUrl = null)
        {
            // Non-SuperAdmin: set Customer Type and Organization from current user (read-only in UI)
            if (!User.IsInRole("SuperAdmin"))
            {
                var currentUser = await _userManager.GetUserAsync(HttpContext.User);
                if (currentUser != null && currentUser.OrganizationId.HasValue)
                {
                    customerVm.OrganizationId = currentUser.OrganizationId.Value;
                    var org = await _context.Organizations
                        .FirstOrDefaultAsync(o => o.OrganizationId == currentUser.OrganizationId.Value);
                    if (org != null)
                    {
                        var fieldTypeName = await GetOrganizationFieldTypeNameAsync(org.OrganizationId);
                        var customerType = await _context.CustomerTypes
                            .Where(ct => ct.DeletedDate == null && ct.IsActive)
                            .FirstOrDefaultAsync(ct => MapFieldTypeToCustomerTypeName(fieldTypeName) == ct.CustomerTypeName);
                        if (customerType != null)
                            customerVm.CustomerTypeId = customerType.CustomerTypeId;
                    }
                }
            }

            // Create new person when user chose "Create New" and provided First/Last name
            if (customerVm.CreateNewPerson)
            {
                var firstName = customerVm.NewPersonFirstName?.Trim();
                var lastName = customerVm.NewPersonLastName?.Trim();
                if (string.IsNullOrEmpty(firstName))
                    ModelState.AddModelError("NewPersonFirstName", "First Name is required when creating a new person.");
                if (string.IsNullOrEmpty(lastName))
                    ModelState.AddModelError("NewPersonLastName", "Last Name is required when creating a new person.");
                if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
                {
                    var currentUser = await _userManager.GetUserAsync(HttpContext.User);
                    var newPerson = new Person
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        IsPrimaryContact = false,
                        IsActive = true,
                        CreatedDate = DateTimeOffset.UtcNow,
                        CreatedByUserId = currentUser?.Id,
                        UpdatedDate = DateTimeOffset.UtcNow,
                        UpdatedByUserId = currentUser?.Id
                    };
                    _context.People.Add(newPerson);
                    await _context.SaveChangesAsync();
                    customerVm.PersonId = newPerson.PersonId;
                }
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(customerVm);
                ViewBag.ReturnUrl = returnUrl;
                await SetCreateReadOnlyViewBagAsync(customerVm);
                return View(customerVm);
            }

            var customer = MapToEntity(customerVm);
            var result = await _customerService.CreateAsync(customer);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await PopulateDropdowns(customerVm);
                ViewBag.ReturnUrl = returnUrl;
                await SetCreateReadOnlyViewBagAsync(customerVm);
                return View(customerVm);
            }

            return RedirectToAction(nameof(CreateAddress), new { id = result.Data!.CustomerId, returnUrl });
        }

        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var result = await _customerService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var vm = MapToEditViewModel(result.Data);
            await PopulateDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, CustomerEditViewModel customerVm)
        {
            if (id != customerVm.CustomerId) return NotFound();

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(customerVm);
                return View(customerVm);
            }

            var customer = MapToEntity(customerVm);
            var result = await _customerService.UpdateAsync(customer);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await PopulateDropdowns(customerVm);
                return View(customerVm);
            }

            return RedirectToAction(nameof(EditAddress), new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            long? deletedByUserId = currentUser?.Id;
            
            var result = await _customerService.DeleteAsync(id, deletedByUserId);
            if (!result.Success)
            {
                return Json(new { success = false, message = result.Message });
            }
            return Json(new { success = true, message = "Customer deleted successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> Filter([FromBody] Dictionary<string, string> filters)
        {
            try
            {
                var result = await _customerService.GetFilteredAsync(filters ?? new Dictionary<string, string>());
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var vm = result.Data?.Select(MapToItemViewModel).ToList() ?? new List<CustomerItemViewModel>();
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

                var result = await _customerService.GetFilteredAsync(filters);
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var customers = result.Data ?? new List<Customer>();
                var vm = customers.Select(MapToItemViewModel).ToList();

                var allColumns = new List<(string Header, string PropertyName)>
                {
                    ("Organization", "Organization"),
                    ("Customer Type", "CustomerTypeName"),
                    ("Person", "Person"),
                    ("Address", "Address"),
                    ("Customer Code", "CustomerCode"),
                    ("Credit Limit", "CreditLimit"),
                    ("Payment Terms Days", "PaymentTermsDays"),
                    ("Tax Exempt", "TaxExempt"),
                    ("Notes", "Notes"),
                    ("Is Active", "IsActive")
                };

                var visibleColumns = allColumns.Where(col => !hiddenColumns.Contains(col.PropertyName)).ToList();
                var columnHeaders = visibleColumns.Select(col => col.Header).ToList();
                var columnIndices = visibleColumns.Select(col => allColumns.IndexOf(allColumns.First(c => c.PropertyName == col.PropertyName))).ToList();

                var stream = _exportToExcel.GenerateExcel(
                    moduleName: "Customers",
                    worksheetName: "Customers",
                    columnHeaders: columnHeaders,
                    data: vm,
                    rowMapper: item =>
                    {
                        var allValues = new List<object>
                        {
                            item.OrganizationName ?? "",
                            item.CustomerTypeName ?? "",
                            item.PersonFullName ?? "",
                            item.Address ?? "",
                            item.CustomerCode ?? "",
                            item.CreditLimit?.ToString("N2") ?? "",
                            item.PaymentTermsDays ?? 0,
                            item.TaxExempt ? "Yes" : "No",
                            item.Notes ?? "",
                            item.IsActive ? "Yes" : "No"
                            
                        };
                        return columnIndices.Select(idx => allValues[idx]).ToList();
                    }
                );

                var fileName = $"Customers_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
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
                var result = await _customerService.GetFilteredAsync(filters);
                if (!result.Success)
                    return Json(new { success = false, message = result.Message });
                var customers = result.Data ?? new List<Customer>();
                var vm = customers.Select(MapToItemViewModel).ToList();
                var allColumns = new List<(string Header, string PropertyName)>
                {
                    ("Organization", "Organization"),
                    ("Customer Type", "CustomerTypeName"),
                    ("Person", "Person"),
                    ("Address", "Address"),
                    ("Customer Code", "CustomerCode"),
                    ("Credit Limit", "CreditLimit"),
                    ("Payment Terms Days", "PaymentTermsDays"),
                    ("Tax Exempt", "TaxExempt"),
                    ("Notes", "Notes"),
                    ("Is Active", "IsActive")
                };
                var visibleColumns = allColumns.Where(col => !hiddenColumns.Contains(col.PropertyName)).ToList();
                var columnHeaders = visibleColumns.Select(col => col.Header).ToList();
                var columnIndices = visibleColumns.Select(col => allColumns.IndexOf(allColumns.First(c => c.PropertyName == col.PropertyName))).ToList();
                var stream = _exportToPdf.GeneratePdf("Customers", columnHeaders, vm, item =>
                {
                    var allValues = new List<object>
                    {
                        item.OrganizationName ?? "",
                        item.CustomerTypeName ?? "",
                        item.PersonFullName ?? "",
                        item.Address ?? "",
                        item.CustomerCode ?? "",
                        item.CreditLimit?.ToString("N2") ?? "",
                        item.PaymentTermsDays ?? 0,
                        item.TaxExempt ? "Yes" : "No",
                        item.Notes ?? "",
                        item.IsActive ? "Yes" : "No"
                    };
                    return columnIndices.Select(idx => allValues[idx]).ToList();
                }, headerImageBytes);
                var fileName = $"Customers_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
                return File(stream, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error generating PDF: {ex.Message}" });
            }
        }

        private static CustomerItemViewModel MapToItemViewModel(Customer entity)
        {
            return new CustomerItemViewModel
            {
                CustomerId = entity.CustomerId,
                OrganizationName = entity.Organization?.OrganizationName,
                CustomerTypeName = entity.CustomerTypeName,
                PersonFullName = entity.Person != null
                    ? $"{entity.Person.FirstName} {entity.Person.LastName}".Trim()
                    : null,
                Address = FormatAddress(entity.Address),
                CustomerCode = entity.CustomerCode,
                CreditLimit = entity.CreditLimit,
                PaymentTermsDays = entity.PaymentTermsDays,
                TaxExempt = entity.TaxExempt,
                Notes = entity.Notes,
                IsActive = entity.IsActive,
                CreatedDate = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate
            };
        }

        private static CustomerEditViewModel MapToEditViewModel(Customer entity, bool isDetailsView = false)
        {
            return new CustomerEditViewModel
            {
                CustomerId = entity.CustomerId,
                OrganizationId = entity.OrganizationId,
                CustomerTypeId = entity.CustomerTypeId,
                PersonId = entity.PersonId,
                AddressId = entity.AddressId,
                CustomerCode = entity.CustomerCode,
                CreditLimit = entity.CreditLimit,
                PaymentTermsDays = entity.PaymentTermsDays,
                TaxExempt = entity.TaxExempt,
                Notes = entity.Notes,
                IsActive = entity.IsActive,
                CreatedDate = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate,
                IsDetailsView = isDetailsView
            };
        }

        private static Customer MapToEntity(CustomerEditViewModel vm)
        {
            return new Customer
            {
                CustomerId = vm.CustomerId,
                OrganizationId = vm.OrganizationId,
                CustomerTypeId = vm.CustomerTypeId,
                PersonId = vm.PersonId,
                AddressId = vm.AddressId,
                CustomerCode = vm.CustomerCode,
                CreditLimit = vm.CreditLimit,
                PaymentTermsDays = vm.PaymentTermsDays,
                TaxExempt = vm.TaxExempt,
                Notes = vm.Notes,
                IsActive = vm.IsActive,
                CreatedDate = vm.CreatedDate ?? DateTimeOffset.UtcNow,
                UpdatedDate = vm.UpdatedDate ?? DateTimeOffset.UtcNow
            };
        }

        private static string FormatAddress(Address? a)
        {
            if (a == null) return string.Empty;
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(a.AddressLine1)) parts.Add(a.AddressLine1);
            if (!string.IsNullOrWhiteSpace(a.AddressLine2)) parts.Add(a.AddressLine2);
            var cityState = new List<string>();
            if (!string.IsNullOrWhiteSpace(a.City)) cityState.Add(a.City);
            if (a.StateProvince != null && !string.IsNullOrWhiteSpace(a.StateProvince.StateName)) cityState.Add(a.StateProvince.StateName);
            if (!string.IsNullOrWhiteSpace(a.PostalCode)) cityState.Add(a.PostalCode);
            if (cityState.Count > 0) parts.Add(string.Join(", ", cityState));
            return string.Join(", ", parts);
        }

        private async Task PopulateAddressDropdowns(AddressFormViewModel vm)
        {
            vm.AddressTypes = await AddressTypeSeeder.GetAddressTypeSelectListAsync(_context, vm.AddressTypeId);
            vm.StateProvinces = await _context.StateProvinces
                .Where(sp => sp.DeletedDate == null)
                .OrderBy(sp => sp.StateName)
                .Select(sp => new SelectListItem { Value = sp.StateProvinceId.ToString(), Text = sp.StateName ?? "" })
                .ToListAsync();
        }

        public async Task<IActionResult> CreateAddress(long id, string? returnUrl = null)
        {
            var result = await _customerService.GetByIdAsync(id);
            if (!result.Success || result.Data == null) return NotFound();
            var customerTypeId = await AddressTypeSeeder.GetAddressTypeIdByNameAsync(_context, "Customer");
            var vm = new AddressFormViewModel
            {
                ParentEntityName = "Customer",
                ParentEntityId = id,
                AddressTypeId = customerTypeId ?? 0,
                IsAddressTypeReadOnly = true,
                IsActive = true
            };
            await PopulateAddressDropdowns(vm);
            ViewBag.ReturnUrl = returnUrl;
            return View("AddressForm", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAddress(long id, AddressFormViewModel vm, string? returnUrl = null)
        {
            if (id != vm.ParentEntityId) return NotFound();
            var result = await _customerService.GetByIdAsync(id);
            if (!result.Success || result.Data == null) return NotFound();
            var customer = result.Data;

            if (ModelState.IsValid)
            {
                var customerTypeId = await AddressTypeSeeder.GetAddressTypeIdByNameAsync(_context, "Customer");
                var address = new Address
                {
                    AddressLine1 = vm.AddressLine1,
                    AddressLine2 = vm.AddressLine2,
                    City = vm.City,
                    StateProvinceId = vm.StateProvinceId,
                    PostalCode = vm.PostalCode,
                    AddressTypeId = customerTypeId ?? vm.AddressTypeId,
                    Latitude = vm.Latitude,
                    Longitude = vm.Longitude,
                    IsPrimary = vm.IsPrimary,
                    IsVerified = vm.IsVerified,
                    IsActive = vm.IsActive
                };
                _context.Addresses.Add(address);
                await _context.SaveChangesAsync();
                customer.AddressId = address.AddressId;
                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Customer and address saved successfully.";
                if (!string.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction(nameof(Index));
            }
            vm.ParentEntityName = "Customer";
            vm.ParentEntityId = id;
            vm.IsAddressTypeReadOnly = true;
            vm.AddressTypeId = (await AddressTypeSeeder.GetAddressTypeIdByNameAsync(_context, "Customer")) ?? vm.AddressTypeId;
            await PopulateAddressDropdowns(vm);
            ViewBag.ReturnUrl = returnUrl;
            return View("AddressForm", vm);
        }

        public async Task<IActionResult> EditAddress(long id)
        {
            var result = await _customerService.GetByIdAsync(id);
            if (!result.Success || result.Data == null) return NotFound();
            var customer = result.Data;
            var customerTypeId = await AddressTypeSeeder.GetAddressTypeIdByNameAsync(_context, "Customer");
            var vm = new AddressFormViewModel
            {
                ParentEntityName = "Customer",
                ParentEntityId = id,
                AddressTypeId = customerTypeId ?? 0,
                IsAddressTypeReadOnly = true,
                IsActive = true
            };
            if (customer.AddressId.HasValue && customer.Address != null)
            {
                var a = customer.Address;
                vm.AddressId = a.AddressId;
                vm.AddressLine1 = a.AddressLine1;
                vm.AddressLine2 = a.AddressLine2;
                vm.City = a.City;
                vm.StateProvinceId = a.StateProvinceId;
                vm.PostalCode = a.PostalCode;
                vm.AddressTypeId = customerTypeId ?? a.AddressTypeId;
                vm.Latitude = a.Latitude;
                vm.Longitude = a.Longitude;
                vm.IsPrimary = a.IsPrimary;
                vm.IsVerified = a.IsVerified;
                vm.IsActive = a.IsActive;
            }
            await PopulateAddressDropdowns(vm);
            return View("AddressForm", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAddress(long id, AddressFormViewModel vm)
        {
            if (id != vm.ParentEntityId) return NotFound();
            var result = await _customerService.GetByIdAsync(id);
            if (!result.Success || result.Data == null) return NotFound();
            var customer = result.Data;

            if (ModelState.IsValid)
            {
                Address address;
                if (vm.AddressId.HasValue)
                {
                    address = await _context.Addresses.FindAsync(vm.AddressId.Value);
                    if (address == null) return NotFound();
                    var customerTypeId = await AddressTypeSeeder.GetAddressTypeIdByNameAsync(_context, "Customer");
                    address.AddressLine1 = vm.AddressLine1;
                    address.AddressLine2 = vm.AddressLine2;
                    address.City = vm.City;
                    address.StateProvinceId = vm.StateProvinceId;
                    address.PostalCode = vm.PostalCode;
                    address.AddressTypeId = customerTypeId ?? vm.AddressTypeId;
                    address.Latitude = vm.Latitude;
                    address.Longitude = vm.Longitude;
                    address.IsPrimary = vm.IsPrimary;
                    address.IsVerified = vm.IsVerified;
                    address.IsActive = vm.IsActive;
                    _context.Addresses.Update(address);
                }
                else
                {
                    var customerTypeId = await AddressTypeSeeder.GetAddressTypeIdByNameAsync(_context, "Customer");
                    address = new Address
                    {
                        AddressLine1 = vm.AddressLine1,
                        AddressLine2 = vm.AddressLine2,
                        City = vm.City,
                        StateProvinceId = vm.StateProvinceId,
                        PostalCode = vm.PostalCode,
                        AddressTypeId = customerTypeId ?? vm.AddressTypeId,
                        Latitude = vm.Latitude,
                        Longitude = vm.Longitude,
                        IsPrimary = vm.IsPrimary,
                        IsVerified = vm.IsVerified,
                        IsActive = vm.IsActive
                    };
                    _context.Addresses.Add(address);
                    await _context.SaveChangesAsync();
                }
                await _context.SaveChangesAsync();
                customer.AddressId = address.AddressId;
                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Address updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            vm.ParentEntityName = "Customer";
            vm.ParentEntityId = id;
            vm.IsAddressTypeReadOnly = true;
            vm.AddressTypeId = (await AddressTypeSeeder.GetAddressTypeIdByNameAsync(_context, "Customer")) ?? vm.AddressTypeId;
            await PopulateAddressDropdowns(vm);
            return View("AddressForm", vm);
        }

        private async Task PopulateDropdowns(CustomerEditViewModel vm)
        {
            var viewData = await _customerService.GetCreateViewDataAsync();
            if (viewData.Success && viewData.Data.Organizations != null && viewData.Data.People != null && viewData.Data.CustomerTypes != null)
            {
                vm.Organizations = viewData.Data.Organizations;
                vm.People = viewData.Data.People;
                vm.CustomerTypes = viewData.Data.CustomerTypes;
            }
            else
            {
                vm.Organizations = Enumerable.Empty<SelectListItem>();
                vm.People = Enumerable.Empty<SelectListItem>();
                vm.CustomerTypes = Enumerable.Empty<SelectListItem>();
                TempData["Error"] = viewData.Message;
            }
        }
    }
}
