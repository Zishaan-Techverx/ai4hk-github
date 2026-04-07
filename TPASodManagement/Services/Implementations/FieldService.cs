using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Database.Entities;
using TpaSodManagement.Database;
using TpaSodManagement.Utilities;

namespace TpaSodManagement.Services.Implementations
{
    public class FieldService : IFieldService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<TpaSodManagementUser> _userManager;
        private readonly ICurrentUserService _currentUserService;

        public FieldService(ApplicationDbContext context, UserManager<TpaSodManagementUser> userManager, ICurrentUserService currentUserService)
        {
            _context = context;
            _userManager = userManager;
            _currentUserService = currentUserService;
        }

        public async Task<ServiceResponse<List<Field>>> GetAllAsync()
        {
            var response = new ServiceResponse<List<Field>>();
            try
            {
                var query = _context.Fields
                    .Include(f => f.Farm)
                    .Include(f => f.AreaType)
                    .Include(f => f.FieldType)
                    .AsQueryable();

                if (!await _currentUserService.IsCurrentUserSuperAdminAsync())
                {
                    var farmId = await _currentUserService.GetCurrentUserFarmIdAsync();
                    if (farmId.HasValue)
                        query = query.Where(f => f.FarmId == farmId.Value);
                    else
                        query = query.Where(f => false);
                }

                response.Data = await query.OrderBy(f => f.FieldName).ToListAsync();
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
                    .Include(f => f.AreaType)
                    .Include(f => f.FieldType)
                    .AsQueryable();

                if (!await _currentUserService.IsCurrentUserSuperAdminAsync())
                {
                    var farmId = await _currentUserService.GetCurrentUserFarmIdAsync();
                    if (farmId.HasValue)
                        query = query.Where(f => f.FarmId == farmId.Value);
                    else
                        query = query.Where(f => false);
                }

                // Apply filters
                if (filters != null && filters.Count > 0)
                {
                    if (filters.ContainsKey("FieldName") && !string.IsNullOrWhiteSpace(filters["FieldName"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["FieldName"]);
                        query = query.Where(f => f.FieldName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("FieldCode") && !string.IsNullOrWhiteSpace(filters["FieldCode"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["FieldCode"]);
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
                        var filterValue = FilterHelper.NormalizeSearchText(filters["AreaTypeName"]);
                        query = query.Where(f => f.AreaType != null && f.AreaType.AreaTypeName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("FieldTypeName") && !string.IsNullOrWhiteSpace(filters["FieldTypeName"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["FieldTypeName"]);
                        query = query.Where(f => f.FieldType != null && f.FieldType.FieldTypeName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("FarmName") && !string.IsNullOrWhiteSpace(filters["FarmName"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["FarmName"]);
                        query = query.Where(f => f.Farm != null && f.Farm.FarmName != null && f.Farm.FarmName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("SoilType") && !string.IsNullOrWhiteSpace(filters["SoilType"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["SoilType"]);
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
                    .Include(f => f.AreaType)
                    .Include(f => f.FieldType)
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
                var currentUserId = await _currentUserService.GetCurrentUserIdAsync();
                field.CreatedDate = DateTimeOffset.UtcNow;
                field.CreatedByUserId = currentUserId;
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
                // Fetch existing entity from database to preserve CreatedByUserId and CreatedDate
                var existingField = await _context.Fields
                    .FirstOrDefaultAsync(f => f.FieldId == field.FieldId);
                
                if (existingField == null)
                {
                    response.Success = false;
                    response.Message = "Field not found";
                    return response;
                }

                // Update only the properties that should be updated
                // Preserve CreatedByUserId and CreatedDate
                existingField.FieldName = field.FieldName;
                existingField.FieldCode = field.FieldCode;
                existingField.FarmId = field.FarmId;
                existingField.AreaAmount = field.AreaAmount;
                existingField.AreaTypeId = field.AreaTypeId;
                existingField.FieldTypeId = field.FieldTypeId;
                existingField.SoilType = field.SoilType;
                existingField.IrrigationAvailable = field.IrrigationAvailable;
                existingField.IsActive = field.IsActive;
                
                // Set update audit fields
                var currentUserId = await _currentUserService.GetCurrentUserIdAsync();
                existingField.UpdatedDate = DateTimeOffset.UtcNow;
                existingField.UpdatedByUserId = currentUserId;
                
                // CreatedByUserId and CreatedDate are preserved from existingField
                
                await _context.SaveChangesAsync();
                response.Data = existingField;
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

        public async Task<ServiceResponse<bool>> DeleteAsync(long id, long? deletedByUserId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var field = await _context.Fields
                    .FirstOrDefaultAsync(f => f.FieldId == id && f.DeletedDate == null);
                if (field == null)
                {
                    response.Success = false;
                    response.Message = "Field not found";
                    response.Data = false;
                    return response;
                }

                // Soft delete: Set DeletedDate and DeletedByUserId
                var currentUserId = deletedByUserId ?? await _currentUserService.GetCurrentUserIdAsync();
                field.DeletedDate = DateTimeOffset.UtcNow;
                field.DeletedByUserId = currentUserId;
                
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

        public async Task<ServiceResponse<(SelectList Farms, SelectList AreaTypes, SelectList FieldTypes, SelectList Users)>> GetDropdownDataAsync()
        {
            var response = new ServiceResponse<(SelectList, SelectList, SelectList, SelectList)>();
            try
            {
                var farmsQuery = _context.Farms.OrderBy(f => f.FarmId).AsQueryable();
                if (!await _currentUserService.IsCurrentUserSuperAdminAsync())
                {
                    var farmId = await _currentUserService.GetCurrentUserFarmIdAsync();
                    if (farmId.HasValue)
                        farmsQuery = farmsQuery.Where(f => f.FarmId == farmId.Value);
                    else
                        farmsQuery = farmsQuery.Where(f => false);
                }
                var farms = await farmsQuery.ToListAsync();

                var areaTypes = await _context.AreaTypes
                    .OrderBy(a => a.AreaTypeName)
                    .ToListAsync();

                var fieldTypes = await _context.FieldTypes
                    .Where(ft => ft.DeletedDate == null && ft.IsActive)
                    .OrderBy(ft => ft.FieldTypeName)
                    .ToListAsync();

                // Users: filter by org for non-SuperAdmin
                var usersQuery = _userManager.Users.AsQueryable();
                if (!await _currentUserService.IsCurrentUserSuperAdminAsync())
                {
                    var farmId = await _currentUserService.GetCurrentUserFarmIdAsync();
                    if (farmId.HasValue)
                        usersQuery = usersQuery.Where(u => u.FarmId == farmId.Value);
                    else
                        usersQuery = usersQuery.Where(u => false);
                }
                var users = await usersQuery.OrderBy(u => u.UserName).ToListAsync();

                var userIds = users.Where(u => u.PersonId.HasValue).Select(u => u.PersonId!.Value).ToList();
                var people = await _context.People
                    .Where(p => userIds.Contains(p.PersonId))
                    .ToDictionaryAsync(p => p.PersonId);

                var farmItems = farms.Select(f => new SelectListItem
                {
                    Value = f.FarmId.ToString(),
                    Text = f.FarmName ?? $"Farm #{f.FarmId}"
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

                var fieldTypeItems = fieldTypes.Select(ft => new SelectListItem
                {
                    Value = ft.FieldTypeId.ToString(),
                    Text = ft.FieldTypeName
                }).ToList();

                response.Data = (
                    new SelectList(farmItems, "Value", "Text"),
                    new SelectList(areaTypeItems, "Value", "Text"),
                    new SelectList(fieldTypeItems, "Value", "Text"),
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
