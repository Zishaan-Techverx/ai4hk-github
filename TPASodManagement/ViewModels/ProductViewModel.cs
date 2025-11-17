using Microsoft.AspNetCore.Mvc.Rendering;

namespace TpaSodManagement.ViewModels;

public class ProductViewModel
{
    public long ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;

    public int ProductCategoryId { get; set; }
    public string ProductCategoryName { get; set; } = string.Empty;

    public string UnitOfMeasure { get; set; } = "Unit";
    public decimal? StandardPrice { get; set; }

    public int? CurrencyId { get; set; }
    public string? CurrencyCode { get; set; }

    public bool RequiresCertificate { get; set; }

    // Dropdowns
    public IEnumerable<SelectListItem>? Categories { get; set; }
    public IEnumerable<SelectListItem>? Currencies { get; set; }
    public IEnumerable<SelectListItem>? CertificateTypes { get; set; }
}