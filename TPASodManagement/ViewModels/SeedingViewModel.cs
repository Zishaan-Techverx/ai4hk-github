using Microsoft.AspNetCore.Mvc.Rendering;

namespace TpaSodManagement.ViewModels;

public class SeedingViewModel
{
    public long SeedingId { get; set; }
    public DateTime SeedingDate { get; set; } = DateTime.Today;

    public long FarmId { get; set; }
    public string FarmName { get; set; } = string.Empty;

    public long? FieldId { get; set; }
    public string? FieldName { get; set; }

    public long TagRangeId { get; set; }
    public string TagRangeCode { get; set; } = string.Empty;

    public decimal AreaAmount { get; set; }
    public int AreaTypeId { get; set; }
    public string AreaTypeName { get; set; } = string.Empty;

    public string? SeedingMethod { get; set; }
    public string? WeatherConditions { get; set; }

    // Dropdowns
    public IEnumerable<SelectListItem>? Farms { get; set; }
    public IEnumerable<SelectListItem>? Fields { get; set; }
    public IEnumerable<SelectListItem>? TagRanges { get; set; }
    public IEnumerable<SelectListItem>? AreaTypes { get; set; }
}