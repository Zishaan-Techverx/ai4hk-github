using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Database.Entities;
using TpaSodManagement.Database;


namespace TpaSodManagement.Services.Implementations
{
    public class FieldService : IFieldService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<TpaSodManagementUser> _userManager;

        public FieldService(ApplicationDbContext context, UserManager<TpaSodManagementUser> userManager)
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

        public async Task<ServiceResponse<List<Field>>> GetFilteredAsync(Dictionary<string, string> filters)
        {
            var response = new ServiceResponse<List<Field>>();
            try
            {
                var query = _context.Fields
                    .Include(f => f.Farm)
                        .ThenInclude(f => f.Organization)
                    .Include(f => f.AreaType)
                    .AsQueryable();

                // Apply filters
                if (filters != null && filters.Count > 0)
                {
                    if (filters.ContainsKey("FieldName") && !string.IsNullOrWhiteSpace(filters["FieldName"]))
                    {
                        var filterValue = filters["FieldName"].Trim();
                        query = query.Where(f => f.FieldName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("FieldCode") && !string.IsNullOrWhiteSpace(filters["FieldCode"]))
                    {
                        var filterValue = filters["FieldCode"].Trim();
                        query = query.Where(f => f.FieldCode != null && f.FieldCode.Contains(filterValue));
                    }

                    if (filters.ContainsKey("AreaAmount") && !string.IsNullOrWhiteSpace(filters["AreaAmount"]))
                    {
                        if (decimal.TryParse(filters["AreaAmount"], out decimal areaAmount))
                        {
                            query = query.Where(f => f.AreaAmount == areaAmount);
                        }
                    }

                    if (filters.ContainsKey("AreaTypeName") && !string.IsNullOrWhiteSpace(filters["AreaTypeName"]))
                    {
                        var filterValue = filters["AreaTypeName"].Trim();
                        query = query.Where(f => f.AreaType != null && f.AreaType.AreaTypeName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("FarmLicenseNumber") && !string.IsNullOrWhiteSpace(filters["FarmLicenseNumber"]))
                    {
                        var filterValue = filters["FarmLicenseNumber"].Trim();
                        query = query.Where(f => f.Farm != null && f.Farm.LicenseNumber != null && f.Farm.LicenseNumber.Contains(filterValue));
                    }

                    if (filters.ContainsKey("SoilType") && !string.IsNullOrWhiteSpace(filters["SoilType"]))
                    {
                        var filterValue = filters["SoilType"].Trim();
                        query = query.Where(f => f.SoilType != null && f.SoilType.Contains(filterValue));
                    }

                    if (filters.ContainsKey("IrrigationAvailable") && !string.IsNullOrWhiteSpace(filters["IrrigationAvailable"]))
                    {
                        if (bool.TryParse(filters["IrrigationAvailable"], out bool irrigationAvailable))
                        {
                            query = query.Where(f => f.IrrigationAvailable == irrigationAvailable);
                        }
                    }

                    if (filters.ContainsKey("IsActive") && !string.IsNullOrWhiteSpace(filters["IsActive"]))
                    {
                        if (bool.TryParse(filters["IsActive"], out bool isActive))
                        {
                            query = query.Where(f => f.IsActive == isActive);
                        }
                    }
                }

                response.Data = await query.OrderBy(f => f.FieldName).ToListAsync();
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error filtering fields: {ex.Message}";
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

                var users = await _userManager.Users
                    .OrderBy(u => u.UserName)
                    .ToListAsync();

                var userIds = users.Where(u => u.PersonId.HasValue).Select(u => u.PersonId.Value).ToList();
                var people = await _context.People
                    .Where(p => userIds.Contains(p.PersonId))
                    .ToDictionaryAsync(p => p.PersonId);

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

                var userItems = users.Select(u =>
                {
                    Person? person = null;
                    if (u.PersonId.HasValue && people.TryGetValue(u.PersonId.Value, out var p))
                    {
                        person = p;
                    }

                    var displayName = person != null && 
                                      !string.IsNullOrEmpty(person.FirstName) && 
                                      !string.IsNullOrEmpty(person.LastName)
                        ? $"{person.FirstName} {person.LastName} ({u.UserName})"
                        : u.UserName ?? $"User #{u.Id}";

                    return new SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = displayName
                    };
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
