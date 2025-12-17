using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TpaSodManagement.ViewModels.Product
{
    public class ProductEditViewModel
    {
        public long ProductId { get; set; }

        [StringLength(50)]
        public string? ProductCode { get; set; }

        [Required]
        [StringLength(200)]
        public string? ProductName { get; set; }

        [StringLength(50)]
        public string? UnitOfMeasure { get; set; }

        public decimal? StandardPrice { get; set; }

        public bool RequiresCertificate { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public int? CertificateTypeId { get; set; }
        public long? CreatedByUserId { get; set; }
        public int? CurrencyId { get; set; }
        public int? ProductCategoryId { get; set; }

        public DateTimeOffset? CreatedDate { get; set; }

        public IEnumerable<SelectListItem> CertificateTypes { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Users { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Currencies { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Categories { get; set; } = Enumerable.Empty<SelectListItem>();

        public bool IsDetailsView { get; set; }
    }
}

