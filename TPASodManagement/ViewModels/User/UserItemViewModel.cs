namespace TpaSodManagement.ViewModels.User
{
    public class UserItemViewModel
    {
        public long Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AddressLine1 { get; set; }
        public string? City { get; set; }
        public string? StateName { get; set; }
        public string? PostalCode { get; set; }
        public bool IsActive { get; set; }
    }
}

