namespace TpaSodManagement.ViewModels.TagRange
{
    public class TagRangeItemViewModel
    {
        public long TagRangeId { get; set; }
        public string TagRangeCode { get; set; } = string.Empty;
        public long TagStartNumber { get; set; }
        public long TagEndNumber { get; set; }
        public long FarmId { get; set; }
        public string? FarmName { get; set; }
        public string SeedType { get; set; } = string.Empty;
        public int TotalTags { get; set; }
        public bool IsActive { get; set; }
    }
}
