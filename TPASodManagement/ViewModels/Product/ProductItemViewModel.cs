namespace TpaSodManagement.ViewModels.Product
{
    public class ProductItemViewModel
    {
        public long ProductId { get; set; }
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public string? UnitOfMeasure { get; set; }
        public decimal? StandardPrice { get; set; }
        public bool RequiresCertificate { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CertificateTypeName { get; set; }
        public string? CreatedByUserName { get; set; }
        public string? CurrencyCode { get; set; }
        public string? ProductCategoryName { get; set; }
    }
}

