using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TpaSodManagement.Data;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Services.Implementations
{
    public class AreaTypeService : IAreaTypeService
    {
        private readonly SodDbContext _context;

        public AreaTypeService(SodDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<List<AreaType>>> GetAllAsync()
        {
            var response = new ServiceResponse<List<AreaType>>();
            try
            {
                response.Data = await _context.AreaTypes
                    .OrderBy(a => a.AreaTypeName)
                    .ToListAsync();
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching area types: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<List<AreaType>>> GetFilteredAsync(Dictionary<string, string> filters)
        {
            var response = new ServiceResponse<List<AreaType>>();
            try
            {
                var query = _context.AreaTypes.AsQueryable();

                // Apply filters
                if (filters != null && filters.Count > 0)
                {
                    if (filters.ContainsKey("AreaTypeName") && !string.IsNullOrWhiteSpace(filters["AreaTypeName"]))
                    {
                        var filterValue = filters["AreaTypeName"].Trim();
                        query = query.Where(a => a.AreaTypeName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("UnitAbbreviation") && !string.IsNullOrWhiteSpace(filters["UnitAbbreviation"]))
                    {
                        var filterValue = filters["UnitAbbreviation"].Trim();
                        query = query.Where(a => a.UnitAbbreviation != null && a.UnitAbbreviation.Contains(filterValue));
                    }

                    if (filters.ContainsKey("UnitSystem") && !string.IsNullOrWhiteSpace(filters["UnitSystem"]))
                    {
                        var filterValue = filters["UnitSystem"].Trim();
                        query = query.Where(a => a.UnitSystem != null && a.UnitSystem.Contains(filterValue));
                    }

                    if (filters.ContainsKey("ConversionToSquareMeters") && !string.IsNullOrWhiteSpace(filters["ConversionToSquareMeters"]))
                    {
                        if (decimal.TryParse(filters["ConversionToSquareMeters"], out var conversionValue))
                        {
                            query = query.Where(a => a.ConversionToSquareMeters == conversionValue);
                        }
                    }

                    if (filters.ContainsKey("IsActive") && !string.IsNullOrWhiteSpace(filters["IsActive"]))
                    {
                        if (bool.TryParse(filters["IsActive"], out var isActiveValue))
                        {
                            query = query.Where(a => a.IsActive == isActiveValue);
                        }
                        else if (filters["IsActive"].ToLower() == "true" || filters["IsActive"].ToLower() == "yes" || filters["IsActive"].ToLower() == "1")
                        {
                            query = query.Where(a => a.IsActive == true);
                        }
                        else if (filters["IsActive"].ToLower() == "false" || filters["IsActive"].ToLower() == "no" || filters["IsActive"].ToLower() == "0")
                        {
                            query = query.Where(a => a.IsActive == false);
                        }
                    }
                }

                response.Data = await query.OrderBy(a => a.AreaTypeName).ToListAsync();
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching filtered area types: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<AreaType>> GetByIdAsync(int id)
        {
            var response = new ServiceResponse<AreaType>();
            try
            {
                var areaType = await _context.AreaTypes.FindAsync(id);

                if (areaType == null)
                {
                    response.Success = false;
                    response.Message = "Area type not found";
                }
                else
                {
                    response.Data = areaType;
                }
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching area type: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<AreaType>> CreateAsync(AreaType areaType)
        {
            var response = new ServiceResponse<AreaType>();
            try
            {
                // Check if area type name already exists
                var exists = await _context.AreaTypes
                    .AnyAsync(a => a.AreaTypeName.ToUpper() == areaType.AreaTypeName.ToUpper());

                if (exists)
                {
                    response.Success = false;
                    response.Message = "Area type name already exists";
                    return response;
                }

                areaType.CreatedDate = System.DateTimeOffset.UtcNow;
                _context.Add(areaType);
                await _context.SaveChangesAsync();
                response.Data = areaType;
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error creating area type: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<AreaType>> UpdateAsync(AreaType areaType)
        {
            var response = new ServiceResponse<AreaType>();
            try
            {
                var exists = await _context.AreaTypes.AnyAsync(a => a.AreaTypeId == areaType.AreaTypeId);
                if (!exists)
                {
                    response.Success = false;
                    response.Message = "Area type not found";
                    return response;
                }

                // Check if area type name already exists (excluding current area type)
                var nameExists = await _context.AreaTypes
                    .AnyAsync(a => a.AreaTypeName.ToUpper() == areaType.AreaTypeName.ToUpper() 
                        && a.AreaTypeId != areaType.AreaTypeId);

                if (nameExists)
                {
                    response.Success = false;
                    response.Message = "Area type name already exists";
                    return response;
                }

                _context.Update(areaType);
                await _context.SaveChangesAsync();
                response.Data = areaType;
            }
            catch (DbUpdateConcurrencyException)
            {
                response.Success = false;
                response.Message = "Concurrency error updating area type";
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error updating area type: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteAsync(int id)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var areaType = await _context.AreaTypes
                    .Include(a => a.Farms)
                    .Include(a => a.Fields)
                    .Include(a => a.SaleLineItems)
                    .Include(a => a.Seedings)
                    .Include(a => a.Wastes)
                    .FirstOrDefaultAsync(a => a.AreaTypeId == id);

                if (areaType == null)
                {
                    response.Success = false;
                    response.Message = "Area type not found";
                    response.Data = false;
                    return response;
                }

                // Check if area type is used in any farms
                if (areaType.Farms != null && areaType.Farms.Any())
                {
                    response.Success = false;
                    response.Message = "Cannot delete area type as it is being used in farms";
                    response.Data = false;
                    return response;
                }

                // Check if area type is used in any fields
                if (areaType.Fields != null && areaType.Fields.Any())
                {
                    response.Success = false;
                    response.Message = "Cannot delete area type as it is being used in fields";
                    response.Data = false;
                    return response;
                }

                // Check if area type is used in any sale line items
                if (areaType.SaleLineItems != null && areaType.SaleLineItems.Any())
                {
                    response.Success = false;
                    response.Message = "Cannot delete area type as it is being used in sale line items";
                    response.Data = false;
                    return response;
                }

                // Check if area type is used in any seedings
                if (areaType.Seedings != null && areaType.Seedings.Any())
                {
                    response.Success = false;
                    response.Message = "Cannot delete area type as it is being used in seedings";
                    response.Data = false;
                    return response;
                }

                // Check if area type is used in any wastes
                if (areaType.Wastes != null && areaType.Wastes.Any())
                {
                    response.Success = false;
                    response.Message = "Cannot delete area type as it is being used in wastes";
                    response.Data = false;
                    return response;
                }

                _context.AreaTypes.Remove(areaType);
                await _context.SaveChangesAsync();
                response.Data = true;
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error deleting area type: {ex.Message}";
                response.Data = false;
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> ExisTpasync(int id)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                response.Data = await _context.AreaTypes.AnyAsync(a => a.AreaTypeId == id);
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error checking area type existence: {ex.Message}";
            }
            return response;
        }
    }
}
