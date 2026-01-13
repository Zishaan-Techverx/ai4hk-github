using System;
using System.Collections.Generic;

namespace TpaSodManagement.Database.Entities;

public partial class StateProvince
{
    public int StateProvinceId { get; set; }

    public int CountryId { get; set; }

    public string StateCode { get; set; } = null!;

    public string StateName { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public long? DeletedByUserId { get; set; }

    public DateTimeOffset? DeletedDate { get; set; }

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    public virtual Country Country { get; set; } = null!;
}
