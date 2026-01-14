using System;
using System.Collections.Generic;

namespace TpaSodManagement.Database.Entities;

public partial class Currency
{
    public int CurrencyId { get; set; }

    public string CurrencyCode { get; set; } = null!;

    public string CurrencyName { get; set; } = null!;

    public string? CurrencySymbol { get; set; }

    public byte DecimalPlaces { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public long? DeletedByUserId { get; set; }

    public DateTimeOffset? DeletedDate { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    public virtual ICollection<Waste> Wastes { get; set; } = new List<Waste>();
}
