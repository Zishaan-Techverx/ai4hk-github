using System;
using System.Collections.Generic;
using TpaSodManagement.Areas.Identity.Data;

namespace TpaSodManagement.Database.Entities;

public partial class Testimonial
{
    public long TestimonialId { get; set; }

    public long? FarmId { get; set; }

    public long? CustomerId { get; set; }

    public long? PersonId { get; set; }

    public int StatusId { get; set; }

    public string TestimonialTitle { get; set; } = null!;

    public string ProblemsToSolve { get; set; } = null!;

    public string Improvements { get; set; } = null!;

    public string? Comments { get; set; }

    public byte? Rating { get; set; }

    public bool? WouldRecommend { get; set; }

    public string? PhotoReference { get; set; }

    public string? VideoReference { get; set; }

    public DateTimeOffset SubmittedDate { get; set; }

    public DateTimeOffset? ApprovedDate { get; set; }

    public long? ApprovedByUserId { get; set; }

    public DateTimeOffset? PublishedDate { get; set; }

    public bool Featured { get; set; }

    public int? DisplayOrder { get; set; }

    public string? Username { get; set; }

    public string? Email { get; set; }

    public string? ContactNumber { get; set; }

    public string? Organization { get; set; }

    public string? Location { get; set; }

    public string? ApproverName { get; set; }

    public long? DeletedByUserId { get; set; }

    public DateTimeOffset? DeletedDate { get; set; }

    public virtual TpaSodManagementUser? ApprovedByUser { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Farm? Farm { get; set; }

    public virtual Person? Person { get; set; }

    public virtual Status Status { get; set; } = null!;
}
