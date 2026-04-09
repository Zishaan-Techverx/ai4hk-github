using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TpaSodManagement.ViewModels.Sale
{
    public class SaleEditViewModel
    {
        public long SaleId { get; set; }

        public string? InvoiceNumber { get; set; }

        [Required(ErrorMessage = "Sale Date is required")]
        [Display(Name = "Sale Date")]
        public DateOnly? SaleDate { get; set; }

        public string? Notes { get; set; }

        [Required(ErrorMessage = "User is required")]
        [Display(Name = "User")]
        public long? UserId { get; set; }
        
        [Required(ErrorMessage = "Customer is required")]
        [Display(Name = "Customer")]
        public long? CustomerId { get; set; }

        [Required(ErrorMessage = "Field is required")]
        [Display(Name = "Field")]
        public long? FieldId { get; set; }
        
        [Required(ErrorMessage = "Sale Type is required")]
        [Display(Name = "Sale Type")]
        public int? SaleTypeId { get; set; }
        
        public long? UpdatedByUserId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTimeOffset? CreatedDate { get; set; }
        public DateTimeOffset? UpdatedDate { get; set; }

        public IEnumerable<SelectListItem> Users { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Customers { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Fields { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> SaleTypes { get; set; } = Enumerable.Empty<SelectListItem>();
        public bool IsDetailsView { get; set; }
    }
}

