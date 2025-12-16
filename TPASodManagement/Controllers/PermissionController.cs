using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Models;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class PermissionController : Controller
    {
        private readonly IPermissionService _permissionService;
        private readonly RoleManager<IdentityRole<long>> _roleManager;
        private readonly ILogger<PermissionController> _logger;

        public PermissionController(
            IPermissionService permissionService,
            RoleManager<IdentityRole<long>> roleManager,
            ILogger<PermissionController> logger)
        {
            _permissionService = permissionService;
            _roleManager = roleManager;
            _logger = logger;
        }

        // GET: Permission/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var roles = await _roleManager.Roles.ToListAsync();
                return View(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading permissions page");
                TempData["ErrorMessage"] = "An error occurred while loading permissions page.";
                return View();
            }
        }

        // GET: Permission/Manage/{roleId}
        public async Task<IActionResult> Manage(long roleId)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(roleId.ToString());
                if (role == null)
                    return View("NotFound");

                // Check if role is SuperAdmin
                if (role.Name.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["ErrorMessage"] = "SuperAdmin role permissions cannot be managed.";
                    return RedirectToAction("Index");
                }

                var permissions = await _permissionService.GetAllPermissionsAsync();
                var rolePermissions = await _permissionService.GetRolePermissionsAsync(roleId);

                var model = new ManagePermissionsViewModel
                {
                    Role = role,
                    AllPermissions = permissions,
                    RolePermissions = rolePermissions
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading permission management for role: {RoleId}", roleId);
                TempData["ErrorMessage"] = "An error occurred while loading permissions.";
                return RedirectToAction("Index");
            }
        }

        // POST: Permission/Manage
        [HttpPost]
        public async Task<IActionResult> Manage(long roleId, Dictionary<int, RolePermission> permissions)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(roleId.ToString());
                if (role == null)
                {
                    TempData["ErrorMessage"] = "Role not found.";
                    return RedirectToAction("Index");
                }

                // Check if role is SuperAdmin
                if (role.Name.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["ErrorMessage"] = "SuperAdmin role permissions cannot be managed.";
                    return RedirectToAction("Index");
                }

                var permissionList = permissions?.Values.ToList() ?? new List<RolePermission>();
                var success = await _permissionService.UpdateRolePermissionsAsync(roleId, permissionList);

                if (success)
                {
                    TempData["SuccessMessage"] = "Permissions updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update permissions.";
                }

                return RedirectToAction("Manage", new { roleId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating permissions for role: {RoleId}", roleId);
                TempData["ErrorMessage"] = "An error occurred while updating permissions.";
                return RedirectToAction("Manage", new { roleId });
            }
        }
    }

    public class ManagePermissionsViewModel
    {
        public IdentityRole<long> Role { get; set; }
        public List<Permission> AllPermissions { get; set; }
        public List<RolePermission> RolePermissions { get; set; }
    }
}