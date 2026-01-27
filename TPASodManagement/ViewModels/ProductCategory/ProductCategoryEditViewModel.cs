using System.ComponentModel.DataAnnotations;

namespace TpaSodManagement.ViewModels.ProductCategory
{
    public class ProductCategoryEditViewModel
    {
        public int ProductCategoryId { get; set; }

        [Required(ErrorMessage = "Category Code is required")]
        [StringLength(50)]
        [Display(Name = "Category Code")]
        public string? CategoryCode { get; set; }

        [Required(ErrorMessage = "Category Name is required")]
        [StringLength(100)]
        [Display(Name = "Category Name")]
        public string? CategoryName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTimeOffset? CreatedDate { get; set; }

        public bool IsDetailsView { get; set; }
    }
}

