using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TpaSodManagement.ViewModels.User
{
    public class UserEditViewModel
    {
        public long Id { get; set; }

        [Display(Name = "User Name")]
        public string? UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required]
        [Display(Name = "Organization")]
        public long? OrganizationId { get; set; }

        [Display(Name = "Primary Contact")]
        public string? PrimaryContact { get; set; }

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Display(Name = "Address Type")]
        public int? AddressTypeId { get; set; }

        [Display(Name = "State/Province")]
        public int? StateProvinceId { get; set; }

        public string? StateProvinceName { get; set; }

        [Display(Name = "Address Line 1")]
        public string? AddressLine1 { get; set; }

        [Display(Name = "City")]
        public string? City { get; set; }

        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }

        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }

        public IEnumerable<SelectListItem> Organizations { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> AddressTypes { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> StateProvinces { get; set; } = Enumerable.Empty<SelectListItem>();

        public bool IsDetailsView { get; set; }
    }
}

