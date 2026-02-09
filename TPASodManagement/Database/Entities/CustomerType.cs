using TpaSodManagement.Database.Base_Entities;

namespace TpaSodManagement.Database.Entities;

public partial class CustomerType : AuditBaseEntity
{
    public long CustomerTypeId { get; set; }
    public string CustomerTypeName { get; set; } = null!;
    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
