namespace TpaSodManagement.ViewModels.Farm
{
    public class FarmItemViewModel
    {
        public long FarmId { get; set; }
        public string FarmName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public decimal? TotalArea { get; set; }
        public bool OrganicCertified { get; set; }
        public string? LicenseNumber { get; set; }
        public string? CertificationDetails { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? ElevationMeters { get; set; }
        public string? SoilType { get; set; }
        public string? IrrigationType { get; set; }
        public string? ClimateZone { get; set; }
        public string? AreaTypeName { get; set; }
        public string? LogoFilePath { get; set; }
        public bool IsActive { get; set; }
    }
}

