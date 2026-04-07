using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Database.Entities;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.Database;
using Microsoft.EntityFrameworkCore;

namespace TpaSodManagement.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<TpaSodManagementUser> _userManager;
    private readonly SignInManager<TpaSodManagementUser> _signInManager;
    private readonly IHomeService _homeService;
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly INotificationResponseService _notificationResponseService;

    public HomeController(
        ILogger<HomeController> logger,
        UserManager<TpaSodManagementUser> userManager,
        SignInManager<TpaSodManagementUser> signInManager,
        IHomeService homeService,
        ApplicationDbContext context,
        INotificationService notificationService,
        INotificationResponseService notificationResponseService)
    {
        _logger = logger;
        _userManager = userManager; 
        _signInManager = signInManager;
        _homeService = homeService;
        _context = context;
        _notificationService = notificationService;
        _notificationResponseService = notificationResponseService;
    }

    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
    {
        var vm = await _homeService.GetHomeIndexViewModelAsync(User.Identity?.IsAuthenticated ?? false);

        // Preserve existing ViewData/ViewBag usage to avoid view changes
        ViewData["Farms"] = vm.Farms;

        if (vm.IsAuthenticated)
        {
            ViewBag.FarmCount = vm.FarmCount;
            ViewBag.CustomerCount = vm.CustomerCount;
            ViewBag.ProductCount = vm.ProductCount;
            ViewBag.SaleCount = vm.SaleCount;

            if (TempData.ContainsKey("ShowWelcomePopup"))
            {
                ViewData["ShowWelcomePopup"] = TempData["ShowWelcomePopup"];
            }

            // Load notifications
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                var roles = await _userManager.GetRolesAsync(currentUser);
                bool isSuperAdmin = roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase);
                
                if (isSuperAdmin)
                {
                    // Set filter columns for the partial view
                    ViewBag.FilterColumns = new Dictionary<string, string>
                    {
                        { "Search", "Search (Title/Message)" },
                        { "CreatedDate", "Created Date" },
                        { "ExpiryDate", "Expiry Date" }
                    };
                    ViewBag.ModuleName = "Notifications";
                    ViewBag.DateColumns = new HashSet<string> { "CreatedDate", "ExpiryDate" };

                    // Load all notifications for SuperAdmin (management view)
                    var allNotifications = await _notificationService.GetAllNotificationsAsync();
                    
                    // Get all notification IDs
                    var notificationIds = allNotifications.Select(n => n.NotificationId).ToList();
                    
                    // Get notifications that have replies
                    var notificationsWithReplies = await _context.NotificationResponses
                        .Where(nr => notificationIds.Contains(nr.NotificationId) && nr.DeletedDate == null)
                        .Select(nr => nr.NotificationId)
                        .Distinct()
                        .ToListAsync();
                    
                    ViewBag.NotificationsWithReplies = notificationsWithReplies;
                    // Note: Count will be calculated on client-side based on sessionStorage (unviewed replies only)
                    ViewBag.NotificationsWithRepliesCount = notificationsWithReplies.Count;
                    
                    // Apply pagination for SuperAdmin
                    var totalCount = allNotifications.Count;
                    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                    var paginatedNotifications = allNotifications
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
                    
                    ViewBag.Notifications = paginatedNotifications;
                    ViewBag.PageNumber = pageNumber;
                    ViewBag.TotalPages = totalPages;
                    ViewBag.TotalCount = totalCount;
                    ViewBag.PageSize = pageSize;
                }
                else
                {
                    // Load user-specific notifications with read status for regular users
                    var allUserNotificationUsers = await _context.NotificationUsers
                        .Where(nu => nu.UserId == currentUser.Id && nu.DeletedDate == null)
                        .Include(nu => nu.Notification)
                        .Where(nu => nu.Notification.DeletedDate == null && nu.Notification.IsActive)
                        .OrderByDescending(nu => nu.Notification.CreatedDate)
                        .ToListAsync();
                    
                    // Count unread notifications (before pagination)
                    var unreadCount = allUserNotificationUsers.Count(nu => !nu.IsRead);
                    ViewBag.UnreadNotificationCount = unreadCount;
                    
                    // Check which notifications the user has already replied to
                    var notificationIds = allUserNotificationUsers.Select(nu => nu.NotificationId).ToList();
                    var userReplies = await _context.NotificationResponses
                        .Where(nr => nr.UserId == currentUser.Id 
                            && notificationIds.Contains(nr.NotificationId) 
                            && nr.DeletedDate == null)
                        .Select(nr => nr.NotificationId)
                        .ToListAsync();
                    
                    ViewBag.UserRepliedNotificationIds = userReplies;
                    
                    // Apply pagination for user notifications
                    var totalCount = allUserNotificationUsers.Count;
                    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                    var paginatedUserNotificationUsers = allUserNotificationUsers
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
                    
                    ViewBag.UserNotificationUsers = paginatedUserNotificationUsers;
                    ViewBag.PageNumber = pageNumber;
                    ViewBag.TotalPages = totalPages;
                    ViewBag.TotalCount = totalCount;
                    ViewBag.PageSize = pageSize;
                }
            }
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string Input_Email, string Input_Password, bool Input_RememberMe = false)
    {
        // Try to find user by email first
        var user = await _userManager.FindByEmailAsync(Input_Email);
        
        // If not found by email, try by username
        if (user == null)
        {
            user = await _userManager.FindByNameAsync(Input_Email);
        }

        if (user == null)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "Invalid email/username or password!" });
            }
            TempData["ErrorMessage"] = "Invalid email/username or password!";
            return RedirectToAction("Index");
        }

        // Try password sign in
        if (string.IsNullOrEmpty(user.UserName))
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "Invalid email/username or password!" });
            }
            TempData["ErrorMessage"] = "Invalid email/username or password!";
            return RedirectToAction("Index");
        }

        var result = await _signInManager.PasswordSignInAsync(user.UserName, Input_Password, Input_RememberMe, lockoutOnFailure: false);

        // Check if 2FA is required (this happens when password is correct but 2FA is enabled)
        if (result.RequiresTwoFactor)
        {
            // Store user info in session for 2FA verification
            HttpContext.Session.SetString("2FA_UserId", user.Id.ToString());
            HttpContext.Session.SetString("2FA_RememberMe", Input_RememberMe.ToString());
            
            // Note: PasswordSignInAsync with RequiresTwoFactor result automatically sets up
            // the two-factor authentication context needed for recovery codes
            // We don't need to sign in/out here - the 2FA context is already established
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, requires2FA = true, message = "Please enter your 2FA code." });
            }
            
            // For non-AJAX requests, redirect to 2FA page
            return RedirectToAction("Verify2FA");
        }

        if (result.Succeeded)
        {
            // No 2FA, proceed with normal login
            TempData["ShowWelcomePopup"] = true;
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, requires2FA = false, redirectUrl = Url.Action("Index") });
            }
            
            return RedirectToAction("Index");
        }

        if (result.IsNotAllowed)
        {
            var errorMsg = "Login failed: Your account is not allowed to sign in (e.g., email not confirmed).";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = errorMsg });
            }
            TempData["ErrorMessage"] = errorMsg;
        }
        else if (result.IsLockedOut)
        {
            var errorMsg = "Login failed: This account is locked out.";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = errorMsg });
            }
            TempData["ErrorMessage"] = errorMsg;
        }
        else 
        {
            var errorMsg = "Invalid email/username or password!";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = errorMsg });
            }
            TempData["ErrorMessage"] = errorMsg;
        }

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { success = false, message = TempData["ErrorMessage"]?.ToString() ?? "Login failed." });
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Verify2FA(string code, bool isRecoveryCode = false)
    {
        var userId = HttpContext.Session.GetString("2FA_UserId");
        var rememberMeStr = HttpContext.Session.GetString("2FA_RememberMe");
        bool rememberMe = bool.TryParse(rememberMeStr, out var rm) && rm;

        if (string.IsNullOrEmpty(userId))
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "Session expired. Please login again." });
            }
            return RedirectToAction("Index");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "User not found." });
            }
            return RedirectToAction("Index");
        }

        if (string.IsNullOrEmpty(code))
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "Please enter a verification code." });
            }
            return Json(new { success = false, message = "Please enter a verification code." });
        }

        bool isValid = false;

        if (isRecoveryCode)
        {
            // Check if user has recovery codes
            var recoveryCodesCount = await _userManager.CountRecoveryCodesAsync(user);
            if (recoveryCodesCount == 0)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "No recovery codes available. Please generate new recovery codes." });
                }
                return Json(new { success = false, message = "No recovery codes available. Please generate new recovery codes." });
            }

            var trimmedCode = code.Trim();
            
            // Try different normalization approaches
            // Approach 1: Remove spaces and dashes, convert to uppercase (most common)
            var normalizedCode1 = trimmedCode
                .Replace(" ", string.Empty)
                .Replace("-", string.Empty)
                .ToUpperInvariant();
            
            // Approach 2: Remove spaces and dashes, keep original case
            var normalizedCode2 = trimmedCode
                .Replace(" ", string.Empty)
                .Replace("-", string.Empty);
            
            // Approach 3: Keep original format (with spaces/dashes)
            var normalizedCode3 = trimmedCode;
            
            // Try Approach 1 first (uppercase, no formatting)
            var recoveryCodeResult = await _userManager.RedeemTwoFactorRecoveryCodeAsync(user, normalizedCode1);
            isValid = recoveryCodeResult.Succeeded;
            
            // Try Approach 2 if Approach 1 failed
            if (!isValid)
            {
                recoveryCodeResult = await _userManager.RedeemTwoFactorRecoveryCodeAsync(user, normalizedCode2);
                isValid = recoveryCodeResult.Succeeded;
            }
            
            // Try Approach 3 if Approach 2 failed
            if (!isValid)
            {
                recoveryCodeResult = await _userManager.RedeemTwoFactorRecoveryCodeAsync(user, normalizedCode3);
                isValid = recoveryCodeResult.Succeeded;
            }
            
            // Debug: Check actual stored recovery codes in database
            if (!isValid)
            {
                try
                {
                    // Check AspNetUserTokens table for recovery codes
                    var recoveryToken = await _context.UserTokens
                        .FirstOrDefaultAsync(t => t.UserId == user.Id && 
                                                  t.LoginProvider == "[AspNetUserStore]" && 
                                                  t.Name == "RecoveryCodes");
                    
                    if (recoveryToken != null)
                    {
                        _logger.LogWarning("Recovery code verification failed for user {UserId}. " +
                            "Input: '{Input}', Normalized1: '{Norm1}', Normalized2: '{Norm2}', Normalized3: '{Norm3}', " +
                            "Remaining codes: {Count}, Stored token exists: {HasToken}",
                            user.Id, code, normalizedCode1, normalizedCode2, normalizedCode3, recoveryCodesCount, recoveryToken != null);
                    }
                    else
                    {
                        _logger.LogWarning("Recovery code verification failed for user {UserId}. " +
                            "No recovery codes token found in database for user. " +
                            "Input: '{Input}', Normalized1: '{Norm1}', Normalized2: '{Norm2}', Normalized3: '{Norm3}', " +
                            "Remaining codes: {Count}",
                            user.Id, code, normalizedCode1, normalizedCode2, normalizedCode3, recoveryCodesCount);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking recovery codes in database for user {UserId}", user.Id);
                }
            }
            
            if (!isValid)
            {
                var errorMessage = "Invalid recovery code. ";
                if (recoveryCodesCount > 0)
                {
                    errorMessage += $"You have {recoveryCodesCount} recovery code(s) remaining. Make sure you're using a valid, unused recovery code from your list.";
                }
                else
                {
                    errorMessage += "Please generate new recovery codes.";
                }
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage });
                }
                return Json(new { success = false, message = errorMessage });
            }
            
            // Recovery code was successfully redeemed, now sign in the user
            // Clear session
            HttpContext.Session.Remove("2FA_UserId");
            HttpContext.Session.Remove("2FA_RememberMe");
            
            // Sign in the user
            await _signInManager.SignInAsync(user, rememberMe);
            TempData["ShowWelcomePopup"] = true;
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, redirectUrl = Url.Action("Index") });
            }
            return RedirectToAction("Index");
        }
        else
        {
            // Verify 2FA token - strip spaces and hyphens for 2FA codes
            var verificationCode = code.Replace(" ", string.Empty).Replace("-", string.Empty);
            isValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!isValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Invalid verification code." });
                }
                return Json(new { success = false, message = "Invalid verification code." });
            }
        }

        // Clear session
        HttpContext.Session.Remove("2FA_UserId");
        HttpContext.Session.Remove("2FA_RememberMe");

        // Sign in the user
        await _signInManager.SignInAsync(user, rememberMe);
        TempData["ShowWelcomePopup"] = true;

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { success = true, redirectUrl = Url.Action("Index") });
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> MarkNotificationAsRead(long notificationId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, message = "User not found" });
        }

        var result = await _notificationService.MarkAsReadAsync(notificationId, currentUser.Id);
        if (result)
        {
            return Json(new { success = true, message = "Notification marked as read" });
        }

        return Json(new { success = false, message = "Failed to mark notification as read" });
    }

    [HttpPost]
    public async Task<IActionResult> SendNotificationReply(long notificationId, string reply)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, message = "User not found" });
        }

        if (string.IsNullOrWhiteSpace(reply))
        {
            return Json(new { success = false, message = "Reply cannot be empty." });
        }

        var response = await _notificationResponseService.CreateResponseAsync(notificationId, currentUser.Id, reply);
        
        if (response.Success)
        {
            return Json(new { success = true, message = response.Message });
        }

        return Json(new { success = false, message = response.Message });
    }

    [HttpGet]
    public async Task<IActionResult> GetUserReplyForNotification(long notificationId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new { success = false, message = "User not found" });
        }

        var replies = await _notificationResponseService.GetResponsesByNotificationIdAsync(notificationId);
        var userReply = replies.FirstOrDefault(r => r.UserId == currentUser.Id && r.DeletedDate == null);

        if (userReply != null)
        {
            return Json(new { 
                success = true, 
                hasReply = true,
                reply = userReply.Reply,
                replyDate = userReply.CreatedDate.ToString("MM/dd/yyyy h:mm tt")
            });
        }

        return Json(new { 
            success = true, 
            hasReply = false 
        });
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> FilterNotifications([FromBody] Dictionary<string, string> filters)
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            var roles = await _userManager.GetRolesAsync(currentUser);
            bool isSuperAdmin = roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase);

            if (isSuperAdmin)
            {
                // SuperAdmin: Filter all notifications
                var result = await _notificationService.GetFilteredAsync(filters ?? new Dictionary<string, string>());
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                // Get notifications that have replies
                var notificationIds = result.Data?.Select(n => n.NotificationId).ToList() ?? new List<long>();
                var notificationsWithReplies = await _context.NotificationResponses
                    .Where(nr => notificationIds.Contains(nr.NotificationId) && nr.DeletedDate == null)
                    .Select(nr => nr.NotificationId)
                    .Distinct()
                    .ToListAsync();

                var notifications = result.Data ?? new List<Notification>();
                var notificationsList = notifications.Select(n => new
                {
                    n.NotificationId,
                    n.Title,
                    Message = !string.IsNullOrEmpty(n.Message) && n.Message.Length > 100 ? n.Message.Substring(0, 100) + "..." : n.Message ?? "",
                    n.Priority,
                    RecipientsCount = n.NotificationUsers?.Count ?? 0,
                    CreatedDate = n.CreatedDate.ToString("MM/dd/yyyy h:mm tt"),
                    ExpiryDate = n.ExpiryDate?.ToString("MM/dd/yyyy h:mm tt") ?? "Not Set",
                    HasReply = notificationsWithReplies.Contains(n.NotificationId)
                }).ToList();

                return Json(new { success = true, data = notificationsList, replies = notificationsWithReplies });
            }
            else
            {
                // User: Filter user notifications
                var result = await _notificationService.GetFilteredUserNotificationsAsync(currentUser.Id, filters ?? new Dictionary<string, string>());
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var notificationUsers = result.Data ?? new List<NotificationUser>();
                var unreadCount = notificationUsers.Count(nu => !nu.IsRead);

                // Check which notifications the user has already replied to
                var notificationIds = notificationUsers.Select(nu => nu.NotificationId).ToList();
                var userReplies = await _context.NotificationResponses
                    .Where(nr => nr.UserId == currentUser.Id 
                        && notificationIds.Contains(nr.NotificationId) 
                        && nr.DeletedDate == null)
                    .Select(nr => nr.NotificationId)
                    .ToListAsync();

                var notificationsList = notificationUsers.Select(nu => new
                {
                    nu.NotificationId,
                    nu.Notification.Title,
                    Message = !string.IsNullOrEmpty(nu.Notification.Message) && nu.Notification.Message.Length > 100 ? nu.Notification.Message.Substring(0, 100) + "..." : nu.Notification.Message ?? "",
                    nu.Notification.Priority,
                    CreatedDate = nu.Notification.CreatedDate.ToString("MM/dd/yyyy h:mm tt"),
                    ExpiryDate = nu.Notification.ExpiryDate?.ToString("MM/dd/yyyy h:mm tt") ?? "Not Set",
                    IsRead = nu.IsRead,
                    ReadDate = nu.ReadDate?.ToString("MM/dd/yyyy h:mm tt"),
                    HasReplied = userReplies.Contains(nu.NotificationId)
                }).ToList();

                return Json(new { success = true, data = notificationsList, unreadCount = unreadCount, repliedIds = userReplies });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering notifications");
            return Json(new { success = false, message = "An error occurred while filtering notifications." });
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}