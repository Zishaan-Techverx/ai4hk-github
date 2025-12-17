namespace TpaSodManagement.ViewModels.AreaType
{
    public class AreaTypeItemViewModel
    {
        public int AreaTypeId { get; set; }
        public string AreaTypeName { get; set; } = string.Empty;
        public string? UnitAbbreviation { get; set; }
        public string? UnitSystem { get; set; }
        public decimal? ConversionToSquareMeters { get; set; }
        public bool IsActive { get; set; }
    }
}

