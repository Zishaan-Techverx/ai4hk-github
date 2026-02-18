namespace TpaSodManagement.ViewModels.Permission
{
    /// <summary>
    /// Used for form binding when updating role permissions.
    /// Checkbox checked = Granted true = add this permission to the role.
    /// </summary>
    public class PermissionUpdateItemViewModel
    {
        public int PermissionId { get; set; }
        public bool Granted { get; set; }
    }
}
