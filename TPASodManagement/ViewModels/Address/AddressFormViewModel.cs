using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TpaSodManagement.ViewModels.Address
{
    /// <summary>
    /// ViewModel for Address step (step 2) when creating/editing Customer, Farm, or Organization.
    /// </summary>
    public class AddressFormViewModel
    {
        public long? AddressId { get; set; }

        /// <summary>Parent entity name: Customer, Farm, or Organization.</summary>
        public string ParentEntityName { get; set; } = "";

        /// <summary>Parent entity id (CustomerId, FarmId, or OrganizationId).</summary>
        public long ParentEntityId { get; set; }

        [Required(ErrorMessage = "Address Type is required")]
        [Display(Name = "Address Type")]
        public int AddressTypeId { get; set; }

        [Required(ErrorMessage = "Address Line 1 is required")]
        [StringLength(200)]
        [Display(Name = "Address Line 1")]
        public string AddressLine1 { get; set; } = "";

        [StringLength(200)]
        [Display(Name = "Address Line 2")]
        public string? AddressLine2 { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(100)]
        [Display(Name = "City")]
        public string City { get; set; } = "";

        [Display(Name = "State / Province")]
        public int? StateProvinceId { get; set; }

        [Required(ErrorMessage = "Postal Code is required")]
        [StringLength(20)]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; } = "";

        [Display(Name = "Latitude")]
        public decimal? Latitude { get; set; }

        [Display(Name = "Longitude")]
        public decimal? Longitude { get; set; }

        [Display(Name = "Primary")]
        public bool IsPrimary { get; set; } = true;

        [Display(Name = "Verified")]
        public bool IsVerified { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public IEnumerable<SelectListItem> AddressTypes { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> StateProvinces { get; set; } = Enumerable.Empty<SelectListItem>();

        /// <summary>When true, Address Type dropdown is read-only (Farm/Customer/Organization flow).</summary>
        public bool IsAddressTypeReadOnly { get; set; }
    }
}
