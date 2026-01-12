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
        var result = await _signInManager.PasswordSignInAsync(Input_Email, Input_Password, Input_RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            TempData["ShowWelcomePopup"] = true;
            return RedirectToAction("Index");
        }

        if (!result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(Input_Email);

            if (user != null && !string.IsNullOrEmpty(user.UserName))
            {
                result = await _signInManager.PasswordSignInAsync(user.UserName, Input_Password, Input_RememberMe, lockoutOnFailure: false);
            }
        }

        if (result.Succeeded)
        {
            TempData["ShowWelcomePopup"] = true;
            return RedirectToAction("Index");
        }

        if (result.IsNotAllowed)
        {
            TempData["ErrorMessage"] = "Login failed: Your account is not allowed to sign in (e.g., email not confirmed).";
        }
        else if (result.IsLockedOut)
        {
            TempData["ErrorMessage"] = "Login failed: This account is locked out.";
        }
        else 
        {
            TempData["ErrorMessage"] = "Invalid email/username or password!";
        }

        return RedirectToAction("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}