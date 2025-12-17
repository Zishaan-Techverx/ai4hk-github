using System.Collections.Generic;

namespace TpaSodManagement.ViewModels.Admin
{
    public class AdminIndexViewModel
    {
        public List<RoleItemViewModel> Roles { get; set; } = new();
        public List<UserItemViewModel> Users { get; set; } = new();
    }

    public class RoleItemViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class UserItemViewModel
    {
        public long Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
}

