namespace TpaSodManagement.ViewModels.Currency
{
    public class CurrencyItemViewModel
    {
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string CurrencyName { get; set; } = string.Empty;
        public string? CurrencySymbol { get; set; }
        public int DecimalPlaces { get; set; }
        public bool IsActive { get; set; }
    }
}

