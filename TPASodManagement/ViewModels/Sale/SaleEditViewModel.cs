using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TpaSodManagement.ViewModels.Sale
{
    public class SaleEditViewModel
    {
        public long SaleId { get; set; }

        public string? SaleNumber { get; set; }
        public string? InvoiceNumber { get; set; }
        public string? PurchaseOrderNumber { get; set; }

        public DateOnly? SaleDate { get; set; }
        public DateOnly? DueDate { get; set; }

        public decimal? SubtotalAmount { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? TotalAmount { get; set; }

        public int? PaymentTermsDays { get; set; }

        public string? Notes { get; set; }

        public long? UserId { get; set; }
        public long? FarmId { get; set; }
        public long? CustomerId { get; set; }
        public int? SaleTypeId { get; set; }
        public int? StatusId { get; set; }
        public int? CurrencyId { get; set; }
        public long? UpdatedByUserId { get; set; }

        public DateTimeOffset? CreatedDate { get; set; }
        public DateTimeOffset? UpdatedDate { get; set; }

        public IEnumerable<SelectListItem> Users { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Farms { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Customers { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> SaleTypes { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Statuses { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Currencies { get; set; } = Enumerable.Empty<SelectListItem>();

        public bool IsDetailsView { get; set; }
    }
}

