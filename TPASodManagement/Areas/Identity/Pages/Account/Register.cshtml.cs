using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering; // ADD THIS for SelectList
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Models.Db; 

namespace TpaSodManagement.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<TpaSodManagementUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly SodDbContext _context;
        private readonly SignInManager<TpaSodManagementUser> _signInManager; 

        public RegisterModel(
            UserManager<TpaSodManagementUser> userManager,
            ILogger<RegisterModel> logger,
            SodDbContext context, 
            SignInManager<TpaSodManagementUser> signInManager) 
        {
            _userManager = userManager;
            _logger = logger;
            _context = context;
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
            var organizations = await _context.Organizations
                .OrderBy(o => o.OrganizationName)
                .ToListAsync();
            
            ViewData["Organizations"] = new SelectList(organizations, "OrganizationName", "OrganizationName");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                // Validate Organization exists
                var organization = await _context.Organizations
                    .FirstOrDefaultAsync(o => o.OrganizationName.ToUpper() == Input.OrganizationName.ToUpper());

                if (organization == null)
                {
                    ModelState.AddModelError("Input.OrganizationName", "Organization does not exist. Please enter a valid organization name.");
                    // Reload organizations
                    var organizations = await _context.Organizations
                        .OrderBy(o => o.OrganizationName)
                        .ToListAsync();
                    ViewData["Organizations"] = new SelectList(organizations, "OrganizationName", "OrganizationName");
                    return Page();
                }

                // Early Email Duplicate Check
                var existingUserByEmail = await _userManager.FindByEmailAsync(Input.Email);
                if (existingUserByEmail != null)
                {
                    ModelState.AddModelError("Input.Email", $"An account with the email '{Input.Email}' already exists. Please use a different email address.");
                    // Reload organizations
                    var organizations = await _context.Organizations
                        .OrderBy(o => o.OrganizationName)
                        .ToListAsync();
                    ViewData["Organizations"] = new SelectList(organizations, "OrganizationName", "OrganizationName");
                    return Page();
                }

                // Generate username (only if email is unique)
                string rawOrgName = organization.OrganizationName.Replace(" ", "");
                const int OrgPrefixLength = 5;
                string orgPrefix = rawOrgName.Length >= OrgPrefixLength
                                   ? rawOrgName.Substring(0, OrgPrefixLength)
                                   : rawOrgName;

                orgPrefix = orgPrefix.ToUpper();

                string userInitials;
                string[] nameParts = Input.FirstName.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

                if (nameParts.Length >= 2)
                {
                    userInitials = (nameParts[0][0].ToString() + nameParts[^1][0].ToString()).ToUpper();
                }
                else if (Input.FirstName.Length >= 2)
                {
                    userInitials = Input.FirstName.Substring(0, 2).ToUpper();
                }
                else
                {
                    userInitials = Input.FirstName.ToUpper();
                }

                string baseUsername = $"{orgPrefix}-{userInitials}";
                string finalUsername = baseUsername;
                int counter = 1;

                while (await _userManager.FindByNameAsync(finalUsername) != null)
                {
                    finalUsername = $"{baseUsername}{counter++}";
                }

                var user = new TpaSodManagementUser
                {
                    UserName = finalUsername,
                    Email = Input.Email,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    OrganizationName = organization.OrganizationName
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Account created successfully for {Email} with username {Username}", Input.Email, finalUsername);

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
            var orgs = await _context.Organizations
                .OrderBy(o => o.OrganizationName)
                .ToListAsync();
            ViewData["Organizations"] = new SelectList(orgs, "OrganizationName", "OrganizationName", Input?.OrganizationName);
            
            return Page();
        }
    }
}