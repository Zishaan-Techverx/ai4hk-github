using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Database.Entities;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<TpaSodManagementUser> _userManager;
    private readonly SignInManager<TpaSodManagementUser> _signInManager;
    private readonly IHomeService _homeService;

    public HomeController(
        ILogger<HomeController> logger,
        UserManager<TpaSodManagementUser> userManager,
        SignInManager<TpaSodManagementUser> signInManager,
        IHomeService homeService)
    {
        _logger = logger;
        _userManager = userManager; 
        _signInManager = signInManager;
        _homeService = homeService;
    }

    public async Task<IActionResult> Index()
    {
        var vm = await _homeService.GetHomeIndexViewModelAsync(User.Identity?.IsAuthenticated ?? false);

        // Preserve existing ViewData/ViewBag usage to avoid view changes
        ViewData["Organizations"] = vm.Organizations;

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
    public async Task<IActionResult> Verify2FA(string code)
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

        // Strip spaces and hyphens
        var verificationCode = code?.Replace(" ", string.Empty).Replace("-", string.Empty) ?? string.Empty;

        if (string.IsNullOrEmpty(verificationCode))
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "Please enter a verification code." });
            }
            return Json(new { success = false, message = "Please enter a verification code." });
        }

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

        if (!isValid)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "Invalid verification code." });
            }
            return Json(new { success = false, message = "Invalid verification code." });
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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}