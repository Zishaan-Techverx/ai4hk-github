using Microsoft.AspNetCore.Mvc.Rendering;

namespace TpaSodManagement.ViewModels;

public class SaleViewModel
{
    public long SaleId { get; set; }

    // Basic info
    public string SaleNumber { get; set; } = string.Empty;
    public string? InvoiceNumber { get; set; }
    public DateTime SaleDate { get; set; } = DateTime.Today;
    public DateTime? DueDate { get; set; }

    // Relations (flattened)
    public long CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int SaleTypeId { get; set; }
    public string SaleTypeName { get; set; } = string.Empty;

    // Money
    public decimal SubtotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public int CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;

    // Dropdown collections
    public IEnumerable<SelectListItem>? Customers { get; set; }
    public IEnumerable<SelectListItem>? SaleTypes { get; set; }
    public IEnumerable<SelectListItem>? Currencies { get; set; }

    // Nested line items
    public List<SaleLineItemViewModel> LineItems { get; set; } = new();
}

public class SaleLineItemViewModel
{
    public long SaleLineItemId { get; set; }
    public int LineNumber { get; set; }

    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;

    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    public decimal? AreaAmount { get; set; }
    public int? AreaTypeId { get; set; }
    public string? AreaTypeName { get; set; }
}