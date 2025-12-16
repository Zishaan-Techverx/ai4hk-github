using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;
using System;
using Microsoft.AspNetCore.Identity;
using TpaSodManagement.Areas.Identity.Data;

namespace TpaSodManagement.Controllers
{
    [Authorize]
    public class SaleController : Controller
    {
        private readonly ISaleService _saleService;
        private readonly UserManager<TpaSodManagementUser> _userManager;
        private readonly ILogger<SaleController> _logger; 

        public SaleController(
            ISaleService saleService,
            UserManager<TpaSodManagementUser> userManager,
            ILogger<SaleController> logger)
        {
            _saleService = saleService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _saleService.GetAllAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(new List<Sale>());
            }
            return View(result.Data);
        }

        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var result = await _saleService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var dropdowns = await _saleService.GetDropdownDataAsync();
            if (dropdowns.Success && dropdowns.Data != null)
            {
                foreach (var kvp in dropdowns.Data)
                    ViewData[kvp.Key] = kvp.Value;
            }

            ViewBag.IsDetailsView = true;
            ViewBag.Title = "Sale Details";
            return View("Edit", result.Data);
        }

        public async Task<IActionResult> Create()
        {
            var dropdowns = await _saleService.GetDropdownDataAsync();
            if (dropdowns.Success && dropdowns.Data != null)
            {
                foreach (var kvp in dropdowns.Data)
                    ViewData[kvp.Key] = kvp.Value;
            }
            else
            {
                TempData["Error"] = dropdowns.Message;
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Sale sale)
        {
            // Set automatic fields
            sale.CreatedDate = DateTimeOffset.UtcNow;
            sale.UpdatedDate = DateTimeOffset.UtcNow;
            
            // Set UpdatedByUserId to current logged-in user or UserId
            if (sale.UpdatedByUserId == 0)
            {
                if (sale.UserId > 0)
                {
                    sale.UpdatedByUserId = sale.UserId;
                }
                else
                {
                    // Get current logged in user
                    var currentUser = await _userManager.GetUserAsync(HttpContext.User);
                    if (currentUser != null)
                    {
                        sale.UpdatedByUserId = currentUser.Id;
                        sale.UserId = currentUser.Id; // Also set UserId if not set
                    }
                }
            }
            
            // Remove all ModelState errors - validations removed (same as ProductController)
            ModelState.Clear();
            
            // Validations removed - directly save
            var result = await _saleService.CreateAsync(sale);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await Create(); // Reload dropdowns
                return View(sale);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var result = await _saleService.GetByIdAsync(id.Value);
            if (!result.Success || result.Data == null) return NotFound();

            var dropdowns = await _saleService.GetDropdownDataAsync();
            if (dropdowns.Success && dropdowns.Data != null)
            {
                foreach (var kvp in dropdowns.Data)
                    ViewData[kvp.Key] = kvp.Value;
            }

            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, Sale sale)
        {
            if (id != sale.SaleId) return NotFound();
            if (!ModelState.IsValid)
            {
                await Edit(id); // reload dropdowns
                return View(sale);
            }

            var result = await _saleService.UpdateAsync(sale);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await Edit(id); // reload dropdowns
                return View(sale);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _saleService.DeleteAsync(id);
            if (!result.Success)
            {
                return Json(new { success = false, message = result.Message });
            }
            return Json(new { success = true, message = "Sale deleted successfully." });
        }
    }
}
