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
        /// <summary>Static file URL when logo is on disk (e.g. /uploads/logos/organization_1.png). Use in img src — no extra API call.</summary>
        public string? LogoUrl { get; set; }
        /// <summary>Data URL when logo is inline in listing (base64). Use in img src when LogoUrl is null — no extra API call.</summary>
        public string? LogoDataUrl { get; set; }
        public bool IsActive { get; set; }
    }
}

