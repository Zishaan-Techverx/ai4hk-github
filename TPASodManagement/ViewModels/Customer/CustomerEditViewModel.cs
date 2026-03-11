using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TpaSodManagement.ViewModels.Customer
{
    public class CustomerEditViewModel
    {
        public long CustomerId { get; set; }

        [Required(ErrorMessage = "Customer Type is required")]
        [Display(Name = "Customer Type")]
        public long? CustomerTypeId { get; set; }

        [StringLength(50)]
        public string? CustomerCode { get; set; }

        public decimal? CreditLimit { get; set; }

        public int? PaymentTermsDays { get; set; }

        public bool TaxExempt { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTimeOffset? CreatedDate { get; set; }
        public DateTimeOffset? UpdatedDate { get; set; }

        public long? OrganizationId { get; set; }
        public long? PersonId { get; set; }
        public long? AddressId { get; set; }

        /// <summary>When true, create a new person from NewPersonFirstName/NewPersonLastName and use it for the customer.</summary>
        public bool CreateNewPerson { get; set; }

        [StringLength(200)]
        [Display(Name = "First Name")]
        public string? NewPersonFirstName { get; set; }

        [StringLength(200)]
        [Display(Name = "Last Name")]
        public string? NewPersonLastName { get; set; }

        public IEnumerable<SelectListItem> CustomerTypes { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Organizations { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> People { get; set; } = Enumerable.Empty<SelectListItem>();

        public bool IsDetailsView { get; set; }
    }
}

