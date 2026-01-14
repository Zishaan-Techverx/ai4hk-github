using System;
using System.Collections.Generic;

namespace TpaSodManagement.Database.Entities;

public partial class AddressType
{
    public int AddressTypeId { get; set; }

    public string AddressTypeCode { get; set; } = null!;

    public string AddressTypeName { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public long? DeletedByUserId { get; set; }

    public DateTimeOffset? DeletedDate { get; set; }

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
}
