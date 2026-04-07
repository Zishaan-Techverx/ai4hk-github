using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Database;
using TpaSodManagement.Database.Entities;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.Utilities;

namespace TpaSodManagement.Services.Implementations
{
    public class OrganizationService : IOrganizationService
    {
        private const string LogoFolderRelativePath = "uploads/logos";
        private const int LogoMaxSizeBytes = 2 * 1024 * 1024; // 2MB
        private static readonly string[] AllowedLogoExtensions = { ".jpg", ".jpeg", ".png", ".svg" };

        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IWebHostEnvironment _env;

        public OrganizationService(ApplicationDbContext context, ICurrentUserService currentUserService, IWebHostEnvironment env)
        {
            _context = context;
            _currentUserService = currentUserService;
            _env = env;
        }

        /// <summary>
        /// Validates organization logo file: allowed types .jpg, .jpeg, .png, .svg; max size 2MB.
        /// Returns (true, null) if valid; (false, errorMessage) if invalid.
        /// </summary>
        public static (bool IsValid, string? ErrorMessage) ValidateLogoFile(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return (true, null);

            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !AllowedLogoExtensions.Contains(ext))
                return (false, "Logo must be one of: .jpg, .jpeg, .png, .svg");

            if (file.Length > LogoMaxSizeBytes)
                return (false, "Logo size must not exceed 2MB.");

            return (true, null);
        }

        public async Task<List<Organization>> GetAllOrganizationsAsync()
        {
            var query = _context.Organizations
                .Include(o => o.Address).ThenInclude(a => a!.StateProvince)
                .AsQueryable();

            if (!await _currentUserService.IsCurrentUserSuperAdminAsync())
            {
                var currentFarmId = await _currentUserService.GetCurrentUserFarmIdAsync();
                var orgId = currentFarmId.HasValue
                    ? await _context.Farms
                        .Where(f => f.FarmId == currentFarmId.Value)
                        .Select(f => (long?)f.OrganizationId)
                        .FirstOrDefaultAsync()
                    : null;
                if (orgId.HasValue)
                    query = query.Where(o => o.OrganizationId == orgId.Value);
                else
                    query = query.Where(o => false);
            }

            return await query.ToListAsync();
        }

        public async Task<Organization> GetOrganizationByIdAsync(long id)
        {
            return await _context.Organizations
                .Include(o => o.Address).ThenInclude(a => a!.StateProvince)
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
                var (valid, _) = ValidateLogoFile(logoFile);
                if (valid)
                {
                    organization.LogoBytes = await ConvertFileToBytesAsync(logoFile);
                }
            }

            var currentUserId = await _currentUserService.GetCurrentUserIdAsync();
            organization.CreatedDate = DateTimeOffset.UtcNow;
            organization.CreatedByUserId = currentUserId;
            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync();

