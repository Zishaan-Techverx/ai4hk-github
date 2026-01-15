using System;

namespace TpaSodManagement.Database.Base_Entities
{
    public abstract class AuditBaseEntity
    {
        public DateTimeOffset CreatedDate { get; set; }
        
        public long? CreatedByUserId { get; set; }
        
        public DateTimeOffset? UpdatedDate { get; set; }
        
        public long? UpdatedByUserId { get; set; }
        
        public DateTimeOffset? DeletedDate { get; set; }
        
        public long? DeletedByUserId { get; set; }
    }
}
