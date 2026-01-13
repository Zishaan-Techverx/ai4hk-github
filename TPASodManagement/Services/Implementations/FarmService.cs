using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Database;
using TpaSodManagement.Database.Entities;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Services.Implementations
{
    public class FarmService : IFarmService
    {
        private readonly ApplicationDbContext _context;

        public FarmService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<List<Farm>>> GetAllAsync()
        {
            var response = new ServiceResponse<List<Farm>>();
            try
            {
                response.Data = await _context.Farms
                    .Include(f => f.AreaType)
                    .Include(f => f.Organization)
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
                    .AsQueryable();

                // Apply filters
                if (filters != null && filters.Count > 0)
                {
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
                        var filterValue = filters["LicenseNumber"].Trim();
                        query = query.Where(f => f.LicenseNumber != null && f.LicenseNumber.Contains(filterValue));
                    }

                    if (filters.ContainsKey("CertificationDetails") && !string.IsNullOrWhiteSpace(filters["CertificationDetails"]))
                    {
                        var filterValue = filters["CertificationDetails"].Trim();
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
                        var filterValue = filters["SoilType"].Trim();
                        query = query.Where(f => f.SoilType != null && f.SoilType.Contains(filterValue));
                    }

                    if (filters.ContainsKey("IrrigationType") && !string.IsNullOrWhiteSpace(filters["IrrigationType"]))
                    {
                        var filterValue = filters["IrrigationType"].Trim();
                        query = query.Where(f => f.IrrigationType != null && f.IrrigationType.Contains(filterValue));
                    }

                    if (filters.ContainsKey("ClimateZone") && !string.IsNullOrWhiteSpace(filters["ClimateZone"]))
                    {
                        var filterValue = filters["ClimateZone"].Trim();
                        query = query.Where(f => f.ClimateZone != null && f.ClimateZone.Contains(filterValue));
                    }

                    if (filters.ContainsKey("AreaType") && !string.IsNullOrWhiteSpace(filters["AreaType"]))
                    {
                        var filterValue = filters["AreaType"].Trim();
                        query = query.Where(f => f.AreaType != null && f.AreaType.AreaTypeName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("Organization") && !string.IsNullOrWhiteSpace(filters["Organization"]))
                    {
                        var filterValue = filters["Organization"].Trim();
                        query = query.Where(f => f.Organization != null && f.Organization.OrganizationName.Contains(filterValue));
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
                farm.DeletedDate = DateTimeOffset.UtcNow;
                farm.DeletedByUserId = deletedByUserId;
                
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
