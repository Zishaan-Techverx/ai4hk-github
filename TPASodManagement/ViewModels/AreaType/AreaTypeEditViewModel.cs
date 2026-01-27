using System.ComponentModel.DataAnnotations;

namespace TpaSodManagement.ViewModels.AreaType
{
    public class AreaTypeEditViewModel
    {
        public int AreaTypeId { get; set; }

        [Required(ErrorMessage = "Area Type Name is required")]
        [StringLength(100)]
        [Display(Name = "Area Type Name")]
        public string AreaTypeName { get; set; } = string.Empty;

        [StringLength(10)]
        public string? UnitAbbreviation { get; set; }

        [StringLength(50)]
        public string? UnitSystem { get; set; }

        public decimal? ConversionToSquareMeters { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTimeOffset? CreatedDate { get; set; }

        // View-only flag
        public bool IsDetailsView { get; set; }
    }
}

