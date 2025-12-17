namespace TpaSodManagement.ViewModels.ProductCategory
{
    public class ProductCategoryItemViewModel
    {
        public int ProductCategoryId { get; set; }
        public string? CategoryCode { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
    }
}

