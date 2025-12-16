using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;

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
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users");
                TempData["ErrorMessage"] = "An error occurred while loading users.";
                return View(new List<TpaSodManagementUser>());
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            // Load Person and Address data
            var person = await _registrationService.GetUserPersonAsync(id);
            var address = await _registrationService.GetUserAddressAsync(id);

            // Load dropdowns
            var organizations = await _organizationService.GetAllOrganizationsAsync();
            ViewBag.Organizations = new SelectList(organizations, "OrganizationName", "OrganizationName", user.OrganizationName);

            // Load AddressTypes for dropdown
            var addressTypes = await _context.AddressTypes
                .Where(at => at.IsActive)
                .OrderBy(at => at.AddressTypeName)
                .ToListAsync();
            ViewBag.AddressTypes = new SelectList(addressTypes, "AddressTypeId", "AddressTypeName", address?.AddressTypeId);

            // Load StateProvinces for dropdown
            var stateProvinces = await _context.StateProvinces
                .Include(sp => sp.Country)
                .Where(sp => sp.IsActive)
                .OrderBy(sp => sp.StateName)
                .ToListAsync();
            ViewBag.StateProvinces = new SelectList(stateProvinces, "StateProvinceId", "StateName", address?.StateProvinceId);

            // Pass Person and Address to ViewBag
            ViewBag.Person = person;
            ViewBag.Address = address;

            ViewBag.IsDetailsView = false;
            return View(user);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            // Load Person and Address data
            var person = await _registrationService.GetUserPersonAsync(id);
            var address = await _registrationService.GetUserAddressAsync(id);

            var organizations = await _organizationService.GetAllOrganizationsAsync();
            ViewBag.Organizations = new SelectList(organizations, "OrganizationName", "OrganizationName", user.OrganizationName);

            // Load AddressTypes for dropdown
            var addressTypes = await _context.AddressTypes
                .Where(at => at.IsActive)
                .OrderBy(at => at.AddressTypeName)
                .ToListAsync();
            ViewBag.AddressTypes = new SelectList(addressTypes, "AddressTypeId", "AddressTypeName", address?.AddressTypeId);

            // Load StateProvinces for dropdown
            var stateProvinces = await _context.StateProvinces
                .Include(sp => sp.Country)
                .Where(sp => sp.IsActive)
                .OrderBy(sp => sp.StateName)
                .ToListAsync();
            ViewBag.StateProvinces = new SelectList(stateProvinces, "StateProvinceId", "StateName", address?.StateProvinceId);

            // Pass Person and Address to ViewBag
            ViewBag.Person = person;
            ViewBag.Address = address;

            ViewBag.IsDetailsView = true;
            ViewBag.Title = "User Details";
            return View("Edit", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, TpaSodManagementUser user, 
            string firstName, string lastName, 
            string addressLine1, string city, int? stateProvinceId, string postalCode, int? addressTypeId)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            if (id != user.Id.ToString())
            {
                ModelState.AddModelError("", "User ID mismatch.");
                return View(user);
            }

            ModelState.Remove("EmailConfirmed");
            ModelState.Remove("PhoneNumberConfirmed");
            ModelState.Remove("TwoFactorEnabled");
            ModelState.Remove("LockoutEnabled");
            ModelState.Remove("AccessFailedCount");
            ModelState.Remove("SecurityStamp");
            ModelState.Remove("ConcurrencyStamp");
            ModelState.Remove("NormalizedEmail");
            ModelState.Remove("NormalizedUserName");
            ModelState.Remove("OrganizationName");
            ModelState.Remove("PrimaryContact");
            ModelState.Remove("PhoneNumber"); // Add this line - Identity's PhoneNumber property

            if (ModelState.IsValid)
            {
                try
                {
                    // Get existing user to preserve fields that might not be posted (if disabled)
                    var existingUserForMerge = await _userService.GetUserByIdAsync(id);
                    if (existingUserForMerge == null)
                    {
                        TempData["ErrorMessage"] = "User not found.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Preserve fields that might not be posted if they're disabled
                    if (string.IsNullOrEmpty(user.Email))
                    {
                        user.Email = existingUserForMerge.Email;
                    }
                    if (string.IsNullOrEmpty(user.UserName))
                    {
                        user.UserName = existingUserForMerge.UserName;
                    }
                    if (string.IsNullOrEmpty(user.PhoneNumber))
                    {
                        user.PhoneNumber = existingUserForMerge.PhoneNumber ?? string.Empty;
                    }

                    // Update Person
                    var person = await _registrationService.GetUserPersonAsync(id);
                    if (person != null)
                    {
                        person.FirstName = firstName ?? "";
                        person.LastName = lastName ?? "";
                        person.UpdatedDate = DateTimeOffset.UtcNow;
                        _context.People.Update(person);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        // Create Person if doesn't exist
                        var existingUser = await _userService.GetUserByIdAsync(id);
                        if (existingUser != null)
                        {
                            person = await _registrationService.CreatePersonForUserAsync(existingUser, firstName ?? "", lastName ?? "");
                            person.UpdatedDate = DateTimeOffset.UtcNow;
                            _context.People.Update(person);
                            await _context.SaveChangesAsync();
                        }
                    }

                    // Update Address
                    var address = await _registrationService.GetUserAddressAsync(id);
                    if (address != null)
                    {
                        address.AddressLine1 = addressLine1 ?? "";
                        address.City = city ?? "";
                        address.PostalCode = postalCode ?? "";
                        if (stateProvinceId.HasValue)
                            address.StateProvinceId = stateProvinceId.Value;
                        if (addressTypeId.HasValue)
                            address.AddressTypeId = addressTypeId.Value;
                        address.UpdatedDate = DateTimeOffset.UtcNow;
                        _context.Addresses.Update(address);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        // Create Address if doesn't exist
                        var existingUser = await _userService.GetUserByIdAsync(id);
                        if (existingUser != null)
                        {
                            address = await _registrationService.CreateAddressForUserAsync(existingUser);
                            address.AddressLine1 = addressLine1 ?? "";
                            address.City = city ?? "";
                            address.PostalCode = postalCode ?? "";
                            if (stateProvinceId.HasValue)
                                address.StateProvinceId = stateProvinceId.Value;
                            if (addressTypeId.HasValue)
                                address.AddressTypeId = addressTypeId.Value;
                            address.UpdatedDate = DateTimeOffset.UtcNow;
                            _context.Addresses.Update(address);
                            await _context.SaveChangesAsync();
                        }
                    }

                    // Update User
                    var result = await _userService.UpdateUserAsync(user);
                    if (result.success)
                    {
                        TempData["SuccessMessage"] = result.message;
                        return RedirectToAction(nameof(Index));
                    }
                    TempData["ErrorMessage"] = result.message ?? "An error occurred while updating the user.";
                    _logger.LogError("User update failed: {UserId}, Error: {Error}", user.Id, result.message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating user: {UserId}, Exception: {ExceptionMessage}, InnerException: {InnerException}", 
                        user.Id, ex.Message, ex.InnerException?.Message);
                    TempData["ErrorMessage"] = $"An error occurred while updating the user: {ex.Message}";
                }
            }
            else
            {
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    foreach (var error in errors)
                    {
                        _logger.LogWarning("Validation error for {Key}: {Error}", key, error.ErrorMessage);
                    }
                }
                // Don't set TempData["ErrorMessage"] here - let field-specific errors show
            }

            // Reload dropdowns for error view
            var organizations = await _organizationService.GetAllOrganizationsAsync();
            ViewBag.Organizations = new SelectList(organizations, "OrganizationName", "OrganizationName", user.OrganizationName);

            var addressTypes = await _context.AddressTypes
                .Where(at => at.IsActive)
                .OrderBy(at => at.AddressTypeName)
                .ToListAsync();
            ViewBag.AddressTypes = new SelectList(addressTypes, "AddressTypeId", "AddressTypeName", addressTypeId);

            var stateProvinces = await _context.StateProvinces
                .Include(sp => sp.Country)
                .Where(sp => sp.IsActive)
                .OrderBy(sp => sp.StateName)
                .ToListAsync();
            ViewBag.StateProvinces = new SelectList(stateProvinces, "StateProvinceId", "StateName", stateProvinceId);

            // Reload Person and Address
            var personReload = await _registrationService.GetUserPersonAsync(id);
            var addressReload = await _registrationService.GetUserAddressAsync(id);
            ViewBag.Person = personReload;
            ViewBag.Address = addressReload;

            return View(user);
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
    }
}