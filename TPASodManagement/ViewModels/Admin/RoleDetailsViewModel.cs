using System.Collections.Generic;

namespace TpaSodManagement.ViewModels.Admin
{
    public class RoleDetailsViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? NormalizedName { get; set; }
        public List<UserRolesViewModel> UsersWithRoles { get; set; } = new();
    }

    public class UserRolesViewModel
    {
        public long Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
}

