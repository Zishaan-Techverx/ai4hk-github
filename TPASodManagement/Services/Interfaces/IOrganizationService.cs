using Microsoft.AspNetCore.Http;
using TpaSodManagement.Models.Db;

namespace TpaSodManagement.Services.Interfaces
{
    public interface IOrganizationService
    {
        Task<List<Organization>> GetAllOrganizationsAsync();
        Task<Organization> GetOrganizationByIdAsync(long id);
        Task<Organization> GetOrganizationByNameAsync(string organizationName);
        Task<Organization> CreateOrganizationAsync(Organization organization, IFormFile logoFile);
        Task<Organization> UpdateOrganizationAsync(long id, Organization updatedOrg, IFormFile logoFile);
        Task<bool> DeleteOrganizationAsync(long id);
        Task<bool> OrganizationExistsAsync(long id);
    }
}