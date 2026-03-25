using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using TpaSodManagement.Database.Entities;
using TpaSodManagement.Services;

namespace TpaSodManagement.Services.Interfaces
{
    public interface IOrganizationService
    {
        Task<List<Organization>> GetAllOrganizationsAsync();
        Task<Organization> GetOrganizationByIdAsync(long id);
        Task<Organization> GetOrganizationByNameAsync(string organizationName);
        Task<Organization> CreateOrganizationAsync(Organization organization, IFormFile logoFile);
        Task<Organization> UpdateOrganizationAsync(long id, Organization updatedOrg, IFormFile logoFile);
        Task<bool> DeleteOrganizationAsync(long id, long? deletedByUserId);
        Task<bool> OrganizationExistsAsync(long id);
        /// <summary>
        /// Returns true if another organization exists with the same name (excludeOrganizationId excluded when editing).
        /// </summary>
        Task<bool> ExistsDuplicateNameAsync(string organizationName, long? excludeOrganizationId = null);
        Task<ServiceResponse<List<Organization>>> GetFilteredAsync(Dictionary<string, string> filters);
    }
}