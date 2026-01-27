using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TpaSodManagement.ViewModels.Organization
{
    public class OrganizationEditViewModel
    {
        public long OrganizationId { get; set; }

        [Required(ErrorMessage = "Organization Name is required")]
        [StringLength(200)]
        [Display(Name = "Organization Name")]
        public string? OrganizationName { get; set; }

        [Required(ErrorMessage = "Organization Type is required")]
        [StringLength(100)]
        [Display(Name = "Organization Type")]
        public string? OrganizationType { get; set; }

        [StringLength(50)]
        public string? OrganizationCode { get; set; }

        [StringLength(100)]
        public string? TaxIdentificationNumber { get; set; }

        [StringLength(100)]
        public string? RegistrationNumber { get; set; }

        public DateTimeOffset? EstablishedDate { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public IFormFile? LogoFile { get; set; }
        public byte[]? LogoBytes { get; set; }
        public bool HasLogo { get; set; }

        public bool IsDetailsView { get; set; }
    }
}

