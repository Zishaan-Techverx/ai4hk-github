namespace TpaSodManagement.ViewModels.Organization
{
    public class OrganizationItemViewModel
    {
        public long OrganizationId { get; set; }
        public string? OrganizationName { get; set; }
        public long? OrganizationTypeId { get; set; }
        public string? OrganizationTypeName { get; set; }
        public string? Address { get; set; }
        public bool HasLogo { get; set; }
        public bool IsActive { get; set; }
    }
}

