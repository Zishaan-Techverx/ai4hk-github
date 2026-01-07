using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TpaSodManagement.Data;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Services.Implementations
{
    public class OrganizationService : IOrganizationService
    {
        private readonly SodDbContext _context;

        // Remove IWebHostEnvironment dependency since we're not using file system
        public OrganizationService(SodDbContext context)
        {
            _context = context;
        }

        public async Task<List<Organization>> GetAllOrganizationsAsync()
        {
            return await _context.Organizations.ToListAsync();
        }

        public async Task<Organization> GetOrganizationByIdAsync(long id)
        {
            return await _context.Organizations
                .FirstOrDefaultAsync(m => m.OrganizationId == id);
        }

        // ADD THIS METHOD
        public async Task<Organization> GetOrganizationByNameAsync(string organizationName)
        {
            if (string.IsNullOrWhiteSpace(organizationName))
                return null;

            return await _context.Organizations
                .FirstOrDefaultAsync(o => o.OrganizationName.ToUpper() == organizationName.ToUpper());
        }

        public async Task<Organization> CreateOrganizationAsync(Organization organization, IFormFile logoFile)
        {
            if (logoFile != null && logoFile.Length > 0)
            {
                organization.LogoBytes = await ConvertFileToBytesAsync(logoFile);
            }

            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync();
            return organization;
        }

        public async Task<Organization> UpdateOrganizationAsync(long id, Organization updatedOrg, IFormFile logoFile)
        {
            var orgDb = await _context.Organizations.FindAsync(id);
            if (orgDb == null)
                return null;

            // Update all fields
            orgDb.OrganizationName = updatedOrg.OrganizationName;
            orgDb.OrganizationType = updatedOrg.OrganizationType;
            orgDb.OrganizationCode = updatedOrg.OrganizationCode;
            orgDb.TaxIdentificationNumber = updatedOrg.TaxIdentificationNumber;
            orgDb.RegistrationNumber = updatedOrg.RegistrationNumber;
            orgDb.EstablishedDate = updatedOrg.EstablishedDate;
            orgDb.Description = updatedOrg.Description;
            orgDb.IsActive = updatedOrg.IsActive;

            // Update logo only if a new file is provided
            if (logoFile != null && logoFile.Length > 0)
            {
                orgDb.LogoBytes = await ConvertFileToBytesAsync(logoFile);
            }

            _context.Organizations.Update(orgDb);
            await _context.SaveChangesAsync();
            return orgDb;
        }

        public async Task<bool> DeleteOrganizationAsync(long id)
        {
            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null)
                return false;

            _context.Organizations.Remove(organization);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> OrganizationExistsAsync(long id)
        {
            return await _context.Organizations.AnyAsync(e => e.OrganizationId == id);
        }

        public async Task<ServiceResponse<List<Organization>>> GetFilteredAsync(Dictionary<string, string> filters)
        {
            var response = new ServiceResponse<List<Organization>>();
            try
            {
                var query = _context.Organizations.AsQueryable();

                // Apply filters
                if (filters != null && filters.Count > 0)
                {
                    if (filters.ContainsKey("OrganizationName") && !string.IsNullOrWhiteSpace(filters["OrganizationName"]))
                    {
                        var filterValue = filters["OrganizationName"].Trim();
                        query = query.Where(o => o.OrganizationName != null && o.OrganizationName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("OrganizationType") && !string.IsNullOrWhiteSpace(filters["OrganizationType"]))
                    {
                        var filterValue = filters["OrganizationType"].Trim();
                        query = query.Where(o => o.OrganizationType != null && o.OrganizationType.Contains(filterValue));
                    }

                    if (filters.ContainsKey("OrganizationCode") && !string.IsNullOrWhiteSpace(filters["OrganizationCode"]))
                    {
                        var filterValue = filters["OrganizationCode"].Trim();
                        query = query.Where(o => o.OrganizationCode != null && o.OrganizationCode.Contains(filterValue));
                    }

                    if (filters.ContainsKey("IsActive") && !string.IsNullOrWhiteSpace(filters["IsActive"]))
                    {
                        if (bool.TryParse(filters["IsActive"], out var isActiveValue))
                        {
                            query = query.Where(o => o.IsActive == isActiveValue);
                        }
                        else if (filters["IsActive"].ToLower() == "true" || filters["IsActive"].ToLower() == "yes" || filters["IsActive"].ToLower() == "1")
                        {
                            query = query.Where(o => o.IsActive == true);
                        }
                        else if (filters["IsActive"].ToLower() == "false" || filters["IsActive"].ToLower() == "no" || filters["IsActive"].ToLower() == "0")
                        {
                            query = query.Where(o => o.IsActive == false);
                        }
                    }
                }

                response.Data = await query.OrderBy(o => o.OrganizationName).ToListAsync();
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching filtered organizations: {ex.Message}";
            }
            return response;
        }

        private async Task<byte[]> ConvertFileToBytesAsync(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}