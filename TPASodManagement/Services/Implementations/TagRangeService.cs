using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Database;
using TpaSodManagement.Database.Entities;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.Utilities;

namespace TpaSodManagement.Services.Implementations
{
    public class TagRangeService : ITagRangeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public TagRangeService(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<ServiceResponse<List<TagRange>>> GetAllAsync()
        {
            var response = new ServiceResponse<List<TagRange>>();
            try
            {
                var query = _context.TagRanges
                    .Include(t => t.Farm)
                    .AsQueryable();

                if (!await _currentUserService.IsCurrentUserSuperAdminAsync())
                {
                    var farmId = await _currentUserService.GetCurrentUserFarmIdAsync();
                    if (farmId.HasValue)
                        query = query.Where(t => t.FarmId == farmId.Value);
                    else
                        query = query.Where(_ => false);
                }

                response.Data = await query
                    .OrderBy(t => t.TagRangeCode)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching tag ranges: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<List<TagRange>>> GetFilteredAsync(Dictionary<string, string> filters)
        {
            var response = new ServiceResponse<List<TagRange>>();
            try
            {
                var query = _context.TagRanges
                    .Include(t => t.Farm)
                    .AsQueryable();

                if (!await _currentUserService.IsCurrentUserSuperAdminAsync())
                {
                    var farmId = await _currentUserService.GetCurrentUserFarmIdAsync();
                    if (farmId.HasValue)
                        query = query.Where(t => t.FarmId == farmId.Value);
                    else
                        query = query.Where(_ => false);
                }

                if (filters != null && filters.Count > 0)
                {
                    if (filters.ContainsKey("TagRangeCode") && !string.IsNullOrWhiteSpace(filters["TagRangeCode"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["TagRangeCode"]);
                        query = query.Where(t => t.TagRangeCode != null && t.TagRangeCode.Contains(filterValue));
                    }
                    if (filters.ContainsKey("TagStartNumber") && !string.IsNullOrWhiteSpace(filters["TagStartNumber"]))
                    {
                        if (long.TryParse(filters["TagStartNumber"], out var val))
                            query = query.Where(t => t.TagStartNumber == val);
                    }
                    if (filters.ContainsKey("TagEndNumber") && !string.IsNullOrWhiteSpace(filters["TagEndNumber"]))
                    {
                        if (long.TryParse(filters["TagEndNumber"], out var val))
                            query = query.Where(t => t.TagEndNumber == val);
                    }
                    if (filters.ContainsKey("FarmName") && !string.IsNullOrWhiteSpace(filters["FarmName"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["FarmName"]);
                        query = query.Where(t => t.Farm != null && t.Farm.FarmName != null && t.Farm.FarmName.Contains(filterValue));
                    }
                    if (filters.ContainsKey("SeedType") && !string.IsNullOrWhiteSpace(filters["SeedType"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["SeedType"]);
                        query = query.Where(t => t.SeedType.ToString().Contains(filterValue));
                    }
                    if (filters.ContainsKey("TotalTags") && !string.IsNullOrWhiteSpace(filters["TotalTags"]))
                    {
                        if (int.TryParse(filters["TotalTags"], out var val))
                            query = query.Where(t => t.TotalTags == val);
                    }
                    if (filters.ContainsKey("IsActive") && !string.IsNullOrWhiteSpace(filters["IsActive"]))
                    {
                        if (bool.TryParse(filters["IsActive"], out var isActiveValue))
                            query = query.Where(t => t.IsActive == isActiveValue);
                        else if (filters["IsActive"].ToLower() == "true" || filters["IsActive"].ToLower() == "yes" || filters["IsActive"].ToLower() == "1")
                            query = query.Where(t => t.IsActive == true);
                        else if (filters["IsActive"].ToLower() == "false" || filters["IsActive"].ToLower() == "no" || filters["IsActive"].ToLower() == "0")
                            query = query.Where(t => t.IsActive == false);
                    }
                }

                response.Data = await query.OrderBy(t => t.TagRangeCode).ToListAsync();
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching filtered tag ranges: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<TagRange>> GetByIdAsync(long id)
        {
            var response = new ServiceResponse<TagRange>();
            try
            {
                var entity = await _context.TagRanges.FindAsync(id);
                if (entity == null)
                {
                    response.Success = false;
                    response.Message = "Tag range not found";
                }
                else
                    response.Data = entity;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching tag range: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<TagRange>> CreateAsync(TagRange tagRange)
        {
            var response = new ServiceResponse<TagRange>();
            try
            {
                var exists = await _context.TagRanges
                    .AnyAsync(t => t.TagRangeCode.ToUpper() == tagRange.TagRangeCode.ToUpper());
                if (exists)
                {
                    response.Success = false;
                    response.Message = "Tag range code already exists";
                    return response;
                }
                tagRange.TotalTags = (int)Math.Max(0, tagRange.TagEndNumber - tagRange.TagStartNumber + 1);
                var currentUserId = await _currentUserService.GetCurrentUserIdAsync();
                tagRange.CreatedDate = DateTimeOffset.UtcNow;
                tagRange.CreatedByUserId = currentUserId;
                _context.Add(tagRange);
                await _context.SaveChangesAsync();
                response.Data = tagRange;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error creating tag range: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<TagRange>> UpdateAsync(TagRange tagRange)
        {
            var response = new ServiceResponse<TagRange>();
            try
            {
                var existing = await _context.TagRanges.FirstOrDefaultAsync(t => t.TagRangeId == tagRange.TagRangeId);
                if (existing == null)
                {
                    response.Success = false;
                    response.Message = "Tag range not found";
                    return response;
                }
                var nameExists = await _context.TagRanges
                    .AnyAsync(t => t.TagRangeCode.ToUpper() == tagRange.TagRangeCode.ToUpper() && t.TagRangeId != tagRange.TagRangeId);
                if (nameExists)
                {
                    response.Success = false;
                    response.Message = "Tag range code already exists";
                    return response;
                }
                existing.TagRangeCode = tagRange.TagRangeCode;
                existing.TagStartNumber = tagRange.TagStartNumber;
                existing.TagEndNumber = tagRange.TagEndNumber;
                existing.FarmId = tagRange.FarmId;
                existing.SeedType = tagRange.SeedType;
                existing.TotalTags = (int)Math.Max(0, tagRange.TagEndNumber - tagRange.TagStartNumber + 1);
                existing.IsActive = tagRange.IsActive;
                var currentUserId = await _currentUserService.GetCurrentUserIdAsync();
                existing.UpdatedDate = DateTimeOffset.UtcNow;
                existing.UpdatedByUserId = currentUserId;
                await _context.SaveChangesAsync();
                response.Data = existing;
            }
            catch (DbUpdateConcurrencyException)
            {
                response.Success = false;
                response.Message = "Concurrency error updating tag range";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error updating tag range: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteAsync(long id, long? deletedByUserId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var entity = await _context.TagRanges
                    .FirstOrDefaultAsync(t => t.TagRangeId == id && t.DeletedDate == null);
                if (entity == null)
                {
                    response.Success = false;
                    response.Message = "Tag range not found";
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
                response.Message = $"Error deleting tag range: {ex.Message}";
                response.Data = false;
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> ExisTpasync(long id)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                response.Data = await _context.TagRanges.AnyAsync(t => t.TagRangeId == id);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error checking tag range existence: {ex.Message}";
            }
            return response;
        }
    }
}
