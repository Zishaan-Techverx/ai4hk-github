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
            [Display(Name = "Organization Name")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
            public string OrganizationName { get; set; }

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
            
            // Load ALL organizations for dropdown (no IsActive filter)
            var organizations = await _registrationService.GetAllOrganizationsAsync();
            
            ViewData["Organizations"] = new SelectList(organizations, "OrganizationName", "OrganizationName");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                // Validate Organization exists
                var organization = await _registrationService.GetOrganizationByNameAsync(Input.OrganizationName);

                if (organization == null)
                {
                    ModelState.AddModelError("Input.OrganizationName", "Organization does not exist. Please enter a valid organization name.");
                    
                    var organizations = await _registrationService.GetAllOrganizationsAsync();
                    ViewData["Organizations"] = new SelectList(organizations, "OrganizationName", "OrganizationName");
                    return Page();
                }

                // Early Email Duplicate Check
                if (await _registrationService.IsEmailExistsAsync(Input.Email))
                {
                    ModelState.AddModelError("Input.Email", $"An account with the email '{Input.Email}' already exists. Please use a different email address.");
                    
                    var organizations = await _registrationService.GetAllOrganizationsAsync();
                    ViewData["Organizations"] = new SelectList(organizations, "OrganizationName", "OrganizationName");
                    return Page();
                }

                // Generate username
                string finalUsername = await _registrationService.GenerateUsernameAsync(organization.OrganizationName, Input.FirstName);

                var user = new TpaSodManagementUser
                {
                    UserName = finalUsername,
                    Email = Input.Email,
                    OrganizationName = organization.OrganizationName,
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
                        
                        // Create or Get Farm for Organization
                        var farm = await _registrationService.CreateOrGetFarmForOrganizationAsync(organization.OrganizationId);
                        user.FarmId = farm.FarmId;
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

            // Reload organizations if validation fails
            var orgs = await _registrationService.GetAllOrganizationsAsync();
            ViewData["Organizations"] = new SelectList(orgs, "OrganizationName", "OrganizationName", Input?.OrganizationName);
            
            return Page();
        }
    }
}