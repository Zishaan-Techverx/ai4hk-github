using Microsoft.AspNetCore.Identity;
using TpaSodManagement.Database.Base_Entities;

namespace TpaSodManagement.Database.Entities
{
    public class Permission : AuditBaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
    }

    public class RolePermission : AuditBaseEntity
    {
        public int Id { get; set; }
        public long RoleId { get; set; }
        public int PermissionId { get; set; }

        // Navigation properties
        public IdentityRole<long> Role { get; set; }
        public Permission Permission { get; set; }
    }
}