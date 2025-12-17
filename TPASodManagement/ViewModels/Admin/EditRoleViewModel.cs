using System.Collections.Generic;

namespace TpaSodManagement.ViewModels.Admin
{
    public class EditRoleViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> UsersInRole { get; set; } = new();
    }
}

