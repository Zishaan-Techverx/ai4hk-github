using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.Areas.Identity.Data; // TpaSodManagementUser ke liye
using System.Collections.Generic; // List aur Tuple ke liye
using System.Linq; // LINQ use karne ke liye

[Authorize]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAdminService adminService, ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    // GET: Admin/Index
    [Authorize(Policy = "CanViewAdmin")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var roles = await _adminService.GetAllRolesAsync();
            var users = await _adminService.GetAllUsersAsync();

            var model = new
            {
                Roles = roles,
                Users = users
            };

            return View("~/Views/AdminPanel/Index.cshtml", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading admin page");
            TempData["ErrorMessage"] = "An error occurred while loading the page.";
            return View("~/Views/AdminPanel/Index.cshtml");
        }
    }

    // POST: Admin/AssignRoleToUser
    // UPDATED: Single role assignment logic ko use karega.
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
    // NEW: User se ek specific role hatane ke liye
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

            // Note: GetRoleByIdAsync expects long, but we're using roleName here which seems incorrect
            // This code path might need review
            return RedirectToAction("Index");

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
            
            var role = await _adminService.GetRoleByIdAsync(roleIdLong);
            if (role == null)
                return View("NotFound");

            var usersInRole = await _adminService.GetUsersInRoleAsync(role.Name);
            ViewBag.UsersInRole = usersInRole.Select(u => u.UserName).ToList();
            ViewBag.RoleName = role.Name;

            return View("~/Views/AdminPanel/Edit.cshtml", role);
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
            if (checkRole != null && checkRole.Name.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
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

            var role = await _adminService.GetRoleByIdAsync(roleIdLong);
            TempData["ErrorMessage"] = message;
            return View("~/Views/AdminPanel/Edit.cshtml", role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role: {RoleId}", id);
            ModelState.AddModelError("", "An error occurred while updating the role.");
            if (long.TryParse(id, out long roleIdLong2))
            {
                var role = await _adminService.GetRoleByIdAsync(roleIdLong2);
                return View("~/Views/AdminPanel/Edit.cshtml", role);
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
            if (role != null && role.Name.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
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
            
            var role = await _adminService.GetRoleByIdAsync(roleIdLong);
            if (role == null)
                return View("NotFound");

            var usersInRole = await _adminService.GetUsersInRoleAsync(role.Name);

            var usersWithRoles = new List<(TpaSodManagementUser user, List<string> roles)>();
            foreach (var user in usersInRole)
            {
                var roles = await _adminService.GetUserRolesAsync(user.Id);
                usersWithRoles.Add((user, roles));
            }

            ViewBag.UsersInRole = usersWithRoles;
            ViewBag.RoleName = role.Name;
            ViewBag.RoleId = role.Id;

            return View("~/Views/AdminPanel/Detail.cshtml", role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role details: {RoleId}", id);
            TempData["ErrorMessage"] = "An error occurred while retrieving role details.";
            return RedirectToAction("Index");
        }
    }
}