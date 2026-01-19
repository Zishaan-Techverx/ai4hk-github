using System;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Database.Base_Entities;

namespace TpaSodManagement.Database.Entities;

public partial class NotificationUser : AuditBaseEntity
{
    public long NotificationUserId { get; set; }
    
    public long NotificationId { get; set; }
    
    public long UserId { get; set; }
    
    public bool IsRead { get; set; } = false;
    
    public DateTimeOffset? ReadDate { get; set; }
    
    public virtual Notification Notification { get; set; } = null!;
    
    public virtual TpaSodManagementUser User { get; set; } = null!;
}

