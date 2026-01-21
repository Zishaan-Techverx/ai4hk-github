using System;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Database.Base_Entities;

namespace TpaSodManagement.Database.Entities;

public partial class NotificationResponse : AuditBaseEntity
{
    public long NotificationResponseId { get; set; }
    
    public long NotificationId { get; set; }
    
    public long UserId { get; set; }
    
    public string Reply { get; set; } = null!;
    
    public virtual Notification Notification { get; set; } = null!;
    
    public virtual TpaSodManagementUser User { get; set; } = null!;
}

