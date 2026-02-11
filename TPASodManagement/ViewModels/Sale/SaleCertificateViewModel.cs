namespace TpaSodManagement.ViewModels.Sale;

public class SaleCertificateViewModel
{
    public long SaleId { get; set; }
    public string SaleNumber { get; set; } = string.Empty;

    /// <summary>Farm name for LICENSED GROWER.</summary>
    public string LicensedGrower { get; set; } = string.Empty;

    /// <summary>Farm address (full address) for certificate.</summary>
    public string FarmAddress { get; set; } = string.Empty;

    /// <summary>Date certificate issued = Sale date.</summary>
    public string DateCertificateIssued { get; set; } = string.Empty;

    /// <summary>Area/amount sold from sale (TotalAmount).</summary>
    public string AreaSold { get; set; } = string.Empty;

    /// <summary>Invoice number(s) from the sale.</summary>
    public string InvoiceNumbers { get; set; } = string.Empty;

    /// <summary>Customer name (FirstName MiddleName LastName or Organization name).</summary>
    public string Customer { get; set; } = string.Empty;

    /// <summary>Relative path to certificate image in wwwroot (e.g. Public Data/Certificates/RTF Sod Certificate 2026_page-0001.jpg).</summary>
    public string CertificateImagePath { get; set; } = string.Empty;

    /// <summary>Filename only for mapping (HGT, RTF, or RTFHGT).</summary>
    public string CertificateImageFileName { get; set; } = string.Empty;
}
