using Microsoft.AspNetCore.Mvc.Rendering;

namespace TpaSodManagement.ViewModels;

public class FarmViewModel
{
    public long FarmId { get; set; }

    // Organization reference (flattened)
    public long OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;

    // Measurements
    public decimal? TotalArea { get; set; }
    public int? AreaTypeId { get; set; }
    public string? AreaTypeName { get; set; }

    public bool OrganicCertified { get; set; }

    // Location info (simplified)
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    // For dropdowns
    public IEnumerable<SelectListItem>? AreaTypes { get; set; }
}