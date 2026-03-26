namespace TpaSodManagement.ViewModels.Field
{
    public class FieldItemViewModel
    {
        public long FieldId { get; set; }
        public string? FieldName { get; set; }
        public string? FieldCode { get; set; }
        public decimal? AreaAmount { get; set; }
        public string? AreaTypeName { get; set; }
        public string? FieldTypeName { get; set; }
        public string? FarmName { get; set; }
        public long? FarmId { get; set; }
        public string? SoilType { get; set; }
        public bool IrrigationAvailable { get; set; }
        public bool IsActive { get; set; }
    }
}

