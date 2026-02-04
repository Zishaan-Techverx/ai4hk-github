using TpaSodManagement.Database.Base_Entities;

namespace TpaSodManagement.Database.Entities
{
    public partial class OrganizationType : AuditBaseEntity
    {
        public long OrganizationTypeId { get; set; }
        public string OrganizationTypeName { get; set; } = null!;
        public virtual ICollection<Organization> Organizations { get; set; } = new List<Organization>();
    }
}
