using System.ComponentModel.DataAnnotations;

namespace TpaSodManagement.ViewModels.ProductCategory
{
    public class ProductCategoryEditViewModel
    {
        public int ProductCategoryId { get; set; }

        [Required]
        [StringLength(50)]
        public string? CategoryCode { get; set; }

        [Required]
        [StringLength(100)]
        public string? CategoryName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTimeOffset? CreatedDate { get; set; }

        public bool IsDetailsView { get; set; }
    }
}

