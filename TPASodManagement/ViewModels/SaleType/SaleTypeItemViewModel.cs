namespace TpaSodManagement.ViewModels.SaleType
{
    public class SaleTypeItemViewModel
    {
        public int SaleTypeId { get; set; }
        public string SaleTypeCode { get; set; } = string.Empty;
        public string SaleTypeName { get; set; } = string.Empty;
        public bool RequiresCertificate { get; set; }
        public int? CertificateTypeId { get; set; }
        public string? CertificateTypeName { get; set; }
        public bool TaxApplicable { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
