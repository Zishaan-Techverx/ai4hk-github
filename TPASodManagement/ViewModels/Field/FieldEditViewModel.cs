using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TpaSodManagement.ViewModels.Field
{
    public class FieldEditViewModel
    {
        public long FieldId { get; set; }

        [Required]
        [StringLength(150)]
        public string? FieldName { get; set; }

        [StringLength(50)]
        public string? FieldCode { get; set; }

        [Required]
        public long? FarmId { get; set; }

        [Required]
        public long? AreaTypeId { get; set; }

        [Required]
        public decimal? AreaAmount { get; set; }

        [Required]
        public long? CreatedByUserId { get; set; }

        [StringLength(100)]
        public string? SoilType { get; set; }

        public decimal? SlopePercentage { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public bool IrrigationAvailable { get; set; }
        public bool IsActive { get; set; } = true;

        public string? BoundaryCoordinates { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public IEnumerable<SelectListItem> Farms { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> AreaTypes { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Users { get; set; } = Enumerable.Empty<SelectListItem>();

        public bool IsDetailsView { get; set; }
    }
}

