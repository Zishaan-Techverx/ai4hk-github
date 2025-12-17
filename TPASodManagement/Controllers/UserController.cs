using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.User;
using System.Linq;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IAdminService _adminService;
        private readonly IOrganizationService _organizationService;
        private readonly IRegistrationService _registrationService;
        private readonly SodDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserService userService,
            IAdminService adminService,
            IOrganizationService organizationService,
            IRegistrationService registrationService,
            SodDbContext context,
            ILogger<UserController> logger)
        {
            _userService = userService;
            _adminService = adminService;
            _organizationService = organizationService;
            _registrationService = registrationService;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                var viewModel = new List<UserItemViewModel>();

                foreach (var user in users)
                {
                    var person = await _registrationService.GetUserPersonAsync(user.Id.ToString());
                    var address = await _registrationService.GetUserAddressAsync(user.Id.ToString());

                    viewModel.Add(MapToItemViewModel(user, person, address));
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users");
                TempData["ErrorMessage"] = "An error occurred while loading users.";
                return View(new List<UserItemViewModel>());
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            // Check if user has SuperAdmin role
            var roles = await _adminService.GetUserRolesAsync(user.Id);
            if (roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "SuperAdmin users cannot be edited.";
                return RedirectToAction(nameof(Index));
            }

            // Load Person and Address data
            var person = await _registrationService.GetUserPersonAsync(id);
            var address = await _registrationService.GetUserAddressAsync(id);

            var vm = MapToEditViewModel(user, person, address);
            await PopulateDropdowns(vm);
            ViewBag.IsDetailsView = false;
            return View(vm);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            // Check if user has SuperAdmin role
            var roles = await _adminService.GetUserRolesAsync(user.Id);
            if (roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "SuperAdmin users cannot be viewed.";
                return RedirectToAction(nameof(Index));
            }

            // Load Person and Address data
            var person = await _registrationService.GetUserPersonAsync(id);
            var address = await _registrationService.GetUserAddressAsync(id);

            var vm = MapToEditViewModel(user, person, address, isDetailsView: true);
            await PopulateDropdowns(vm);

            ViewBag.IsDetailsView = true;
            ViewBag.Title = "User Details";
            return View("Edit", vm);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new UserEditViewModel
            {
                IsActive = true
            };
            await PopulateDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserEditViewModel model)
        {
            // Validate password match
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Password and Confirm Password do not match.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            try
            {
                var organization = await _organizationService.GetOrganizationByNameAsync(model.OrganizationName!);
                if (organization == null)
                {
                    TempData["ErrorMessage"] = "Selected organization not found.";
                    await PopulateDropdowns(model);
                    return View(model);
                }

                var finalUsername = await _registrationService.GenerateUsernameAsync(organization.OrganizationName, model.FirstName ?? string.Empty);

                var user = new TpaSodManagementUser
                {
                    UserName = finalUsername,
                    NormalizedUserName = finalUsername.ToUpperInvariant(),
                    Email = model.Email,
                    NormalizedEmail = model.Email?.ToUpperInvariant(),
                    OrganizationName = model.OrganizationName,
                    PrimaryContact = model.PrimaryContact,
                    PhoneNumber = model.PhoneNumber ?? string.Empty,
                    IsActive = model.IsActive,
                    EmailConfirmed = false,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = false,
                    AccessFailedCount = 0
                };

                // Create user
                var createResult = await _registrationService.CreateUserAsync(user, model.Password ?? string.Empty);
                if (!createResult.Succeeded)
                {
                    TempData["ErrorMessage"] = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    await PopulateDropdowns(model);
                    return View(model);
                }

                // Create Person record
                await _registrationService.CreatePersonForUserAsync(user, model.FirstName ?? string.Empty, model.LastName ?? string.Empty);

                // Create Address record if address fields provided
                if (!string.IsNullOrEmpty(model.AddressLine1) ||
                    !string.IsNullOrEmpty(model.City) ||
                    model.StateProvinceId.HasValue ||
                    model.AddressTypeId.HasValue ||
                    !string.IsNullOrEmpty(model.PostalCode))
                {
                    var address = await _registrationService.CreateAddressForUserAsync(user);
                    address.AddressLine1 = model.AddressLine1 ?? "";
                    address.City = model.City ?? "";
                    address.PostalCode = model.PostalCode ?? "";
                    if (model.StateProvinceId.HasValue)
                        address.StateProvinceId = model.StateProvinceId.Value;
                    if (model.AddressTypeId.HasValue)
                        address.AddressTypeId = model.AddressTypeId.Value;
                    address.UpdatedDate = DateTimeOffset.UtcNow;
                    _context.Addresses.Update(address);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = $"User created successfully! Username: {finalUsername}";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                TempData["ErrorMessage"] = $"An error occurred while creating the user: {ex.Message}";
                await PopulateDropdowns(model);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserEditViewModel model)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            // Check if user has SuperAdmin role
            var existingUser = await _userService.GetUserByIdAsync(id);
            if (existingUser != null)
            {
                var roles = await _adminService.GetUserRolesAsync(existingUser.Id);
                if (roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase))
                {
                    TempData["ErrorMessage"] = "SuperAdmin users cannot be edited.";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (!long.TryParse(id, out var idLong) || idLong != model.Id)
            {
                ModelState.AddModelError("", "User ID mismatch.");
                await PopulateDropdowns(model);
                return View(model);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUserForMerge = await _userService.GetUserByIdAsync(id);
                    if (existingUserForMerge == null)
                    {
                        TempData["ErrorMessage"] = "User not found.";
                        return RedirectToAction(nameof(Index));
                    }

                    var userEntity = MapToUserEntity(model);

                    // Update User
                    var result = await _userService.UpdateUserAsync(userEntity);
                    if (result.success)
                    {
                        // Update Person
                        var person = await _registrationService.GetUserPersonAsync(id);
                        if (person != null)
                        {
                            person.FirstName = model.FirstName ?? "";
                            person.LastName = model.LastName ?? "";
                            person.UpdatedDate = DateTimeOffset.UtcNow;
                            _context.People.Update(person);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            person = await _registrationService.CreatePersonForUserAsync(existingUserForMerge, model.FirstName ?? "", model.LastName ?? "");
                            person.UpdatedDate = DateTimeOffset.UtcNow;
                            _context.People.Update(person);
                            await _context.SaveChangesAsync();
                        }

                        // Update Address
                        var address = await _registrationService.GetUserAddressAsync(id);
                        if (address != null)
                        {
                            address.AddressLine1 = model.AddressLine1 ?? "";
                            address.City = model.City ?? "";
                            address.PostalCode = model.PostalCode ?? "";
                            if (model.StateProvinceId.HasValue)
                                address.StateProvinceId = model.StateProvinceId.Value;
                            if (model.AddressTypeId.HasValue)
                                address.AddressTypeId = model.AddressTypeId.Value;
                            address.UpdatedDate = DateTimeOffset.UtcNow;
                            _context.Addresses.Update(address);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            address = await _registrationService.CreateAddressForUserAsync(existingUserForMerge);
                            address.AddressLine1 = model.AddressLine1 ?? "";
                            address.City = model.City ?? "";
                            address.PostalCode = model.PostalCode ?? "";
                            if (model.StateProvinceId.HasValue)
                                address.StateProvinceId = model.StateProvinceId.Value;
                            if (model.AddressTypeId.HasValue)
                                address.AddressTypeId = model.AddressTypeId.Value;
                            address.UpdatedDate = DateTimeOffset.UtcNow;
                            _context.Addresses.Update(address);
                            await _context.SaveChangesAsync();
                        }

                        TempData["SuccessMessage"] = result.message;
                        return RedirectToAction(nameof(Index));
                    }
                    TempData["ErrorMessage"] = result.message ?? "An error occurred while updating the user.";
                    _logger.LogError("User update failed: {UserId}, Error: {Error}", userEntity.Id, result.message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating user: {UserId}, Exception: {ExceptionMessage}, InnerException: {InnerException}", 
                        model.Id, ex.Message, ex.InnerException?.Message);
                    TempData["ErrorMessage"] = $"An error occurred while updating the user: {ex.Message}";
                }
            }
            else
            {
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key]?.Errors ?? Enumerable.Empty<ModelError>();
                    foreach (var error in errors)
                    {
                        _logger.LogWarning("Validation error for {Key}: {Error}", key, error.ErrorMessage);
                    }
                }
            }

            await PopulateDropdowns(model);
            return View(model);
        }

        // POST: User/DeleteUser - For AJAX modal deletion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { success = false, message = "User ID is required." });
            }

            try
            {
                // Check if user has SuperAdmin role
                var user = await _userService.GetUserByIdAsync(id);
                if (user != null)
                {
                    var roles = await _adminService.GetUserRolesAsync(user.Id);
                    if (roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase))
                    {
                        return BadRequest(new { success = false, message = "SuperAdmin users cannot be deleted." });
                    }
                }

                var result = await _userService.DeleteUserAsync(id);
                if (result.success)
                {
                    return Ok(new { success = true, message = result.message });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", id);
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the user." });
            }
        }

        private static UserItemViewModel MapToItemViewModel(TpaSodManagementUser user, Person? person, Address? address)
        {
            return new UserItemViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FirstName = person?.FirstName,
                LastName = person?.LastName,
                AddressLine1 = address?.AddressLine1,
                City = address?.City,
                StateName = address?.StateProvince?.StateName,
                PostalCode = address?.PostalCode,
                IsActive = user.IsActive
            };
        }

        private static UserEditViewModel MapToEditViewModel(TpaSodManagementUser user, Person? person, Address? address, bool isDetailsView = false)
        {
            return new UserEditViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                OrganizationName = user.OrganizationName,
                PrimaryContact = user.PrimaryContact,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                FirstName = person?.FirstName,
                LastName = person?.LastName,
                AddressTypeId = address?.AddressTypeId,
                StateProvinceId = address?.StateProvinceId,
                StateProvinceName = address?.StateProvince?.StateName,
                AddressLine1 = address?.AddressLine1,
                City = address?.City,
                PostalCode = address?.PostalCode,
                IsDetailsView = isDetailsView
            };
        }

        private static TpaSodManagementUser MapToUserEntity(UserEditViewModel model)
        {
            return new TpaSodManagementUser
            {
                Id = model.Id,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber ?? string.Empty,
                IsActive = model.IsActive,
                PrimaryContact = model.PrimaryContact,
                OrganizationName = model.OrganizationName,
                UserName = model.UserName
            };
        }

        private async Task PopulateDropdowns(UserEditViewModel vm)
        {
            var organizations = await _organizationService.GetAllOrganizationsAsync();
            vm.Organizations = new SelectList(organizations, "OrganizationName", "OrganizationName", vm.OrganizationName);

            var addressTypes = await _context.AddressTypes
                .Where(at => at.IsActive)
                .OrderBy(at => at.AddressTypeName)
                .ToListAsync();
            vm.AddressTypes = new SelectList(addressTypes, "AddressTypeId", "AddressTypeName", vm.AddressTypeId);

            var stateProvinces = await _context.StateProvinces
                .Include(sp => sp.Country)
                .Where(sp => sp.IsActive)
                .OrderBy(sp => sp.StateName)
                .ToListAsync();
            vm.StateProvinces = new SelectList(stateProvinces, "StateProvinceId", "StateName", vm.StateProvinceId);

            if (vm.StateProvinceId.HasValue && string.IsNullOrEmpty(vm.StateProvinceName))
            {
                var selectedState = stateProvinces.FirstOrDefault(sp => sp.StateProvinceId == vm.StateProvinceId.Value);
                vm.StateProvinceName = selectedState?.StateName;
            }
        }
    }
}