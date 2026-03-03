using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Database;
using TpaSodManagement.Database.Entities;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.Utilities;

namespace TpaSodManagement.Services.Implementations
{
    public class SaleTypeService : ISaleTypeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public SaleTypeService(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<ServiceResponse<List<SaleType>>> GetAllAsync()
        {
            var response = new ServiceResponse<List<SaleType>>();
            try
            {
                response.Data = await _context.SaleTypes
                    .Include(s => s.CertificateType)
                    .OrderBy(s => s.SaleTypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching sale types: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<List<SaleType>>> GetFilteredAsync(Dictionary<string, string> filters)
        {
            var response = new ServiceResponse<List<SaleType>>();
            try
            {
                var query = _context.SaleTypes.Include(s => s.CertificateType).AsQueryable();

                if (filters != null && filters.Count > 0)
                {
                    if (filters.ContainsKey("SaleTypeCode") && !string.IsNullOrWhiteSpace(filters["SaleTypeCode"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["SaleTypeCode"]);
                        query = query.Where(s => s.SaleTypeCode != null && s.SaleTypeCode.Contains(filterValue));
                    }
                    if (filters.ContainsKey("SaleTypeName") && !string.IsNullOrWhiteSpace(filters["SaleTypeName"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["SaleTypeName"]);
                        query = query.Where(s => s.SaleTypeName != null && s.SaleTypeName.Contains(filterValue));
                    }
                    if (filters.ContainsKey("RequiresCertificate") && !string.IsNullOrWhiteSpace(filters["RequiresCertificate"]))
                    {
                        if (bool.TryParse(filters["RequiresCertificate"], out var val))
                            query = query.Where(s => s.RequiresCertificate == val);
                        else if (filters["RequiresCertificate"].ToLower() == "true" || filters["RequiresCertificate"].ToLower() == "yes")
                            query = query.Where(s => s.RequiresCertificate == true);
                        else if (filters["RequiresCertificate"].ToLower() == "false" || filters["RequiresCertificate"].ToLower() == "no")
                            query = query.Where(s => s.RequiresCertificate == false);
                    }
                    if (filters.ContainsKey("CertificateTypeName") && !string.IsNullOrWhiteSpace(filters["CertificateTypeName"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["CertificateTypeName"]);
                        query = query.Where(s => s.CertificateType != null && s.CertificateType.CertificateTypeName != null && s.CertificateType.CertificateTypeName.Contains(filterValue));
                    }
                    if (filters.ContainsKey("TaxApplicable") && !string.IsNullOrWhiteSpace(filters["TaxApplicable"]))
                    {
                        if (bool.TryParse(filters["TaxApplicable"], out var val))
                            query = query.Where(s => s.TaxApplicable == val);
                        else if (filters["TaxApplicable"].ToLower() == "true" || filters["TaxApplicable"].ToLower() == "yes")
                            query = query.Where(s => s.TaxApplicable == true);
                        else if (filters["TaxApplicable"].ToLower() == "false" || filters["TaxApplicable"].ToLower() == "no")
                            query = query.Where(s => s.TaxApplicable == false);
                    }
                    if (filters.ContainsKey("IsActive") && !string.IsNullOrWhiteSpace(filters["IsActive"]))
                    {
                        if (bool.TryParse(filters["IsActive"], out var isActiveValue))
                            query = query.Where(s => s.IsActive == isActiveValue);
                        else if (filters["IsActive"].ToLower() == "true" || filters["IsActive"].ToLower() == "yes" || filters["IsActive"].ToLower() == "1")
                            query = query.Where(s => s.IsActive == true);
                        else if (filters["IsActive"].ToLower() == "false" || filters["IsActive"].ToLower() == "no" || filters["IsActive"].ToLower() == "0")
                            query = query.Where(s => s.IsActive == false);
                    }
                }

                response.Data = await query.OrderBy(s => s.SaleTypeName).ToListAsync();
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching filtered sale types: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<SaleType>> GetByIdAsync(int id)
        {
            var response = new ServiceResponse<SaleType>();
            try
            {
                var entity = await _context.SaleTypes
                    .Include(s => s.CertificateType)
                    .FirstOrDefaultAsync(s => s.SaleTypeId == id);
                if (entity == null)
                {
                    response.Success = false;
                    response.Message = "Sale type not found";
                }
                else
                    response.Data = entity;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching sale type: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<SaleType>> CreateAsync(SaleType saleType)
        {
            var response = new ServiceResponse<SaleType>();
            try
            {
                var exists = await _context.SaleTypes
                    .AnyAsync(s => s.SaleTypeCode.ToUpper() == saleType.SaleTypeCode.ToUpper());
                if (exists)
                {
                    response.Success = false;
                    response.Message = "Sale type code already exists";
                    return response;
                }
                var currentUserId = await _currentUserService.GetCurrentUserIdAsync();
                saleType.CreatedDate = DateTimeOffset.UtcNow;
                saleType.CreatedByUserId = currentUserId;
                _context.Add(saleType);
                await _context.SaveChangesAsync();
                response.Data = saleType;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error creating sale type: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<SaleType>> UpdateAsync(SaleType saleType)
        {
            var response = new ServiceResponse<SaleType>();
            try
            {
                var existing = await _context.SaleTypes.FirstOrDefaultAsync(s => s.SaleTypeId == saleType.SaleTypeId);
                if (existing == null)
                {
                    response.Success = false;
                    response.Message = "Sale type not found";
                    return response;
                }
                var codeExists = await _context.SaleTypes
                    .AnyAsync(s => s.SaleTypeCode.ToUpper() == saleType.SaleTypeCode.ToUpper() && s.SaleTypeId != saleType.SaleTypeId);
                if (codeExists)
                {
                    response.Success = false;
                    response.Message = "Sale type code already exists";
                    return response;
                }
                existing.SaleTypeCode = saleType.SaleTypeCode;
                existing.SaleTypeName = saleType.SaleTypeName;
                existing.RequiresCertificate = saleType.RequiresCertificate;
                existing.CertificateTypeId = saleType.CertificateTypeId;
                existing.TaxApplicable = saleType.TaxApplicable;
                existing.Description = saleType.Description;
                existing.IsActive = saleType.IsActive;
                var currentUserId = await _currentUserService.GetCurrentUserIdAsync();
                existing.UpdatedDate = DateTimeOffset.UtcNow;
                existing.UpdatedByUserId = currentUserId;
                await _context.SaveChangesAsync();
                response.Data = existing;
            }
            catch (DbUpdateConcurrencyException)
            {
                response.Success = false;
                response.Message = "Concurrency error updating sale type";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error updating sale type: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteAsync(int id, long? deletedByUserId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var entity = await _context.SaleTypes
                    .FirstOrDefaultAsync(s => s.SaleTypeId == id && s.DeletedDate == null);
                if (entity == null)
                {
                    response.Success = false;
                    response.Message = "Sale type not found";
                    response.Data = false;
                    return response;
                }
                var currentUserId = deletedByUserId ?? await _currentUserService.GetCurrentUserIdAsync();
                entity.DeletedDate = DateTimeOffset.UtcNow;
                entity.DeletedByUserId = currentUserId;
                await _context.SaveChangesAsync();
                response.Data = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error deleting sale type: {ex.Message}";
                response.Data = false;
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> ExisTpasync(int id)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                response.Data = await _context.SaleTypes.AnyAsync(s => s.SaleTypeId == id);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error checking sale type existence: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<SelectListItem>>> GetCertificateTypesForDropdownAsync()
        {
            var response = new ServiceResponse<IEnumerable<SelectListItem>>();
            try
            {
                var list = await _context.CertificateTypes
                    .Where(c => c.DeletedDate == null)
                    .OrderBy(c => c.CertificateTypeName)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CertificateTypeId.ToString(),
                        Text = c.CertificateTypeName ?? c.CertificateTypeCode ?? $"#{c.CertificateTypeId}"
                    })
                    .ToListAsync();
                response.Data = list;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching certificate types: {ex.Message}";
                response.Data = Enumerable.Empty<SelectListItem>();
            }
            return response;
        }
    }
}
