using System;
using System.Collections.Generic;
using TpaSodManagement.Database.Base_Entities;

namespace TpaSodManagement.Database.Entities;

public partial class Country : AuditBaseEntity
{
    public int CountryId { get; set; }

    public string CountryCode { get; set; } = null!;

    public string CountryName { get; set; } = null!;

    public string? CurrencyCode { get; set; }

    public string? PhonePrefix { get; set; }

    public virtual ICollection<StateProvince> StateProvinces { get; set; } = new List<StateProvince>();
}
