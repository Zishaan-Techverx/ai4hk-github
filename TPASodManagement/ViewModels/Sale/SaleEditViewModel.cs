using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TpaSodManagement.ViewModels.Sale
{
    public class SaleEditViewModel
    {
        public long SaleId { get; set; }

        [Required(ErrorMessage = "Sale Number is required")]
        [Display(Name = "Sale Number")]
        public string? SaleNumber { get; set; }
        public string? InvoiceNumber { get; set; }
        public string? PurchaseOrderNumber { get; set; }

        [Required(ErrorMessage = "Sale Date is required")]
        [Display(Name = "Sale Date")]
        public DateOnly? SaleDate { get; set; }
        public DateOnly? DueDate { get; set; }

        [Required(ErrorMessage = "Subtotal Amount is required")]
        [Display(Name = "Subtotal Amount")]
        public decimal? SubtotalAmount { get; set; }
        
        [Required(ErrorMessage = "Tax Amount is required")]
        [Display(Name = "Tax Amount")]
        public decimal? TaxAmount { get; set; }
        
        [Required(ErrorMessage = "Discount Amount is required")]
        [Display(Name = "Discount Amount")]
        public decimal? DiscountAmount { get; set; }
        
        [Required(ErrorMessage = "Total Amount is required")]
        [Display(Name = "Total Amount")]
        public decimal? TotalAmount { get; set; }

        public int? PaymentTermsDays { get; set; }

        public string? Notes { get; set; }

        [Required(ErrorMessage = "User is required")]
        [Display(Name = "User")]
        public long? UserId { get; set; }
        
        [Required(ErrorMessage = "Farm is required")]
        [Display(Name = "Farm")]
        public long? FarmId { get; set; }
        
        [Required(ErrorMessage = "Customer is required")]
        [Display(Name = "Customer")]
        public long? CustomerId { get; set; }

        [Required(ErrorMessage = "Field is required")]
        [Display(Name = "Field")]
        public long? FieldId { get; set; }
        
        [Required(ErrorMessage = "Sale Type is required")]
        [Display(Name = "Sale Type")]
        public int? SaleTypeId { get; set; }
        
        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public int? StatusId { get; set; }
        
        [Required(ErrorMessage = "Currency is required")]
        [Display(Name = "Currency")]
        public int? CurrencyId { get; set; }
        public long? UpdatedByUserId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTimeOffset? CreatedDate { get; set; }
        public DateTimeOffset? UpdatedDate { get; set; }

        public IEnumerable<SelectListItem> Users { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Farms { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Customers { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Fields { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> SaleTypes { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Statuses { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Currencies { get; set; } = Enumerable.Empty<SelectListItem>();

        public bool IsDetailsView { get; set; }
    }
}

