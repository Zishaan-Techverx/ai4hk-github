using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TpaSodManagement.ViewModels.Seeding
{
    public class SeedingEditViewModel : IValidatableObject
    {
        public long SeedingId { get; set; }

        [Display(Name = "Area Amount")]
        public decimal? AreaAmount { get; set; }

        [Required(ErrorMessage = "Area Type is required")]
        [Display(Name = "Area Type")]
        public int? AreaTypeId { get; set; }
        
        [Required(ErrorMessage = "Farm is required")]
        [Display(Name = "Farm")]
        public long? FarmId { get; set; }
        
        public long? FieldId { get; set; }
        
        [Required(ErrorMessage = "Tag Start Number is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Tag Start Number must be a positive integer")]
        [Display(Name = "Tag Start Number")]
        public int? TagStartNumber { get; set; }

        [Required(ErrorMessage = "Tag End Number is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Tag End Number must be a positive integer")]
        [Display(Name = "Tag End Number")]
        public int? TagEndNumber { get; set; }
        
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
        public IEnumerable<SelectListItem> Users { get; set; } = Enumerable.Empty<SelectListItem>();

        public bool IsDetailsView { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (TagStartNumber.HasValue && TagEndNumber.HasValue && TagStartNumber.Value > TagEndNumber.Value)
            {
                yield return new ValidationResult(
                    "Tag End Number must be greater than or equal to Tag Start Number.",
                    new[] { nameof(TagEndNumber) });
            }
        }
    }
}

