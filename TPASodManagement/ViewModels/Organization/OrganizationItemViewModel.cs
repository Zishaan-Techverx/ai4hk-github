namespace TpaSodManagement.ViewModels.Organization
{
    public class OrganizationItemViewModel
    {
        public long OrganizationId { get; set; }
        public string? OrganizationName { get; set; }
        public string? OrganizationType { get; set; }
        public bool HasLogo { get; set; }
    }
}

