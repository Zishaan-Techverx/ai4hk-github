using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Models;
using TpaSodManagement.Models.Db;

namespace TsaSodManagement.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly SodDbContext _context;
    private readonly UserManager<TpaSodManagementUser> _userManager;
    private readonly SignInManager<TpaSodManagementUser> _signInManager;

    public HomeController(
        ILogger<HomeController> logger,
        SodDbContext context,
        UserManager<TpaSodManagementUser> userManager,
        SignInManager<TpaSodManagementUser> signInManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager; 
        _signInManager = signInManager;
    }

    public async Task<IActionResult> Index()
    {
        // Load ALL organizations for _AuthPartial dropdown (no IsActive filter)
        var organizations = await _context.Organizations
            .OrderBy(o => o.OrganizationName)
            .ToListAsync();
        
        ViewData["Organizations"] = organizations;

        if (User.Identity.IsAuthenticated)
        {
            ViewBag.FarmCount = await _context.Farms.CountAsync();
            ViewBag.CustomerCount = await _context.Customers.CountAsync();
            ViewBag.ProductCount = await _context.Products.CountAsync();
            ViewBag.SaleCount = await _context.Sales.CountAsync();

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

            if (user != null)
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