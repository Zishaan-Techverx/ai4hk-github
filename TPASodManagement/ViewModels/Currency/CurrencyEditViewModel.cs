using System.ComponentModel.DataAnnotations;

namespace TpaSodManagement.ViewModels.Currency
{
    public class CurrencyEditViewModel
    {
        public int CurrencyId { get; set; }

        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string CurrencyCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CurrencyName { get; set; } = string.Empty;

        [StringLength(10)]
        public string? CurrencySymbol { get; set; }

        [Range(0, 4)]
        public int DecimalPlaces { get; set; } = 2;

        public bool IsActive { get; set; } = true;

        public DateTimeOffset? CreatedDate { get; set; }

        public bool IsDetailsView { get; set; }
    }
}

