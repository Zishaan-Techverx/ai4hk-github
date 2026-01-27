using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TpaSodManagement.ViewModels.Field
{
    public class FieldEditViewModel
    {
        public long FieldId { get; set; }

        [Required(ErrorMessage = "Field Name is required")]
        [StringLength(150)]
        [Display(Name = "Field Name")]
        public string? FieldName { get; set; }

        [StringLength(50)]
        public string? FieldCode { get; set; }

        [Required(ErrorMessage = "Farm is required")]
        [Display(Name = "Farm")]
        public long? FarmId { get; set; }

        [Required(ErrorMessage = "Area Type is required")]
        [Display(Name = "Area Type")]
        public long? AreaTypeId { get; set; }

        [Required(ErrorMessage = "Area Amount is required")]
        [Display(Name = "Area Amount")]
        public decimal? AreaAmount { get; set; }

        [Required(ErrorMessage = "Created By User is required")]
        [Display(Name = "Created By User")]
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

