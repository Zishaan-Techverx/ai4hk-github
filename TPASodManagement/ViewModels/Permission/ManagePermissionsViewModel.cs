using System.Collections.Generic;
using Perm = TpaSodManagement.Database.Entities.Permission;
using RolePerm = TpaSodManagement.Database.Entities.RolePermission;

namespace TpaSodManagement.ViewModels.Permission
{
    public class ManagePermissionsViewModel
    {
        public long RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public List<Perm> AllPermissions { get; set; } = new();
        public List<RolePerm> RolePermissions { get; set; } = new();
    }
}

