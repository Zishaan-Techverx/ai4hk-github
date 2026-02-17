using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Database;
using TpaSodManagement.Utilities;
using TpaSodManagement.Database.Entities;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Services.Implementations
{
    public class FarmService : IFarmService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public FarmService(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<ServiceResponse<List<Farm>>> GetAllAsync()
        {
            var response = new ServiceResponse<List<Farm>>();
            try
            {
                response.Data = await _context.Farms
                    .Include(f => f.AreaType)
                    .Include(f => f.Organization)
                    .Include(f => f.Address).ThenInclude(a => a!.StateProvince)
                    .ToListAsync();
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching farms: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<List<Farm>>> GetFilteredAsync(Dictionary<string, string> filters)
        {
            var response = new ServiceResponse<List<Farm>>();
            try
            {
                var query = _context.Farms
                    .Include(f => f.AreaType)
                    .Include(f => f.Organization)
                    .Include(f => f.Address).ThenInclude(a => a!.StateProvince)
                    .AsQueryable();

                // Apply filters
                if (filters != null && filters.Count > 0)
                {
                    if (filters.ContainsKey("FarmName") && !string.IsNullOrWhiteSpace(filters["FarmName"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["FarmName"]);
                        query = query.Where(f => f.FarmName != null && f.FarmName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("Address") && !string.IsNullOrWhiteSpace(filters["Address"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["Address"]);
                        query = query.Where(f => f.Address != null && (
                            (f.Address.AddressLine1 != null && f.Address.AddressLine1.Contains(filterValue)) ||
                            (f.Address.AddressLine2 != null && f.Address.AddressLine2.Contains(filterValue)) ||
                            (f.Address.City != null && f.Address.City.Contains(filterValue)) ||
                            (f.Address.PostalCode != null && f.Address.PostalCode.Contains(filterValue))));
                    }

                    if (filters.ContainsKey("TotalArea") && !string.IsNullOrWhiteSpace(filters["TotalArea"]))
                    {
                        if (decimal.TryParse(filters["TotalArea"], out decimal totalArea))
                        {
                            query = query.Where(f => f.TotalArea == totalArea);
                        }
                    }

                    if (filters.ContainsKey("OrganicCertified") && !string.IsNullOrWhiteSpace(filters["OrganicCertified"]))
                    {
                        if (bool.TryParse(filters["OrganicCertified"], out bool organicCertified))
                        {
                            query = query.Where(f => f.OrganicCertified == organicCertified);
                        }
                        else if (filters["OrganicCertified"].ToLower() == "true" || filters["OrganicCertified"].ToLower() == "yes" || filters["OrganicCertified"].ToLower() == "1")
                        {
                            query = query.Where(f => f.OrganicCertified == true);
                        }
                        else if (filters["OrganicCertified"].ToLower() == "false" || filters["OrganicCertified"].ToLower() == "no" || filters["OrganicCertified"].ToLower() == "0")
                        {
                            query = query.Where(f => f.OrganicCertified == false);
                        }
                    }

                    if (filters.ContainsKey("LicenseNumber") && !string.IsNullOrWhiteSpace(filters["LicenseNumber"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["LicenseNumber"]);
                        query = query.Where(f => f.LicenseNumber != null && f.LicenseNumber.Contains(filterValue));
                    }

                    if (filters.ContainsKey("CertificationDetails") && !string.IsNullOrWhiteSpace(filters["CertificationDetails"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["CertificationDetails"]);
                        query = query.Where(f => f.CertificationDetails != null && f.CertificationDetails.Contains(filterValue));
                    }

                    if (filters.ContainsKey("Latitude") && !string.IsNullOrWhiteSpace(filters["Latitude"]))
                    {
                        if (decimal.TryParse(filters["Latitude"], out decimal latitude))
                        {
                            query = query.Where(f => f.Latitude == latitude);
                        }
                    }

                    if (filters.ContainsKey("Longitude") && !string.IsNullOrWhiteSpace(filters["Longitude"]))
                    {
                        if (decimal.TryParse(filters["Longitude"], out decimal longitude))
                        {
                            query = query.Where(f => f.Longitude == longitude);
                        }
                    }

                    if (filters.ContainsKey("ElevationMeters") && !string.IsNullOrWhiteSpace(filters["ElevationMeters"]))
                    {
                        if (int.TryParse(filters["ElevationMeters"], out int elevation))
                        {
                            query = query.Where(f => f.ElevationMeters == elevation);
                        }
                    }

                    if (filters.ContainsKey("SoilType") && !string.IsNullOrWhiteSpace(filters["SoilType"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["SoilType"]);
                        query = query.Where(f => f.SoilType != null && f.SoilType.Contains(filterValue));
                    }

                    if (filters.ContainsKey("IrrigationType") && !string.IsNullOrWhiteSpace(filters["IrrigationType"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["IrrigationType"]);
                        query = query.Where(f => f.IrrigationType != null && f.IrrigationType.Contains(filterValue));
                    }

                    if (filters.ContainsKey("ClimateZone") && !string.IsNullOrWhiteSpace(filters["ClimateZone"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["ClimateZone"]);
                        query = query.Where(f => f.ClimateZone != null && f.ClimateZone.Contains(filterValue));
                    }

                    if (filters.ContainsKey("AreaType") && !string.IsNullOrWhiteSpace(filters["AreaType"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["AreaType"]);
                        query = query.Where(f => f.AreaType != null && f.AreaType.AreaTypeName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("IsActive") && !string.IsNullOrWhiteSpace(filters["IsActive"]))
                    {
                        if (bool.TryParse(filters["IsActive"], out bool isActiveValue))
                            query = query.Where(f => f.IsActive == isActiveValue);
                        else if (filters["IsActive"].ToLower() == "true" || filters["IsActive"].ToLower() == "yes" || filters["IsActive"].ToLower() == "1")
                            query = query.Where(f => f.IsActive == true);
                        else if (filters["IsActive"].ToLower() == "false" || filters["IsActive"].ToLower() == "no" || filters["IsActive"].ToLower() == "0")
                            query = query.Where(f => f.IsActive == false);
                    }
                }

                response.Data = await query.OrderBy(f => f.FarmId).ToListAsync();
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error filtering farms: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<Farm>> GetByIdAsync(long id)
        {
            var response = new ServiceResponse<Farm>();
            try
            {
                var farm = await _context.Farms
                    .Include(f => f.AreaType)
                    .Include(f => f.Organization)
                    .Include(f => f.Address).ThenInclude(a => a!.StateProvince)
                    .FirstOrDefaultAsync(f => f.FarmId == id);

                if (farm == null)
                {
                    response.Success = false;
                    response.Message = "Farm not found";
                }
                else
                {
                    response.Data = farm;
                }
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching farm: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<Farm>> CreateAsync(Farm farm)
        {
            var response = new ServiceResponse<Farm>();
            try
            {
                var currentUserId = await _currentUserService.GetCurrentUserIdAsync();
                farm.CreatedDate = DateTimeOffset.UtcNow;
                farm.CreatedByUserId = currentUserId;
                _context.Add(farm);
                await _context.SaveChangesAsync();
                response.Data = farm;
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error creating farm: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<Farm>> UpdateAsync(Farm farm)
        {
            var response = new ServiceResponse<Farm>();
            try
            {
                var existingFarm = await _context.Farms
                    .FirstOrDefaultAsync(f => f.FarmId == farm.FarmId);
                
                if (existingFarm == null)
                {
                    response.Success = false;
                    response.Message = "Farm not found";
                    return response;
                }

                // Update only the properties that are provided
                existingFarm.FarmName = farm.FarmName;
                existingFarm.AddressId = farm.AddressId;
                existingFarm.OrganizationId = farm.OrganizationId;
                existingFarm.TotalArea = farm.TotalArea;
                existingFarm.AreaTypeId = farm.AreaTypeId;
                existingFarm.LicenseNumber = farm.LicenseNumber;
                existingFarm.OrganicCertified = farm.OrganicCertified;
                existingFarm.CertificationDetails = farm.CertificationDetails;
                existingFarm.Latitude = farm.Latitude;
                existingFarm.Longitude = farm.Longitude;
                existingFarm.ElevationMeters = farm.ElevationMeters;
                existingFarm.SoilType = farm.SoilType;
                existingFarm.IrrigationType = farm.IrrigationType;
                existingFarm.ClimateZone = farm.ClimateZone;
                existingFarm.IsActive = farm.IsActive;

                var currentUserId = await _currentUserService.GetCurrentUserIdAsync();
                existingFarm.UpdatedDate = DateTimeOffset.UtcNow;
                existingFarm.UpdatedByUserId = currentUserId;

                await _context.SaveChangesAsync();
                response.Data = existingFarm;
            }
            catch (DbUpdateConcurrencyException)
            {
                response.Success = false;
                response.Message = "Concurrency error updating farm";
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error updating farm: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteAsync(long id, long? deletedByUserId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var farm = await _context.Farms
                    .FirstOrDefaultAsync(f => f.FarmId == id && f.DeletedDate == null);
                if (farm == null)
                {
                    response.Success = false;
                    response.Message = "Farm not found";
                    response.Data = false;
                    return response;
                }

                // Soft delete: Set DeletedDate and DeletedByUserId
                var currentUserId = deletedByUserId ?? await _currentUserService.GetCurrentUserIdAsync();
                farm.DeletedDate = DateTimeOffset.UtcNow;
                farm.DeletedByUserId = currentUserId;
                
                await _context.SaveChangesAsync();
                response.Data = true;
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error deleting farm: {ex.Message}";
                response.Data = false;
            }
            return response;
        }

        public async Task<ServiceResponse<(SelectList AreaTypes, SelectList Organizations)>> GetDropdownDataAsync(long? selectedOrganizationId = null, int? selectedAreaTypeId = null)
        {
            var response = new ServiceResponse<(SelectList AreaTypes, SelectList Organizations)>();
            try
            {
                var areaTypes = await _context.AreaTypes
                    .OrderBy(a => a.AreaTypeName)
                    .ToListAsync();

                var orgs = await _context.Organizations
                    .OrderBy(o => o.OrganizationName)
                    .ToListAsync();

                response.Data = (
                    new SelectList(areaTypes, "AreaTypeId", "AreaTypeName", selectedAreaTypeId), 
                    new SelectList(orgs, "OrganizationId", "OrganizationName", selectedOrganizationId) 
                );
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching dropdowns: {ex.Message}";
            }
            return response;
        }
    }
}
