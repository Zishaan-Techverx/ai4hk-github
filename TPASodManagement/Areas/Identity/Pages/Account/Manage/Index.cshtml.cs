using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TpaSodManagement.Areas.Identity.Data;

namespace TpaSodManagement.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<TpaSodManagementUser> _userManager;
        private readonly SignInManager<TpaSodManagementUser> _signInManager;

        public IndexModel(
            UserManager<TpaSodManagementUser> userManager,
            SignInManager<TpaSodManagementUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }
        public string PhoneNumber { get; set; }
        public bool HasPassword { get; set; }
        public bool TwoFactorEnabled { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [System.ComponentModel.DataAnnotations.Phone]
            [System.ComponentModel.DataAnnotations.Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            Username = user.UserName;
            PhoneNumber = user.PhoneNumber;
            HasPassword = await _userManager.HasPasswordAsync(user);
            TwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);

            Input = new InputModel
            {
                PhoneNumber = PhoneNumber
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (Input.PhoneNumber != user.PhoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}

