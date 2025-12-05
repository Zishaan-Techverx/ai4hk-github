using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TpaSodManagement.Data;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using TpaSodManagement.Areas.Identity.Data;

namespace TpaSodManagement.Services.Implementations
{
    public class FieldService : IFieldService
    {
        private readonly SodDbContext _context;
        private readonly UserManager<TpaSodManagementUser> _userManager;

        public FieldService(SodDbContext context, UserManager<TpaSodManagementUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ServiceResponse<List<Field>>> GetAllAsync()
        {
            var response = new ServiceResponse<List<Field>>();
            try
            {
                response.Data = await _context.Fields
                    .Include(f => f.Farm)
                        .ThenInclude(f => f.Organization)
                    .Include(f => f.AreaType)
                    .Include(f => f.CreatedByUser)
                        .ThenInclude(u => u.Person)
                    .OrderBy(f => f.FieldName)
                    .ToListAsync();
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching fields: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<Field>> GetByIdAsync(long id)
        {
            var response = new ServiceResponse<Field>();
            try
            {
                var field = await _context.Fields
                    .Include(f => f.Farm)
                        .ThenInclude(f => f.Organization)
                    .Include(f => f.AreaType)
                    .Include(f => f.CreatedByUser)
                        .ThenInclude(u => u.Person)
                    .FirstOrDefaultAsync(f => f.FieldId == id);

                if (field == null)
                {
                    response.Success = false;
                    response.Message = "Field not found";
                }
                else
                {
                    response.Data = field;
                }
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching field: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<Field>> CreateAsync(Field field)
        {
            var response = new ServiceResponse<Field>();
            try
            {
                field.CreatedDate = System.DateTimeOffset.Now;
                _context.Fields.Add(field);
                await _context.SaveChangesAsync();
                response.Data = field;
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error creating field: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<Field>> UpdateAsync(Field field)
        {
            var response = new ServiceResponse<Field>();
            try
            {
                var exists = await _context.Fields.AnyAsync(f => f.FieldId == field.FieldId);
                if (!exists)
                {
                    response.Success = false;
                    response.Message = "Field not found";
                    return response;
                }

                _context.Fields.Update(field);
                await _context.SaveChangesAsync();
                response.Data = field;
            }
            catch (DbUpdateConcurrencyException)
            {
                response.Success = false;
                response.Message = "Concurrency error updating field";
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error updating field: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteAsync(long id)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var field = await _context.Fields.FindAsync(id);
                if (field == null)
                {
                    response.Success = false;
                    response.Message = "Field not found";
                    response.Data = false;
                    return response;
                }

                _context.Fields.Remove(field);
                await _context.SaveChangesAsync();
                response.Data = true;
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error deleting field: {ex.Message}";
                response.Data = false;
            }
            return response;
        }

        public async Task<ServiceResponse<(SelectList Farms, SelectList AreaTypes, SelectList Users)>> GetDropdownDataAsync()
        {
            var response = new ServiceResponse<(SelectList, SelectList, SelectList)>();
            try
            {
                var farms = await _context.Farms
                    .Include(f => f.Organization)
                    .OrderBy(f => f.FarmId)
                    .ToListAsync();

                var areaTypes = await _context.AreaTypes
                    .OrderBy(a => a.AreaTypeName)
                    .ToListAsync();

                // TpaSodManagementUser se users fetch karein
                var users = await _userManager.Users
                    .OrderBy(u => u.UserName)
                    .ToListAsync();

                var farmItems = farms.Select(f => new SelectListItem
                {
                    Value = f.FarmId.ToString(),
                    Text = !string.IsNullOrEmpty(f.LicenseNumber)
                        ? $"{f.LicenseNumber} ({(f.Organization != null ? f.Organization.OrganizationName : "N/A")})"
                        : $"Farm #{f.FarmId} ({(f.Organization != null ? f.Organization.OrganizationName : "N/A")})"
                }).ToList();

                var areaTypeItems = areaTypes.Select(a => new SelectListItem
                {
                    Value = a.AreaTypeId.ToString(),
                    Text = a.AreaTypeName
                }).ToList();

                // Create SelectList for Users with display name (FirstName LastName or UserName)
                var userItems = users.Select(u => new SelectListItem
                {
                    Value = u.Id, // TpaSodManagementUser ka Id string type hai
                    Text = !string.IsNullOrEmpty(u.FirstName) && !string.IsNullOrEmpty(u.LastName)
                        ? $"{u.FirstName} {u.LastName} ({u.UserName})"
                        : u.UserName ?? $"User #{u.Id}"
                }).ToList();

                response.Data = (
                    new SelectList(farmItems, "Value", "Text"),
                    new SelectList(areaTypeItems, "Value", "Text"),
                    new SelectList(userItems, "Value", "Text")
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
                response.Data = await _context.Fields.AnyAsync(f => f.FieldId == id);
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error checking field existence: {ex.Message}";
            }
            return response;
        }
    }
}

