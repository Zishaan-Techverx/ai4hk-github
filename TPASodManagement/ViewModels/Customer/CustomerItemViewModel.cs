namespace TpaSodManagement.ViewModels.Customer
{
    public class CustomerItemViewModel
    {
        public long CustomerId { get; set; }
        public string? CustomerType { get; set; }
        public string? CustomerCode { get; set; }
        public decimal? CreditLimit { get; set; }
        public int? PaymentTermsDays { get; set; }
        public bool TaxExempt { get; set; }
        public string? Notes { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public DateTimeOffset? UpdatedDate { get; set; }
        public string? OrganizationName { get; set; }
        public string? PersonFullName { get; set; }
    }
}

