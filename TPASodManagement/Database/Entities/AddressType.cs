using System;
using System.Collections.Generic;
using TpaSodManagement.Database.Base_Entities;

namespace TpaSodManagement.Database.Entities;

public partial class AddressType : AuditBaseEntity
{
    public int AddressTypeId { get; set; }

    public string AddressTypeCode { get; set; } = null!;

    public string AddressTypeName { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
}
