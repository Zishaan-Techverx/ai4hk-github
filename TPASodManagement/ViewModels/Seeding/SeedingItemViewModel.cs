namespace TpaSodManagement.ViewModels.Seeding
{
    public class SeedingItemViewModel
    {
        public long SeedingId { get; set; }
        public decimal? AreaAmount { get; set; }
        public DateOnly? SeedingDate { get; set; }
        public string? SeedingMethod { get; set; }
        public decimal? SeedRatePerUnit { get; set; }
        public string? WeatherConditions { get; set; }
        public decimal? SoilTemperature { get; set; }
        public string? SoilMoisture { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? AreaTypeName { get; set; }
        public int? AreaTypeId { get; set; }
        public string? FarmDisplay { get; set; }
        public long? FarmId { get; set; }
        public string? FieldName { get; set; }
        public long? FieldId { get; set; }
        public string? TagRangeCode { get; set; }
        public long? TagRangeId { get; set; }
        public string? UserName { get; set; }
    }
}

