using TpaSodManagement.Database.Base_Entities;

namespace TpaSodManagement.Database.Entities;

public partial class FieldType : AuditBaseEntity
{
    public long FieldTypeId { get; set; }
    public string FieldTypeName { get; set; } = null!;
    public virtual ICollection<Field> Fields { get; set; } = new List<Field>();
}
