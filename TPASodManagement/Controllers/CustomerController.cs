using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.Customer;
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
        private readonly UserManager<TpaSodManagementUser> _userManager;

        public CustomerController(ICustomerService customerService, IExportToExcel exportToExcel, UserManager<TpaSodManagementUser> userManager)
        {
            _customerService = customerService;
            _exportToExcel = exportToExcel;
            _userManager = userManager;
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
                { "Notes", "Notes" }
            };
            ViewBag.ModuleName = "Customers";
            ViewBag.BooleanColumns = new HashSet<string> { "TaxExempt" };
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
            await PopulateDropdowns(vm);
            ViewBag.IsDetailsView = true;
            ViewBag.Title = "Customer Details";
            return View("Edit", vm);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new CustomerEditViewModel { IsActive = true };
            await PopulateDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerEditViewModel customerVm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(customerVm);
                return View(customerVm);
            }

            var customer = MapToEntity(customerVm);
            var result = await _customerService.CreateAsync(customer);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await PopulateDropdowns(customerVm);
                return View(customerVm);
            }

            return RedirectToAction(nameof(Index));
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

            return RedirectToAction(nameof(Index));
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
                    ("Customer Type", "CustomerType"),
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
                            item.CustomerType ?? "",
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

        private static CustomerItemViewModel MapToItemViewModel(Customer entity)
        {
            return new CustomerItemViewModel
            {
                CustomerId = entity.CustomerId,
                OrganizationName = entity.Organization?.OrganizationName,
                CustomerType = entity.CustomerType,
                PersonFullName = entity.Person != null
                    ? $"{entity.Person.FirstName} {entity.Person.LastName}".Trim()
                    : null,
                Address = entity.Address,
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
                CustomerType = entity.CustomerType,
                PersonId = entity.PersonId,
                CustomerCode = entity.CustomerCode,
                CreditLimit = entity.CreditLimit,
                PaymentTermsDays = entity.PaymentTermsDays,
                TaxExempt = entity.TaxExempt,
                Notes = entity.Notes,
                Address = entity.Address,
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
                CustomerType = vm.CustomerType,
                PersonId = vm.PersonId,
                CustomerCode = vm.CustomerCode,
                CreditLimit = vm.CreditLimit,
                PaymentTermsDays = vm.PaymentTermsDays,
                TaxExempt = vm.TaxExempt,
                Notes = vm.Notes,
                Address = vm.Address,
                IsActive = vm.IsActive,
                CreatedDate = vm.CreatedDate ?? DateTimeOffset.UtcNow,
                UpdatedDate = vm.UpdatedDate ?? DateTimeOffset.UtcNow
                
                
            };
        }

        private async Task PopulateDropdowns(CustomerEditViewModel vm)
        {
            var viewData = await _customerService.GetCreateViewDataAsync();
            if (viewData.Success && viewData.Data.Organizations != null && viewData.Data.People != null)
            {
                vm.Organizations = viewData.Data.Organizations;
                vm.People = viewData.Data.People;
            }
            else
            {
                vm.Organizations = Enumerable.Empty<SelectListItem>();
                vm.People = Enumerable.Empty<SelectListItem>();
                TempData["Error"] = viewData.Message;
            }
        }
    }
}
