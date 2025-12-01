using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using TpaSodManagement.Data;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Services.Implementations
{
    public class SeedingService : ISeedingService
    {
        private readonly SodDbContext _context;

        public SeedingService(SodDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<List<Seeding>>> GetAllAsync()
        {
            var response = new ServiceResponse<List<Seeding>>();
            try
            {
                response.Data = await _context.Seedings
                    .Include(s => s.AreaType)
                    .Include(s => s.Farm)
                    .Include(s => s.Field)
                    .Include(s => s.TagRange)
                    .Include(s => s.User)
                    .ToListAsync();
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching seedings: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<Seeding>> GetByIdAsync(long id)
        {
            var response = new ServiceResponse<Seeding>();
            try
            {
                var seeding = await _context.Seedings
                    .Include(s => s.AreaType)
                    .Include(s => s.Farm)
                    .Include(s => s.Field)
                    .Include(s => s.TagRange)
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.SeedingId == id);

                if (seeding == null)
                {
                    response.Success = false;
                    response.Message = "Seeding not found";
                }
                else
                {
                    response.Data = seeding;
                }
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching seeding: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<Seeding>> CreateAsync(Seeding seeding)
        {
            var response = new ServiceResponse<Seeding>();
            try
            {
                _context.Seedings.Add(seeding);
                await _context.SaveChangesAsync();
                response.Data = seeding;
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error creating seeding: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<Seeding>> UpdateAsync(Seeding seeding)
        {
            var response = new ServiceResponse<Seeding>();
            try
            {
                var exists = await _context.Seedings.AnyAsync(s => s.SeedingId == seeding.SeedingId);
                if (!exists)
                {
                    response.Success = false;
                    response.Message = "Seeding not found";
                    return response;
                }

                _context.Seedings.Update(seeding);
                await _context.SaveChangesAsync();
                response.Data = seeding;
            }
            catch (DbUpdateConcurrencyException)
            {
                response.Success = false;
                response.Message = "Concurrency error updating seeding";
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error updating seeding: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteAsync(long id)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var seeding = await _context.Seedings.FindAsync(id);
                if (seeding == null)
                {
                    response.Success = false;
                    response.Message = "Seeding not found";
                    response.Data = false;
                    return response;
                }

                _context.Seedings.Remove(seeding);
                await _context.SaveChangesAsync();
                response.Data = true;
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error deleting seeding: {ex.Message}";
                response.Data = false;
            }
            return response;
        }

        public async Task<ServiceResponse<(SelectList AreaTypes, SelectList Farms, SelectList Fields, SelectList TagRanges, SelectList Users)>> GetDropdownDataAsync()
        {
            var response = new ServiceResponse<(SelectList, SelectList, SelectList, SelectList, SelectList)>();
            try
            {
                var areaTypes = await _context.AreaTypes.ToListAsync();
                var farms = await _context.Farms.ToListAsync();
                var fields = await _context.Fields.ToListAsync();
                var tagRanges = await _context.TagRanges.ToListAsync();
                var users = await _context.TpaUsers.ToListAsync();

                response.Data = (
                    new SelectList(areaTypes, "AreaTypeId", "AreaTypeId"),
                    new SelectList(farms, "FarmId", "FarmId"),
                    new SelectList(fields, "FieldId", "FieldId"),
                    new SelectList(tagRanges, "TagRangeId", "TagRangeId"),
                    new SelectList(users, "UserId", "UserId")
                );
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching dropdowns: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> ExisTpasync(long id)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                response.Data = await _context.Seedings.AnyAsync(s => s.SeedingId == id);
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error checking seeding existence: {ex.Message}";
            }
            return response;
        }
    }
}
