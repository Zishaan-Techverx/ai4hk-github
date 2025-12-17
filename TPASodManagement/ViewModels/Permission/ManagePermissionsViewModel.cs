using System.Collections.Generic;
using TpaSodManagement.Models;
using Perm = TpaSodManagement.Models.Permission;
using RolePerm = TpaSodManagement.Models.RolePermission;

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

