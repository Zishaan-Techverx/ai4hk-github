using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Services.Implementations
{
    public class FarmService : IFarmService
    {
        private readonly SodDbContext _context;

        public FarmService(SodDbContext context)
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

        public async Task<ServiceResponse<bool>> DeleteAsync(long id)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var farm = await _context.Farms.FindAsync(id);
                if (farm == null)
                {
                    response.Success = false;
                    response.Message = "Farm not found";
                    response.Data = false;
                    return response;
                }

                _context.Farms.Remove(farm);
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
