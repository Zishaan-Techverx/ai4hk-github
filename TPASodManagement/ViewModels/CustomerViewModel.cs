using Microsoft.AspNetCore.Mvc.Rendering;

namespace TpaSodManagement.ViewModels;

public class CustomerViewModel
{
    public long CustomerId { get; set; }
    public string CustomerType { get; set; } = "PERSON"; // PERSON or ORG

    // Flatten choice
    public string? PersonName { get; set; }
    public string? OrganizationName { get; set; }

    public string? CustomerCode { get; set; }
    public decimal? CreditLimit { get; set; }
    public int PaymentTermsDays { get; set; } = 30;

    public bool TaxExempt { get; set; }
    public bool IsActive { get; set; }

    // Dropdowns for selects
    public IEnumerable<SelectListItem>? Organizations { get; set; }
    public IEnumerable<SelectListItem>? Persons { get; set; }
}