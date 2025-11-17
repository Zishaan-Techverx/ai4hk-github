using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
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
                var exists = await _context.Farms.AnyAsync(f => f.FarmId == farm.FarmId);
                if (!exists)
                {
                    response.Success = false;
                    response.Message = "Farm not found";
                    return response;
                }

                _context.Update(farm);
                await _context.SaveChangesAsync();
                response.Data = farm;
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

        public async Task<ServiceResponse<(SelectList AreaTypes, SelectList Organizations)>> GetDropdownDataAsync()
        {
            var response = new ServiceResponse<(SelectList AreaTypes, SelectList Organizations)>();
            try
            {
                var areaTypes = await _context.AreaTypes.ToListAsync();
                var orgs = await _context.Organizations.ToListAsync();

                response.Data = (
                    new SelectList(areaTypes, "AreaTypeId", "AreaTypeId"),
                    new SelectList(orgs, "OrganizationId", "OrganizationId")
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
