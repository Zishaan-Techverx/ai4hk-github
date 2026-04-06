using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TpaSodManagement.ViewModels.Farm
{
    public class FarmEditViewModel
    {
        public long FarmId { get; set; }

        [Required(ErrorMessage = "Farm Name is required")]
        [Display(Name = "Farm Name")]
        public string FarmName { get; set; } = string.Empty;

        public decimal? TotalArea { get; set; }

        public bool OrganicCertified { get; set; }

        [StringLength(100)]
        public string? LicenseNumber { get; set; }

        [StringLength(500)]
        public string? CertificationDetails { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? ElevationMeters { get; set; }

        [StringLength(100)]
        public string? SoilType { get; set; }

        [StringLength(100)]
        public string? IrrigationType { get; set; }

        [StringLength(100)]
        public string? ClimateZone { get; set; }

        public long? AreaTypeId { get; set; }

        public long? AddressId { get; set; }

        [Display(Name = "Farm Logo")]
        public IFormFile? LogoFile { get; set; }

        public string? LogoFilePath { get; set; }

        public bool IsActive { get; set; } = true;

        public IEnumerable<SelectListItem> AreaTypes { get; set; } = Enumerable.Empty<SelectListItem>();

        public bool IsDetailsView { get; set; }
    }
}

