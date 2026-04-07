using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Database;
using TpaSodManagement.Database.Entities;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Controllers;

[Authorize]
public class NotificationController : Controller
{
    private readonly INotificationService _notificationService;
    private readonly INotificationResponseService _notificationResponseService;
    private readonly UserManager<TpaSodManagementUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(
        INotificationService notificationService,
        INotificationResponseService notificationResponseService,
        UserManager<TpaSodManagementUser> userManager,
        ApplicationDbContext context,
        ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _notificationResponseService = notificationResponseService;
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    // GET: Notification/Index (Only SuperAdmin)
    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var roles = await _userManager.GetRolesAsync(currentUser);
        bool isSuperAdmin = roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase);

        if (!isSuperAdmin)
        {
            TempData["ErrorMessage"] = "Access denied. Only SuperAdmin can access this page.";
            return RedirectToAction("Index", "Home");
        }

        var notifications = await _notificationService.GetAllNotificationsAsync();
        return View(notifications);
    }

    // GET: Notification/GetOrganizations (AJAX - now farm list)
    [HttpGet]
    public async Task<IActionResult> GetOrganizations()
    {
        var farms = await _context.Farms
            .Where(f => f.DeletedDate == null && f.IsActive)
            .OrderBy(f => f.FarmName)
            .Select(f => new { OrganizationId = f.FarmId, OrganizationName = f.FarmName })
            .ToListAsync();
        return Json(farms);
    }

    // GET: Notification/GetUsersByOrganization (AJAX - now filtered by farm)
    [HttpGet]
    public async Task<IActionResult> GetUsersByOrganization(long? organizationId)
    {
        if (!organizationId.HasValue)
        {
            // Get all users (excluding SuperAdmin)
            var allUsers = await _userManager.Users
                .Include(u => u.Farm)
                .ToListAsync();

            var filteredUsers = new List<object>();
            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase))
                {
                    filteredUsers.Add(new
                    {
                        user.Id,
                        user.UserName,
                        user.Email,
                        OrganizationName = user.Farm?.FarmName ?? "N/A"
                    });
                }
            }
            return Json(filteredUsers);
        }

        var users = await _userManager.Users
            .Include(u => u.Farm)
            .Where(u => u.FarmId == organizationId.Value)
            .ToListAsync();

        var result = new List<object>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase))
            {
                result.Add(new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    OrganizationName = user.Farm?.FarmName ?? "N/A"
                });
            }
        }

        return Json(result);
    }

    // POST: Notification/Create
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Create([FromBody] CreateNotificationModel model)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, message = "User not found" });
        }

        var roles = await _userManager.GetRolesAsync(currentUser);
        bool isSuperAdmin = roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase);

        if (!isSuperAdmin)
        {
            return Json(new { success = false, message = "Access denied. Only SuperAdmin can create notifications." });
        }

        if (string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.Message) || string.IsNullOrWhiteSpace(model.Priority))
        {
            return Json(new { success = false, message = "Title, Message, and Priority are required." });
        }

        if (model.UserIds == null || model.UserIds.Count == 0)
        {
            return Json(new { success = false, message = "Please select at least one user." });
        }

        DateTimeOffset? expiryDate = null;
        if (!string.IsNullOrEmpty(model.ExpiryDate))
        {
            if (DateTimeOffset.TryParse(model.ExpiryDate, out var parsedDate))
            {
                expiryDate = parsedDate;
            }
        }

        var notification = new Notification
        {
            Title = model.Title,
            Message = model.Message,
            Priority = model.Priority,
            ExpiryDate = expiryDate,
            IsActive = true
        };

        var response = await _notificationService.CreateNotificationAsync(notification, model.UserIds);

        if (response.Success)
        {
            return Json(new { success = true, message = "Notification sent successfully!" });
        }

        return Json(new { success = false, message = response.Message });
    }

    // GET: Notification/Details/{id}
    public async Task<IActionResult> Details(long? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var roles = await _userManager.GetRolesAsync(currentUser);
        bool isSuperAdmin = roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase);

        if (!isSuperAdmin)
        {
            TempData["ErrorMessage"] = "Access denied. Only SuperAdmin can view notification details.";
            return RedirectToAction("Index", "Home");
        }

        var notification = await _notificationService.GetNotificationByIdAsync(id.Value);
        if (notification == null)
        {
            return NotFound();
        }

        // Load Person data for all users in the notification
        var users = notification.NotificationUsers
            .Where(nu => nu.User != null && nu.User.PersonId.HasValue)
            .Select(nu => nu.User!)
            .ToList();
        var personIds = users
            .Where(u => u.PersonId.HasValue)
            .Select(u => u.PersonId!.Value)
            .Distinct()
            .ToList();
        
        Dictionary<long, Person> personsDict = new Dictionary<long, Person>();
        if (personIds.Any())
        {
            var persons = await _context.People
                .Where(p => personIds.Contains(p.PersonId))
                .ToListAsync();
            personsDict = persons.ToDictionary(p => p.PersonId, p => p);
        }
        ViewBag.Persons = personsDict;

        // Fetch replies for this notification
        var replies = await _notificationResponseService.GetResponsesByNotificationIdAsync(id.Value);
        ViewBag.Replies = replies;

        ViewBag.IsDetailsView = true;
        ViewBag.Title = "Notification Details";
        return View(notification);
    }

    // GET: Notification/GetNotification (AJAX)
    [HttpGet]
    public async Task<IActionResult> GetNotification(long id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, message = "User not found" });
        }

        var roles = await _userManager.GetRolesAsync(currentUser);
        bool isSuperAdmin = roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase);

        if (!isSuperAdmin)
        {
            return Json(new { success = false, message = "Access denied. Only SuperAdmin can view notifications." });
        }

        var notification = await _notificationService.GetNotificationByIdAsync(id);
        if (notification == null)
        {
            return Json(new { success = false, message = "Notification not found" });
        }

        // Get only active users for the UI
        var userIds = notification.NotificationUsers
            .Where(nu => nu.DeletedDate == null)
            .Select(nu => nu.UserId)
            .ToList();
        
        return Json(new
        {
            success = true,
            notification = new
            {
                notification.NotificationId,
                notification.Title,
                notification.Message,
                notification.Priority,
                ExpiryDate = notification.ExpiryDate?.ToString("yyyy-MM-ddTHH:mm"),
                UserIds = userIds
            }
        });
    }

    // POST: Notification/Update
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Update([FromBody] UpdateNotificationModel model)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, message = "User not found" });
        }

        var roles = await _userManager.GetRolesAsync(currentUser);
        bool isSuperAdmin = roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase);

        if (!isSuperAdmin)
        {
            return Json(new { success = false, message = "Access denied. Only SuperAdmin can update notifications." });
        }

        if (string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.Message) || string.IsNullOrWhiteSpace(model.Priority))
        {
            return Json(new { success = false, message = "Title, Message, and Priority are required." });
        }

        var existingNotification = await _notificationService.GetNotificationByIdAsync(model.NotificationId);
        if (existingNotification == null)
        {
            return Json(new { success = false, message = "Notification not found" });
        }

        DateTimeOffset? expiryDate = null;
        if (!string.IsNullOrEmpty(model.ExpiryDate))
        {
            if (DateTimeOffset.TryParse(model.ExpiryDate, out var parsedDate))
            {
                expiryDate = parsedDate;
            }
        }

        if (model.UserIds == null || model.UserIds.Count == 0)
        {
            return Json(new { success = false, message = "Please select at least one user." });
        }

        existingNotification.Title = model.Title;
        existingNotification.Message = model.Message;
        existingNotification.Priority = model.Priority;
        existingNotification.ExpiryDate = expiryDate;

        var response = await _notificationService.UpdateNotificationAsync(existingNotification, model.UserIds);

        if (response.Success)
        {
            return Json(new { success = true, message = "Notification updated successfully!" });
        }

        return Json(new { success = false, message = response.Message });
    }

    // POST: Notification/Delete
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, message = "User not found" });
        }

        var roles = await _userManager.GetRolesAsync(currentUser);
        bool isSuperAdmin = roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase);

        if (!isSuperAdmin)
        {
            return Json(new { success = false, message = "Access denied. Only SuperAdmin can delete notifications." });
        }

        var response = await _notificationService.DeleteNotificationAsync(id);

        if (response.Success)
        {
            return Json(new { success = true, message = "Notification deleted successfully!" });
        }

        return Json(new { success = false, message = response.Message });
    }

    public class CreateNotificationModel
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string? ExpiryDate { get; set; }
        public List<long> UserIds { get; set; } = new List<long>();
    }

    public class UpdateNotificationModel
    {
        public long NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string? ExpiryDate { get; set; }
        public List<long> UserIds { get; set; } = new List<long>();
    }
}

