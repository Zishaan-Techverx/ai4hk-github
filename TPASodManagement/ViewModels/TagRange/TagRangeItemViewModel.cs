namespace TpaSodManagement.ViewModels.TagRange
{
    public class TagRangeItemViewModel
    {
        public long TagRangeId { get; set; }
        public string TagRangeCode { get; set; } = string.Empty;
        public long TagStartNumber { get; set; }
        public long TagEndNumber { get; set; }
        public string? TagPrefix { get; set; }
        public string? TagSuffix { get; set; }
        public int TotalTags { get; set; }
        public bool IsActive { get; set; }
    }
}
