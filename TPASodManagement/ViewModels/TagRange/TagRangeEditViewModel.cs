using System.ComponentModel.DataAnnotations;

namespace TpaSodManagement.ViewModels.TagRange
{
    public class TagRangeEditViewModel
    {
        public long TagRangeId { get; set; }

        [Required(ErrorMessage = "Tag Range Code is required")]
        [StringLength(50)]
        [Display(Name = "Tag Range Code")]
        public string TagRangeCode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Tag Start Number")]
        public long TagStartNumber { get; set; }

        [Required]
        [Display(Name = "Tag End Number")]
        public long TagEndNumber { get; set; }

        [StringLength(20)]
        [Display(Name = "Tag Prefix")]
        public string? TagPrefix { get; set; }

        [StringLength(20)]
        [Display(Name = "Tag Suffix")]
        public string? TagSuffix { get; set; }

        [Display(Name = "Total Tags")]
        public int TotalTags { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTimeOffset? CreatedDate { get; set; }

        public bool IsDetailsView { get; set; }
    }
}
