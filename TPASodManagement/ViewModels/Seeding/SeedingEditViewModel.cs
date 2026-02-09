using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TpaSodManagement.ViewModels.Seeding
{
    public class SeedingEditViewModel
    {
        public long SeedingId { get; set; }

        [Required(ErrorMessage = "Area Amount is required")]
        [Display(Name = "Area Amount")]
        public decimal? AreaAmount { get; set; }

        [Required(ErrorMessage = "Area Type is required")]
        [Display(Name = "Area Type")]
        public int? AreaTypeId { get; set; }
        
        [Required(ErrorMessage = "Farm is required")]
        [Display(Name = "Farm")]
        public long? FarmId { get; set; }
        
        public long? FieldId { get; set; }
        
        [Required(ErrorMessage = "Tag Range is required")]
        [Display(Name = "Tag Range")]
        public long? TagRangeId { get; set; }
        
        [Required(ErrorMessage = "User is required")]
        [Display(Name = "User")]
        public long? UserId { get; set; }

        [Required(ErrorMessage = "Seeding Date is required")]
        [Display(Name = "Seeding Date")]
        public DateOnly? SeedingDate { get; set; }
        public string? SeedingMethod { get; set; }
        public decimal? SeedRatePerUnit { get; set; }
        public string? WeatherConditions { get; set; }
        public decimal? SoilTemperature { get; set; }
        public string? SoilMoisture { get; set; }
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTimeOffset? CreatedDate { get; set; }

        public IEnumerable<SelectListItem> AreaTypes { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Farms { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Fields { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> TagRanges { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Users { get; set; } = Enumerable.Empty<SelectListItem>();

        public bool IsDetailsView { get; set; }
    }
}

