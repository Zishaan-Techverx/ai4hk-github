using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.Customer;

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

        public async Task<IActionResult> Index()
        {
            var result = await _customerService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(new List<CustomerItemViewModel>());
            }
            var vm = result.Data?.Select(MapToItemViewModel).ToList() ?? new List<CustomerItemViewModel>();
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
