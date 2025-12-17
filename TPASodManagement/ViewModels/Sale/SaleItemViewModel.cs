namespace TpaSodManagement.ViewModels.Sale
{
    public class SaleItemViewModel
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
        public DateTimeOffset? CreatedDate { get; set; }
        public DateTimeOffset? UpdatedDate { get; set; }
        public string? CurrencyName { get; set; }
        public long? CustomerId { get; set; }
        public string? CustomerDisplay { get; set; }
        public long? FarmId { get; set; }
        public string? FarmDisplay { get; set; }
        public string? SaleTypeName { get; set; }
        public string? StatusName { get; set; }
        public string? UpdatedByUserName { get; set; }
        public string? UserName { get; set; }
    }
}

