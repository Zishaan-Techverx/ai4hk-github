using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.Customer;
using OfficeOpenXml;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            // Set filter columns for the partial view
            ViewBag.FilterColumns = new Dictionary<string, string>
            {
                { "CustomerType", "Customer Type" },
                { "CustomerCode", "Customer Code" },
                { "CreditLimit", "Credit Limit" },
                { "PaymentTermsDays", "Payment Terms Days" },
                { "TaxExempt", "Tax Exempt" },
                { "Notes", "Notes" },
                { "IsActive", "Is Active" },
                { "CreatedDate", "Created Date" },
                { "UpdatedDate", "Updated Date" },
                { "Organization", "Organization" },
                { "Person", "Person" }
            };
            ViewBag.ModuleName = "Customers";
            ViewBag.BooleanColumns = new HashSet<string> { "IsActive", "TaxExempt" };
            ViewBag.DateColumns = new HashSet<string> { "CreatedDate", "UpdatedDate" };

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
            var result = await _customerService.DeleteAsync(id);
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
        public async Task<IActionResult> Print([FromBody] Dictionary<string, string> filters)
        {
            try
            {
                // Get all filtered records (no pagination)
                var result = await _customerService.GetFilteredAsync(filters ?? new Dictionary<string, string>());
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var customers = result.Data ?? new List<Customer>();
                var vm = customers.Select(MapToItemViewModel).ToList();

                // Set EPPlus license context (non-commercial use)
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                // Generate Excel file using EPPlus
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Customers");

                    // Set header row
                    worksheet.Cells[1, 1].Value = "Customer Type";
                    worksheet.Cells[1, 2].Value = "Customer Code";
                    worksheet.Cells[1, 3].Value = "Credit Limit";
                    worksheet.Cells[1, 4].Value = "Payment Terms Days";
                    worksheet.Cells[1, 5].Value = "Tax Exempt";
                    worksheet.Cells[1, 6].Value = "Notes";
                    worksheet.Cells[1, 7].Value = "Is Active";
                    worksheet.Cells[1, 8].Value = "Organization";
                    worksheet.Cells[1, 9].Value = "Person";

                    // Style header row
                    using (var range = worksheet.Cells[1, 1, 1, 9])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }

                    // Add data rows
                    for (int i = 0; i < vm.Count; i++)
                    {
                        var row = i + 2;
                        var item = vm[i];
                        worksheet.Cells[row, 1].Value = item.CustomerType ?? "";
                        worksheet.Cells[row, 2].Value = item.CustomerCode ?? "";
                        worksheet.Cells[row, 3].Value = item.CreditLimit?.ToString("N2") ?? "";
                        worksheet.Cells[row, 4].Value = item.PaymentTermsDays ?? 0;
                        worksheet.Cells[row, 5].Value = item.TaxExempt ? "Yes" : "No";
                        worksheet.Cells[row, 6].Value = item.Notes ?? "";
                        worksheet.Cells[row, 7].Value = item.IsActive ? "Yes" : "No";
                        worksheet.Cells[row, 8].Value = item.OrganizationName ?? "";
                        worksheet.Cells[row, 9].Value = item.PersonFullName ?? "";
                    }

                    // Auto-fit columns
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    // Add borders to data cells
                    if (vm.Count > 0)
                    {
                        using (var range = worksheet.Cells[1, 1, vm.Count + 1, 9])
                        {
                            range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        }
                    }

                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;

                    var fileName = $"Customers_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
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
                CustomerType = entity.CustomerType,
                CustomerCode = entity.CustomerCode,
                CreditLimit = entity.CreditLimit,
                PaymentTermsDays = entity.PaymentTermsDays,
                TaxExempt = entity.TaxExempt,
                Notes = entity.Notes,
                IsActive = entity.IsActive,
                CreatedDate = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate,
                OrganizationName = entity.Organization?.OrganizationName,
                PersonFullName = entity.Person != null
                    ? $"{entity.Person.FirstName} {entity.Person.LastName}".Trim()
                    : null
            };
        }

        private static CustomerEditViewModel MapToEditViewModel(Customer entity, bool isDetailsView = false)
        {
            return new CustomerEditViewModel
            {
                CustomerId = entity.CustomerId,
                CustomerType = entity.CustomerType,
                CustomerCode = entity.CustomerCode,
                CreditLimit = entity.CreditLimit,
                PaymentTermsDays = entity.PaymentTermsDays,
                TaxExempt = entity.TaxExempt,
                Notes = entity.Notes,
                IsActive = entity.IsActive,
                CreatedDate = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate,
                OrganizationId = entity.OrganizationId,
                PersonId = entity.PersonId,
                IsDetailsView = isDetailsView
            };
        }

        private static Customer MapToEntity(CustomerEditViewModel vm)
        {
            return new Customer
            {
                CustomerId = vm.CustomerId,
                CustomerType = vm.CustomerType,
                CustomerCode = vm.CustomerCode,
                CreditLimit = vm.CreditLimit,
                PaymentTermsDays = vm.PaymentTermsDays,
                TaxExempt = vm.TaxExempt,
                Notes = vm.Notes,
                IsActive = vm.IsActive,
                CreatedDate = vm.CreatedDate ?? DateTimeOffset.UtcNow,
                UpdatedDate = vm.UpdatedDate ?? DateTimeOffset.UtcNow,
                OrganizationId = vm.OrganizationId,
                PersonId = vm.PersonId
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
