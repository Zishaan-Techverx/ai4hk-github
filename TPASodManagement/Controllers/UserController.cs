using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.User;
using TpaSodManagement.Database.Entities;
using TpaSodManagement.Database;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IAdminService _adminService;
        private readonly IOrganizationService _organizationService;
        private readonly IRegistrationService _registrationService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserController> _logger;
        private readonly UserManager<TpaSodManagementUser> _userManager;
        private readonly TpaSodManagement.Utilities.IExportToExcel _exportToExcel;

        public UserController(
            IUserService userService,
            IAdminService adminService,
            IOrganizationService organizationService,
            IRegistrationService registrationService,
            ApplicationDbContext context,
            ILogger<UserController> logger,
            UserManager<TpaSodManagementUser> userManager,
            TpaSodManagement.Utilities.IExportToExcel exportToExcel)
        {
            _userService = userService;
            _adminService = adminService;
            _organizationService = organizationService;
            _registrationService = registrationService;
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _exportToExcel = exportToExcel;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Get current logged-in user
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return View(new List<UserItemViewModel>());
                }

                // Check if current user is SuperAdmin
                var currentUserRoles = await _adminService.GetUserRolesAsync(currentUser.Id);
                bool isSuperAdmin = currentUserRoles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase);
                
                // If SuperAdmin, show all users (pass null to get all organizations)
                // Otherwise, filter by current user's organization
                long? organizationFilter = isSuperAdmin ? null : currentUser.OrganizationId;
                
                // Get users filtered by organization (or all if SuperAdmin)
                var users = await _userService.GetAllUsersAsync(organizationFilter);
                var viewModel = new List<UserItemViewModel>();

                foreach (var user in users)
                {
                    var person = await _registrationService.GetUserPersonAsync(user.Id.ToString());
                    var address = await _registrationService.GetUserAddressAsync(user.Id.ToString());

                    viewModel.Add(MapToItemViewModel(user, person, address));
                }

                // Set ViewBag properties for filter partial
                ViewBag.FilterColumns = new Dictionary<string, string>
                {
                    { "UserName", "User Name" },
                    { "Email", "Email" },
                    { "FirstName", "First Name" },
                    { "LastName", "Last Name" },
                    { "PhoneNumber", "Phone Number" },
                    { "City", "City" },
                    { "StateName", "State" },
                    { "PostalCode", "Postal Code" },
                    { "IsActive", "Is Active" }
                };
                ViewBag.ModuleName = "Users";
                ViewBag.BooleanColumns = new HashSet<string> { "IsActive" };

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
                if (!model.OrganizationId.HasValue)
                {
                    TempData["ErrorMessage"] = "Please select an organization.";
                    await PopulateDropdowns(model);
                    return View(model);
                }

                var organization = await _organizationService.GetOrganizationByIdAsync(model.OrganizationId.Value);
                if (organization == null)
                {
                    TempData["ErrorMessage"] = "Selected organization not found.";
                    await PopulateDropdowns(model);
                    return View(model);
                }

                var finalUsername = await _registrationService.GenerateUsernameAsync(organization, model.FirstName ?? string.Empty);

                var user = new TpaSodManagementUser
                {
                    UserName = finalUsername,
                    NormalizedUserName = finalUsername.ToUpperInvariant(),
                    Email = model.Email,
                    NormalizedEmail = model.Email?.ToUpperInvariant(),
                    OrganizationId = model.OrganizationId.Value,
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

        // POST: User/ResetPassword - For AJAX password reset
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id, string newPassword)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { success = false, message = "User ID is required." });
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                return BadRequest(new { success = false, message = "New password is required." });
            }

            try
            {
                var result = await _userService.ResetPasswordAsync(id, newPassword);
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
                _logger.LogError(ex, "Error resetting password for user: {UserId}", id);
                return StatusCode(500, new { success = false, message = "An error occurred while resetting the password." });
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
                OrganizationId = user.OrganizationId,
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
                OrganizationId = model.OrganizationId,
                UserName = model.UserName
            };
        }

        [HttpPost]
        public async Task<IActionResult> Filter([FromBody] Dictionary<string, string> filters)
        {
            try
            {
                // Get current logged-in user
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                // Check if current user is SuperAdmin
                var currentUserRoles = await _adminService.GetUserRolesAsync(currentUser.Id);
                bool isSuperAdmin = currentUserRoles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase);
                
                // If SuperAdmin, show all users (pass null to get all organizations)
                // Otherwise, filter by current user's organization
                long? organizationFilter = isSuperAdmin ? null : currentUser.OrganizationId;

                var result = await _userService.GetFilteredAsync(filters ?? new Dictionary<string, string>(), organizationFilter);
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var viewModel = new List<UserItemViewModel>();
                foreach (var user in result.Data ?? new List<TpaSodManagementUser>())
                {
                    var person = await _registrationService.GetUserPersonAsync(user.Id.ToString());
                    var address = await _registrationService.GetUserAddressAsync(user.Id.ToString());
                    viewModel.Add(MapToItemViewModel(user, person, address));
                }

                return Json(new { success = true, data = viewModel });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering users");
                return Json(new { success = false, message = $"Error filtering data: {ex.Message}" });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
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

                // Get current logged-in user
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                // Check if current user is SuperAdmin
                var currentUserRoles = await _adminService.GetUserRolesAsync(currentUser.Id);
                bool isSuperAdmin = currentUserRoles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase);
                
                // If SuperAdmin, show all users (pass null to get all organizations)
                // Otherwise, filter by current user's organization
                long? organizationFilter = isSuperAdmin ? null : currentUser.OrganizationId;

                var result = await _userService.GetFilteredAsync(filters, organizationFilter);
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var users = result.Data ?? new List<TpaSodManagementUser>();
                var vm = new List<UserItemViewModel>();
                foreach (var user in users)
                {
                    var person = await _registrationService.GetUserPersonAsync(user.Id.ToString());
                    var address = await _registrationService.GetUserAddressAsync(user.Id.ToString());
                    vm.Add(MapToItemViewModel(user, person, address));
                }

                var allColumns = new List<(string Header, string PropertyName)>
                {
                    ("User Name", "UserName"),
                    ("Email", "Email"),
                    ("First Name", "FirstName"),
                    ("Last Name", "LastName"),
                    ("Phone Number", "PhoneNumber"),
                    ("Address", "AddressLine1"),
                    ("City", "City"),
                    ("State", "StateName"),
                    ("Postal Code", "PostalCode"),
                    ("Is Active", "IsActive")
                };

                var visibleColumns = allColumns.Where(col => !hiddenColumns.Contains(col.PropertyName)).ToList();
                var columnHeaders = visibleColumns.Select(col => col.Header).ToList();
                var columnIndices = visibleColumns.Select(col => allColumns.IndexOf(allColumns.First(c => c.PropertyName == col.PropertyName))).ToList();

                var stream = _exportToExcel.GenerateExcel(
                    moduleName: "Users",
                    worksheetName: "Users",
                    columnHeaders: columnHeaders,
                    data: vm,
                    rowMapper: item =>
                    {
                        var allValues = new List<object>
                        {
                            item.UserName ?? "",
                            item.Email ?? "",
                            item.FirstName ?? "",
                            item.LastName ?? "",
                            item.PhoneNumber ?? "",
                            item.AddressLine1 ?? "",
                            item.City ?? "",
                            item.StateName ?? "",
                            item.PostalCode ?? "",
                            item.IsActive ? "Active" : "Inactive"
                        };
                        return columnIndices.Select(idx => allValues[idx]).ToList();
                    }
                );

                var fileName = $"Users_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Excel for users");
                return Json(new { success = false, message = $"Error generating Excel: {ex.Message}" });
            }
        }

        private async Task PopulateDropdowns(UserEditViewModel vm)
        {
            var organizations = await _organizationService.GetAllOrganizationsAsync();
            vm.Organizations = new SelectList(organizations, "OrganizationId", "OrganizationName", vm.OrganizationId);

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