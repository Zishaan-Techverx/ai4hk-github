using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Services.Implementations
{
    public class OrganizationService : IOrganizationService
    {
        private readonly SodDbContext _context;
        private readonly IWebHostEnvironment _env;

        public OrganizationService(SodDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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

        public async Task<Organization> CreateOrganizationAsync(Organization organization, IFormFile logoFile)
        {
            if (logoFile != null)
            {
                organization.LogoPath = await SaveLogoFileAsync(logoFile);
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

            orgDb.OrganizationName = updatedOrg.OrganizationName;
            orgDb.OrganizationType = updatedOrg.OrganizationType;

            if (logoFile != null)
            {
                if (!string.IsNullOrEmpty(orgDb.LogoPath))
                {
                    DeleteLogoFile(orgDb.LogoPath);
                }

                orgDb.LogoPath = await SaveLogoFileAsync(logoFile);
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

            if (!string.IsNullOrEmpty(organization.LogoPath))
            {
                DeleteLogoFile(organization.LogoPath);
            }

            _context.Organizations.Remove(organization);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> OrganizationExistsAsync(long id)
        {
            return await _context.Organizations.AnyAsync(e => e.OrganizationId == id);
        }

        private async Task<string> SaveLogoFileAsync(IFormFile logoFile)
        {
            var uploadPath = Path.Combine(_env.WebRootPath, "uploads/organizations");

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            string fileName = Guid.NewGuid() + Path.GetExtension(logoFile.FileName);
            string filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await logoFile.CopyToAsync(stream);
            }

            return "/uploads/organizations/" + fileName;
        }

        private void DeleteLogoFile(string logoPath)
        {
            var logoFile = Path.Combine(_env.WebRootPath, logoPath.TrimStart('/'));
            if (File.Exists(logoFile))
                File.Delete(logoFile);
        }
    }
}