            if (logoFile != null && logoFile.Length > 0 && organization.LogoBytes != null && organization.LogoBytes.Length > 0)
            {
                var (valid, _) = ValidateLogoFile(logoFile);
                if (valid)
                {
                    var relativePath = await SaveLogoToFileSystemAsync(organization.LogoBytes, organization.OrganizationId, logoFile.FileName);
                    if (relativePath != null)
                    {
                        organization.LogoFilePath = relativePath;
                        _context.Organizations.Update(organization);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            return organization;
        }

        public async Task<Organization> UpdateOrganizationAsync(long id, Organization updatedOrg, IFormFile logoFile)
        {
            var orgDb = await _context.Organizations.FindAsync(id);
            if (orgDb == null)
                return null;

            // Update all fields
            orgDb.OrganizationName = updatedOrg.OrganizationName;
            orgDb.OrganizationCode = updatedOrg.OrganizationCode;
            orgDb.TaxIdentificationNumber = updatedOrg.TaxIdentificationNumber;
            orgDb.RegistrationNumber = updatedOrg.RegistrationNumber;
            orgDb.EstablishedDate = updatedOrg.EstablishedDate;
            orgDb.Description = updatedOrg.Description;
            orgDb.AddressId = updatedOrg.AddressId;
            orgDb.IsActive = updatedOrg.IsActive;

            // Update logo only if a new file is provided
            if (logoFile != null && logoFile.Length > 0)
            {
                var (valid, _) = ValidateLogoFile(logoFile);
                if (valid)
                {
                    orgDb.LogoBytes = await ConvertFileToBytesAsync(logoFile);
                    var relativePath = await SaveLogoToFileSystemAsync(orgDb.LogoBytes, orgDb.OrganizationId, logoFile.FileName);
                    if (relativePath != null)
                        orgDb.LogoFilePath = relativePath;
                }
            }

            var currentUserId = await _currentUserService.GetCurrentUserIdAsync();
            orgDb.UpdatedDate = DateTimeOffset.UtcNow;
            orgDb.UpdatedByUserId = currentUserId;
            _context.Organizations.Update(orgDb);
            await _context.SaveChangesAsync();
            return orgDb;
        }

        public async Task<bool> DeleteOrganizationAsync(long id, long? deletedByUserId)
        {
            var organization = await _context.Organizations
                .FirstOrDefaultAsync(o => o.OrganizationId == id && o.DeletedDate == null);
            if (organization == null)
                return false;

            // Soft delete: Set DeletedDate and DeletedByUserId
            var currentUserId = deletedByUserId ?? await _currentUserService.GetCurrentUserIdAsync();
            organization.DeletedDate = DateTimeOffset.UtcNow;
            organization.DeletedByUserId = currentUserId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> OrganizationExistsAsync(long id)
        {
            return await _context.Organizations.AnyAsync(e => e.OrganizationId == id);
        }

        public async Task<bool> ExistsDuplicateNameAsync(string organizationName, long? excludeOrganizationId = null)
        {
            if (string.IsNullOrWhiteSpace(organizationName))
                return false;

            var nameLower = organizationName.Trim().ToLower();
            // Query database table directly (IgnoreQueryFilters + AsNoTracking), filter non-deleted, then match name+type
            var query = _context.Organizations
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(o => o.DeletedDate == null
                    && o.OrganizationName != null
                    && o.OrganizationName.ToLower() == nameLower);

            if (excludeOrganizationId.HasValue)
                query = query.Where(o => o.OrganizationId != excludeOrganizationId.Value);

            return await query.AnyAsync();
        }

        public async Task<ServiceResponse<List<Organization>>> GetFilteredAsync(Dictionary<string, string> filters)
        {
            var response = new ServiceResponse<List<Organization>>();
            try
            {
                var query = _context.Organizations
                    .Include(o => o.Address).ThenInclude(a => a!.StateProvince)
                    .AsQueryable();

                if (!await _currentUserService.IsCurrentUserSuperAdminAsync())
                {
                    var currentFarmId = await _currentUserService.GetCurrentUserFarmIdAsync();
                    var orgId = currentFarmId.HasValue
                        ? await _context.Farms
                            .Where(f => f.FarmId == currentFarmId.Value)
                            .Select(f => (long?)f.OrganizationId)
                            .FirstOrDefaultAsync()
                        : null;
                    if (orgId.HasValue)
                        query = query.Where(o => o.OrganizationId == orgId.Value);
                    else
                        query = query.Where(o => false);
                }

                // Apply filters
                if (filters != null && filters.Count > 0)
                {
                    if (filters.ContainsKey("OrganizationName") && !string.IsNullOrWhiteSpace(filters["OrganizationName"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["OrganizationName"]);
                        query = query.Where(o => o.OrganizationName != null && o.OrganizationName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("OrganizationCode") && !string.IsNullOrWhiteSpace(filters["OrganizationCode"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["OrganizationCode"]);
                        query = query.Where(o => o.OrganizationCode != null && o.OrganizationCode.Contains(filterValue));
                    }

                    if (filters.ContainsKey("Address") && !string.IsNullOrWhiteSpace(filters["Address"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["Address"]);
                        query = query.Where(o => o.Address != null &&
                            ((o.Address.AddressLine1 != null && o.Address.AddressLine1.Contains(filterValue)) ||
                            (o.Address.City != null && o.Address.City.Contains(filterValue)) ||
                            (o.Address.PostalCode != null && o.Address.PostalCode.Contains(filterValue))));
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

        /// <summary>
        /// Saves logo to wwwroot/uploads/logos as organization_{OrganizationId}.{ext}.
        /// Returns relative path (e.g. uploads/logos/organization_1.png) or null on failure.
        /// </summary>
        private async Task<string?> SaveLogoToFileSystemAsync(byte[] logoBytes, long organizationId, string? originalFileName)
        {
            if (string.IsNullOrEmpty(_env?.WebRootPath))
                return null;

            var ext = Path.GetExtension(originalFileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !AllowedLogoExtensions.Contains(ext))
                ext = ".png";

            var fileName = $"organization_{organizationId}{ext}";
            var folderPath = Path.Combine(_env.WebRootPath, "uploads", "logos");
            Directory.CreateDirectory(folderPath);
            var filePath = Path.Combine(folderPath, fileName);

            await System.IO.File.WriteAllBytesAsync(filePath, logoBytes);
            return $"{LogoFolderRelativePath}/{fileName}";
        }
    }
}