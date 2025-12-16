using System;
using System.Collections.Generic;
using TpaSodManagement.Areas.Identity.Data;

namespace TpaSodManagement.Models.Db;

public partial class Person
{
    public long PersonId { get; set; }

    public string FirstName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public string LastName { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? Title { get; set; }

    public string? Bio { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public DateTimeOffset UpdatedDate { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual ICollection<Testimonial> Testimonials { get; set; } = new List<Testimonial>();

    public virtual TpaSodManagementUser? TpaUser { get; set; }
    
    public bool IsPrimaryContact { get;  set; }
}
