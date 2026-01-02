using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Models.Db;
using Microsoft.AspNetCore.Identity;

namespace TpaSodManagement.Services.Interfaces
{
    public interface IRegistrationService
    {
        Task<List<Organization>> GetAllOrganizationsAsync();
        Task<Organization?> GetOrganizationByNameAsync(string organizationName);
        Task<bool> IsEmailExistsAsync(string email);
        Task<string> GenerateUsernameAsync(Organization organization, string firstName);
        Task<IdentityResult> CreateUserAsync(TpaSodManagementUser user, string password);
        Task<Person> CreatePersonForUserAsync(TpaSodManagementUser user, string firstName, string lastName);
        Task<Address> CreateAddressForUserAsync(TpaSodManagementUser user, string? addressLine1 = null, string? state = null, string? country = null, string? postalCode = null);
        Task<Website> CreateWebsiteForUserAsync(TpaSodManagementUser user, string username);
        Task<Farm> CreateOrGetFarmForOrganizationAsync(long organizationId);
        Task<TpaSodManagementUser?> GetUserWithAddressAsync(string userId);
        Task<Address?> GetUserAddressAsync(string userId);
        Task<Person?> GetUserPersonAsync(string userId);
        Task<Website?> GetUserWebsiteAsync(string userId);
        Task<Farm?> GetUserFarmAsync(string userId);
    }
}

