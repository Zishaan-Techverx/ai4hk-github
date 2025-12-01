using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;  
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IAdminService _adminService;
        private readonly IOrganizationService _organizationService;  
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserService userService,
            IAdminService adminService,
            IOrganizationService organizationService,  
            ILogger<UserController> logger)
        {
            _userService = userService;
            _adminService = adminService;
            _organizationService = organizationService;  
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // if  (!User.HasPermission("UserManagement.Edit") && 
                //     !User.HasPermission("UserManagement.Delete"))
                // {
                //     return Forbid();
                // }

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

            //if (!User.HasPermission("UserManagement.Edit"))
            //    return Forbid();

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            var organizations = await _organizationService.GetAllOrganizationsAsync();
            ViewBag.Organizations = new SelectList(organizations, "OrganizationName", "OrganizationName", user.OrganizationName);

            ViewBag.IsDetailsView = false; 
            return View(user);
        }

        // GET: User/Details
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            var organizations = await _organizationService.GetAllOrganizationsAsync();
            ViewBag.Organizations = new SelectList(organizations, "OrganizationName", "OrganizationName", user.OrganizationName);

            ViewBag.IsDetailsView = true; 
            ViewBag.Title = "User Details";
            return View("Edit", user); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, TpaSodManagementUser user)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            if (id != user.Id)
            {
                ModelState.AddModelError("", "User ID mismatch.");
                return View(user);
            }

            //if (!User.HasPermission("UserManagement.Edit"))
            //    return Forbid();

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

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _userService.UpdateUserAsync(user);
                    if (result.success)
                    {
                        TempData["SuccessMessage"] = result.message;
                        return RedirectToAction(nameof(Index));
                    }
                    TempData["ErrorMessage"] = result.message;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating user: {UserId}", user.Id);
                    TempData["ErrorMessage"] = "An error occurred while updating the user.";
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
                TempData["ErrorMessage"] = "Please fix the validation errors and try again.";
            }
            
            var organizations = await _organizationService.GetAllOrganizationsAsync();
            ViewBag.Organizations = new SelectList(organizations, "OrganizationName", "OrganizationName", user.OrganizationName);
            
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

            // if (!User.HasPermission("UserManagement.Delete"))
            // {
            //     return Forbid();
            // }

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