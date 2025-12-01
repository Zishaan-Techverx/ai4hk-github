using Microsoft.AspNetCore.Identity;

namespace TpaSodManagement.Models
{
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
    }

    public class RolePermission
    {
        public int Id { get; set; }
        public string RoleId { get; set; }
        public int PermissionId { get; set; }
        public bool IsActive { get; set; }

        // Navigation properties
        public IdentityRole Role { get; set; }
        public Permission Permission { get; set; }
    }
}