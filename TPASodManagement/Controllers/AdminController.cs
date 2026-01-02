using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.Areas.Identity.Data; // TpaSodManagementUser ke liye
using System.Collections.Generic; // List aur Tuple ke liye
using System.Linq; // LINQ use karne ke liye
using TpaSodManagement.ViewModels.Admin;

[Authorize]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;
    private readonly UserManager<TpaSodManagementUser> _userManager;

    public AdminController(
        IAdminService adminService, 
        ILogger<AdminController> logger,
        UserManager<TpaSodManagementUser> userManager)
    {
        _adminService = adminService;
        _logger = logger;
        _userManager = userManager;
    }

    // GET: Admin/Index
    [Authorize(Policy = "CanViewAdmin")]
    public async Task<IActionResult> Index()
    {
        try
        {
            // Get current logged-in user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return View("~/Views/AdminPanel/Index.cshtml", new AdminIndexViewModel());
            }

            // Check if current user is SuperAdmin
            var currentUserRoles = await _adminService.GetUserRolesAsync(currentUser.Id);
            bool isSuperAdmin = currentUserRoles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase);
            
            // If SuperAdmin, show all users (pass null to get all organizations)
            // Otherwise, filter by current user's organization
            long? organizationFilter = isSuperAdmin ? null : currentUser.OrganizationId;
            
            var model = await _adminService.GetAdminIndexViewModelAsync(organizationFilter);
            return View("~/Views/AdminPanel/Index.cshtml", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading admin page");
            TempData["ErrorMessage"] = "An error occurred while loading the page.";
            return View("~/Views/AdminPanel/Index.cshtml", new AdminIndexViewModel());
        }
    }

    // POST: Admin/AssignRoleToUser
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CanEditAdmin")]
    public async Task<IActionResult> AssignRoleToUser(string userId, string roleName)
    {
        try
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleName))
            {
                TempData["ErrorMessage"] = "Please select both user and role.";
                return RedirectToAction("Index");
            }

            if (!long.TryParse(userId, out long userIdLong))
            {
                TempData["ErrorMessage"] = "Invalid user ID.";
                return RedirectToAction("Index");
            }
            var (success, message) = await _adminService.AssignRoleToUserAsync(userIdLong, roleName);

            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role {RoleName} to user {UserId}", roleName, userId);
            TempData["ErrorMessage"] = "An error occurred while assigning the role.";
            return RedirectToAction("Index");
        }
    }

    // POST: Admin/RemoveRoleFromUser
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CanEditAdmin")]
    public async Task<IActionResult> RemoveRoleFromUser(string userId, string roleName)
    {
        try
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleName))
            {
                TempData["ErrorMessage"] = "User ID and Role Name are required for removal.";
                return RedirectToAction("Index");
            }

            if (!long.TryParse(userId, out long userIdLong))
            {
                TempData["ErrorMessage"] = "Invalid user ID.";
                return RedirectToAction("Index");
            }
            var (success, message) = await _adminService.RemoveRoleFromUserAsync(userIdLong, roleName);

            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role {RoleName} from user {UserId}", roleName, userId);
            TempData["ErrorMessage"] = "An error occurred while removing the role.";
            return RedirectToAction("Index");
        }
    }


    // GET: Admin/CreateRole
    [Authorize(Policy = "CanCreateAdmin")]
    public IActionResult CreateRole()
    {
        return View("~/Views/AdminPanel/Create.cshtml");
    }

    // POST: Admin/CreateRole
    [HttpPost]
    [Authorize(Policy = "CanCreateAdmin")]
    public async Task<IActionResult> CreateRole(string roleName)
    {
        try
        {
            var (success, message) = await _adminService.CreateRoleAsync(roleName);

            if (success)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", message);
            return View("~/Views/AdminPanel/Create.cshtml");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role: {RoleName}", roleName);
            ModelState.AddModelError("", "An error occurred while creating the role.");
            return View("~/Views/AdminPanel/Create.cshtml");
        }
    }

    // GET: Admin/EditRole/{id}
    [Authorize(Policy = "CanViewAdmin")]
    public async Task<IActionResult> EditRole(string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
                return View("NotFound");

            if (!long.TryParse(id, out long roleIdLong))
                return View("NotFound");
            
            var roleVm = await _adminService.GetEditRoleViewModelAsync(roleIdLong);
            if (roleVm == null)
                return View("NotFound");

            // Block SuperAdmin edit (behavior unchanged)
            if (roleVm.Name.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "SuperAdmin role cannot be edited.";
                return RedirectToAction("Index");
            }

            return View("~/Views/AdminPanel/Edit.cshtml", roleVm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role for edit: {RoleId}", id);
            TempData["ErrorMessage"] = "An error occurred while retrieving the role.";
            return RedirectToAction("Index");
        }
    }

    // POST: Admin/EditRole
    [HttpPost]
    [Authorize(Policy = "CanEditAdmin")]
    public async Task<IActionResult> EditRole(string id, string roleName)
    {
        try
        {
            if (!long.TryParse(id, out long roleIdLong))
            {
                TempData["ErrorMessage"] = "Invalid role ID.";
                return RedirectToAction("Index");
            }

            // Check if role is SuperAdmin
            var checkRole = await _adminService.GetRoleByIdAsync(roleIdLong);
            if (checkRole != null && string.Equals(checkRole.Name, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "SuperAdmin role cannot be edited.";
                return RedirectToAction("Index");
            }

            var (success, message) = await _adminService.UpdateRoleAsync(roleIdLong, roleName);

            if (success)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToAction("Index");
            }

            var roleVm = await _adminService.GetEditRoleViewModelAsync(roleIdLong);
            TempData["ErrorMessage"] = message;
            return View("~/Views/AdminPanel/Edit.cshtml", roleVm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role: {RoleId}", id);
            ModelState.AddModelError("", "An error occurred while updating the role.");
            if (long.TryParse(id, out long roleIdLong2))
            {
                var roleVm = await _adminService.GetEditRoleViewModelAsync(roleIdLong2);
                return View("~/Views/AdminPanel/Edit.cshtml", roleVm);
            }
            return RedirectToAction("Index");
        }
    }

    // GET: Admin/DeleteRole/{id}
    [Authorize(Policy = "CanViewAdmin")]
    public async Task<IActionResult> DeleteRole(string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
                return View("NotFound");

            if (!long.TryParse(id, out long roleIdLong))
                return View("NotFound");
            
            var role = await _adminService.GetRoleByIdAsync(roleIdLong);
            if (role == null)
                return View("NotFound");

            if (string.IsNullOrWhiteSpace(role.Name))
                return View("NotFound");

            var usersInRole = await _adminService.GetUsersInRoleAsync(role.Name);
            ViewBag.UsersInRole = usersInRole;

            return View("~/Views/AdminPanel/Delete.cshtml", role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role for deletion: {RoleId}", id);
            TempData["ErrorMessage"] = "An error occurred while retrieving the role.";
            return RedirectToAction("Index");
        }
    }

    // POST: Admin/DeleteRole
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CanDeleteAdmin")]
    public async Task<IActionResult> DeleteRole(string id, string confirm)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Role ID is required.";
                return RedirectToAction("Index");
            }

            if (!long.TryParse(id, out long roleIdLong))
            {
                TempData["ErrorMessage"] = "Invalid role ID.";
                return RedirectToAction("Index");
            }

            // Check if role is SuperAdmin
            var role = await _adminService.GetRoleByIdAsync(roleIdLong);
            if (role != null && string.Equals(role.Name, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "SuperAdmin role cannot be deleted.";
                return RedirectToAction("Index");
            }

            var (success, message) = await _adminService.DeleteRoleAsync(roleIdLong);

            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role: {RoleId}", id);
            TempData["ErrorMessage"] = "An error occurred while deleting the role.";
            return RedirectToAction("Index");
        }
    }

    // GET: Admin/RoleDetails/{id}
    [Authorize(Policy = "CanViewAdmin")]
    public async Task<IActionResult> RoleDetails(string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
                return View("NotFound");

            if (!long.TryParse(id, out long roleIdLong))
                return View("NotFound");
            
            var roleDetails = await _adminService.GetRoleDetailsViewModelAsync(roleIdLong);
            if (roleDetails == null)
                return View("NotFound");

            return View("~/Views/AdminPanel/Detail.cshtml", roleDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role details: {RoleId}", id);
            TempData["ErrorMessage"] = "An error occurred while retrieving role details.";
            return RedirectToAction("Index");
        }
    }
}