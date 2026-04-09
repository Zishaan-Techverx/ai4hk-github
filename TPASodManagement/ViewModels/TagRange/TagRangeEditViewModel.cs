using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using TpaSodManagement.Database.Entities;

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

        [Required]
        [Display(Name = "Farm")]
        public long? FarmId { get; set; }

        [Required]
        [Display(Name = "Type of Seed")]
        public TagSeedType? SeedType { get; set; }

        [Display(Name = "Total Tags")]
        public int TotalTags { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTimeOffset? CreatedDate { get; set; }

        public IEnumerable<SelectListItem> Farms { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> SeedTypes { get; set; } = Enumerable.Empty<SelectListItem>();

        public bool IsDetailsView { get; set; }
    }
}
