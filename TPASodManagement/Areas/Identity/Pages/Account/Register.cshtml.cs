using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Services.Interfaces; 

namespace TpaSodManagement.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<TpaSodManagementUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IRegistrationService _registrationService;
        private readonly SignInManager<TpaSodManagementUser> _signInManager; 

        public RegisterModel(
            UserManager<TpaSodManagementUser> userManager,
            ILogger<RegisterModel> logger,
            IRegistrationService registrationService, 
            SignInManager<TpaSodManagementUser> signInManager) 
        {
            _userManager = userManager;
            _logger = logger;
            _registrationService = registrationService;
            _signInManager = signInManager; 
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "FirstName")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "LastName")]
            public string LastName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [Display(Name = "Farm")]
            public long FarmId { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            
            var farms = await _registrationService.GetAllFarmsAsync();
            ViewData["Farms"] = new SelectList(farms, "FarmId", "FarmName");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var farms = await _registrationService.GetAllFarmsAsync();
                var farm = farms.FirstOrDefault(f => f.FarmId == Input.FarmId);
                if (farm == null)
                {
                    ModelState.AddModelError("Input.FarmId", "Farm does not exist. Please select a valid farm.");
                    ViewData["Farms"] = new SelectList(farms, "FarmId", "FarmName", Input.FarmId);
                    return Page();
                }

                // Early Email Duplicate Check
                if (await _registrationService.IsEmailExistsAsync(Input.Email))
                {
                    ModelState.AddModelError("Input.Email", $"An account with the email '{Input.Email}' already exists. Please use a different email address.");
                    
                    ViewData["Farms"] = new SelectList(farms, "FarmId", "FarmName", Input.FarmId);
                    return Page();
                }

                string finalUsername = await _registrationService.GenerateUsernameFromFarmAsync(farm, Input.FirstName);

                var user = new TpaSodManagementUser
                {
                    UserName = finalUsername,
                    Email = Input.Email,
                    FarmId = farm.FarmId,
                    OrganizationId = farm.OrganizationId,
                    IsActive = true, 
                    PhoneNumber = string.Empty,
                    EmailConfirmed = false,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = false,
                    AccessFailedCount = 0,
                };

                var result = await _registrationService.CreateUserAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Account created successfully for {Email} with username {Username}", Input.Email, finalUsername);

                    // Create related records
                    try
                    {
                        // Create Person record with FirstName and LastName from Input
                        await _registrationService.CreatePersonForUserAsync(user, Input.FirstName, Input.LastName);
                        
                        // Create Address record (currently empty, but you can add address fields to InputModel if needed)
                        await _registrationService.CreateAddressForUserAsync(user);
                        
                        // Create Website record
                        await _registrationService.CreateWebsiteForUserAsync(user, finalUsername);
                        
                        await _userManager.UpdateAsync(user);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error creating related records for user {Email}", Input.Email);
                    }

                    TempData["SuccessMessage"] = $"Registration successful! Your username is '{finalUsername}'. Please login to continue.";
                    return RedirectToPage("AuthPartial");
                }

                // Handle other errors from Identity framework (backup - in case early check missed something)
                foreach (var error in result.Errors)
                {
                    if (error.Code == "DuplicateUserName")
                    {
                        ModelState.AddModelError("Input.Email", $"A user with the generated username '{finalUsername}' already exists.");
                    }
                    else if (error.Code == "DuplicateEmail")
                    {
                        ModelState.AddModelError("Input.Email", $"Account already exists for '{Input.Email}' email.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            var fallbackFarms = await _registrationService.GetAllFarmsAsync();
            ViewData["Farms"] = new SelectList(fallbackFarms, "FarmId", "FarmName", Input?.FarmId);
            
            return Page();
        }
    }
}