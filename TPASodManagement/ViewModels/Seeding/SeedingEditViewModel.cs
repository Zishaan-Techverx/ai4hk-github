using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TpaSodManagement.ViewModels.Seeding
{
    public class SeedingEditViewModel
    {
        public long SeedingId { get; set; }

        public decimal? AreaAmount { get; set; }

        public int? AreaTypeId { get; set; }
        public long? FarmId { get; set; }
        public long? FieldId { get; set; }
        public long? TagRangeId { get; set; }
        public long? UserId { get; set; }

        public DateOnly? SeedingDate { get; set; }
        public string? SeedingMethod { get; set; }
        public decimal? SeedRatePerUnit { get; set; }
        public string? WeatherConditions { get; set; }
        public decimal? SoilTemperature { get; set; }
        public string? SoilMoisture { get; set; }
        public string? Notes { get; set; }

        public DateTimeOffset? CreatedDate { get; set; }

        public IEnumerable<SelectListItem> AreaTypes { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Farms { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Fields { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> TagRanges { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Users { get; set; } = Enumerable.Empty<SelectListItem>();

        public bool IsDetailsView { get; set; }
    }
}

