using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TpaSodManagement.ViewModels.SaleType
{
    public class SaleTypeEditViewModel
    {
        public int SaleTypeId { get; set; }

        [Required(ErrorMessage = "Sale Type Code is required")]
        [StringLength(50)]
        [Display(Name = "Sale Type Code")]
        public string SaleTypeCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sale Type Name is required")]
        [StringLength(100)]
        [Display(Name = "Sale Type Name")]
        public string SaleTypeName { get; set; } = string.Empty;

        [Display(Name = "Requires Certificate")]
        public bool RequiresCertificate { get; set; }

        [Display(Name = "Certificate Type")]
        public int? CertificateTypeId { get; set; }

        [Display(Name = "Tax Applicable")]
        public bool TaxApplicable { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTimeOffset? CreatedDate { get; set; }

        public bool IsDetailsView { get; set; }

        public IEnumerable<SelectListItem> CertificateTypes { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
