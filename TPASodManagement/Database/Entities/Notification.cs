using System;
using System.Collections.Generic;
using TpaSodManagement.Database.Base_Entities;

namespace TpaSodManagement.Database.Entities;

public partial class Notification : AuditBaseEntity
{
    public long NotificationId { get; set; }
    
    public string Title { get; set; } = null!;
    
    public string Message { get; set; } = null!;
    
    public string Priority { get; set; } = null!; // Critical, High, Medium, Low
    
    public bool IsActive { get; set; } = true;
    
    public virtual ICollection<NotificationUser> NotificationUsers { get; set; } = new List<NotificationUser>();
}

