using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Database.Entities;
using TpaSodManagement.Database;

namespace TpaSodManagement.Services.Implementations
{
    public class SeedingService : ISeedingService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<TpaSodManagementUser> _userManager;

        public SeedingService(ApplicationDbContext context, UserManager<TpaSodManagementUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

        public async Task<ServiceResponse<List<Seeding>>> GetFilteredAsync(Dictionary<string, string> filters)
        {
            var response = new ServiceResponse<List<Seeding>>();
            try
            {
                var query = _context.Seedings
                    .Include(s => s.AreaType)
                    .Include(s => s.Farm)
                    .Include(s => s.Field)
                    .Include(s => s.TagRange)
                    .Include(s => s.User)
                    .AsQueryable();

                // Apply filters
                if (filters != null && filters.Count > 0)
                {
                    if (filters.ContainsKey("AreaAmount") && !string.IsNullOrWhiteSpace(filters["AreaAmount"]))
                    {
                        if (decimal.TryParse(filters["AreaAmount"], out decimal areaAmount))
                        {
                            query = query.Where(s => s.AreaAmount == areaAmount);
                        }
                    }

                    if (filters.ContainsKey("SeedingMethod") && !string.IsNullOrWhiteSpace(filters["SeedingMethod"]))
                    {
                        var filterValue = filters["SeedingMethod"].Trim();
                        query = query.Where(s => s.SeedingMethod != null && s.SeedingMethod.Contains(filterValue));
                    }

                    if (filters.ContainsKey("SeedRatePerUnit") && !string.IsNullOrWhiteSpace(filters["SeedRatePerUnit"]))
                    {
                        if (decimal.TryParse(filters["SeedRatePerUnit"], out decimal seedRate))
                        {
                            query = query.Where(s => s.SeedRatePerUnit == seedRate);
                        }
                    }

                    if (filters.ContainsKey("WeatherConditions") && !string.IsNullOrWhiteSpace(filters["WeatherConditions"]))
                    {
                        var filterValue = filters["WeatherConditions"].Trim();
                        query = query.Where(s => s.WeatherConditions != null && s.WeatherConditions.Contains(filterValue));
                    }

                    if (filters.ContainsKey("SoilTemperature") && !string.IsNullOrWhiteSpace(filters["SoilTemperature"]))
                    {
                        if (decimal.TryParse(filters["SoilTemperature"], out decimal soilTemp))
                        {
                            query = query.Where(s => s.SoilTemperature == soilTemp);
                        }
                    }

                    if (filters.ContainsKey("SoilMoisture") && !string.IsNullOrWhiteSpace(filters["SoilMoisture"]))
                    {
                        var filterValue = filters["SoilMoisture"].Trim();
                        query = query.Where(s => s.SoilMoisture != null && s.SoilMoisture.Contains(filterValue));
                    }

                    if (filters.ContainsKey("Notes") && !string.IsNullOrWhiteSpace(filters["Notes"]))
                    {
                        var filterValue = filters["Notes"].Trim();
                        query = query.Where(s => s.Notes != null && s.Notes.Contains(filterValue));
                    }

                    if (filters.ContainsKey("AreaTypeName") && !string.IsNullOrWhiteSpace(filters["AreaTypeName"]))
                    {
                        var filterValue = filters["AreaTypeName"].Trim();
                        query = query.Where(s => s.AreaType != null && s.AreaType.AreaTypeName != null && s.AreaType.AreaTypeName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("FarmLicenseNumber") && !string.IsNullOrWhiteSpace(filters["FarmLicenseNumber"]))
                    {
                        var filterValue = filters["FarmLicenseNumber"].Trim();
                        query = query.Where(s => s.Farm != null && s.Farm.LicenseNumber != null && s.Farm.LicenseNumber.Contains(filterValue));
                    }

                    if (filters.ContainsKey("FieldName") && !string.IsNullOrWhiteSpace(filters["FieldName"]))
                    {
                        var filterValue = filters["FieldName"].Trim();
                        query = query.Where(s => s.Field != null && s.Field.FieldName != null && s.Field.FieldName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("TagRangeCode") && !string.IsNullOrWhiteSpace(filters["TagRangeCode"]))
                    {
                        var filterValue = filters["TagRangeCode"].Trim();
                        query = query.Where(s => s.TagRange != null && s.TagRange.TagRangeCode != null && s.TagRange.TagRangeCode.Contains(filterValue));
                    }

                    if (filters.ContainsKey("UserName") && !string.IsNullOrWhiteSpace(filters["UserName"]))
                    {
                        var filterValue = filters["UserName"].Trim();
                        query = query.Where(s => s.User != null && s.User.UserName != null && s.User.UserName.Contains(filterValue));
                    }

                    // Date range filters for SeedingDate (DateOnly)
                    if (filters.ContainsKey("SeedingDate_From") && !string.IsNullOrWhiteSpace(filters["SeedingDate_From"]))
                    {
                        if (DateOnly.TryParse(filters["SeedingDate_From"], out DateOnly fromDate))
                        {
                            query = query.Where(s => s.SeedingDate >= fromDate);
                        }
                    }

                    if (filters.ContainsKey("SeedingDate_To") && !string.IsNullOrWhiteSpace(filters["SeedingDate_To"]))
                    {
                        if (DateOnly.TryParse(filters["SeedingDate_To"], out DateOnly toDate))
                        {
                            query = query.Where(s => s.SeedingDate <= toDate);
                        }
                    }

                    // Date range filters for CreatedDate
                    if (filters.ContainsKey("CreatedDate_From") && !string.IsNullOrWhiteSpace(filters["CreatedDate_From"]))
                    {
                        if (DateTimeOffset.TryParse(filters["CreatedDate_From"], out DateTimeOffset fromDate))
                        {
                            query = query.Where(s => s.CreatedDate >= fromDate);
                        }
                    }

                    if (filters.ContainsKey("CreatedDate_To") && !string.IsNullOrWhiteSpace(filters["CreatedDate_To"]))
                    {
                        if (DateTimeOffset.TryParse(filters["CreatedDate_To"], out DateTimeOffset toDate))
                        {
                            // Add one day to include the entire end date
                            toDate = toDate.AddDays(1).AddTicks(-1);
                            query = query.Where(s => s.CreatedDate <= toDate);
                        }
                    }
                }

                response.Data = await query.OrderByDescending(s => s.CreatedDate).ToListAsync();
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error filtering seedings: {ex.Message}";
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

        public async Task<ServiceResponse<bool>> DeleteAsync(long id, long? deletedByUserId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var seeding = await _context.Seedings
                    .FirstOrDefaultAsync(s => s.SeedingId == id && s.DeletedDate == null);
                if (seeding == null)
                {
                    response.Success = false;
                    response.Message = "Seeding not found";
                    response.Data = false;
                    return response;
                }

                // Soft delete: Set DeletedDate and DeletedByUserId
                seeding.DeletedDate = DateTimeOffset.UtcNow;
                seeding.DeletedByUserId = deletedByUserId;
                
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
                var areaTypes = await _context.AreaTypes
                    .OrderBy(a => a.AreaTypeName)
                    .ToListAsync();

                var farms = await _context.Farms
                    .Include(f => f.Organization)
                    .OrderBy(f => f.FarmId)
                    .ToListAsync();

                // Fields fetch - Simple version without complex includes
                var fields = await _context.Fields
                    .OrderBy(f => f.FieldName)
                    .ToListAsync();
                
                var tagRanges = await _context.TagRanges
                    .OrderBy(t => t.TagRangeId)
                    .ToListAsync();
                
                // TpaSodManagementUser se users fetch karein
                var users = await _userManager.Users
                    .OrderBy(u => u.UserName)
                    .ToListAsync();

                // Fetch all Person records for these users in one query (efficient batch loading)
                var userIds = users.Where(u => u.PersonId.HasValue).Select(u => u.PersonId.Value).ToList();
                var people = await _context.People
                    .Where(p => userIds.Contains(p.PersonId))
                    .ToDictionaryAsync(p => p.PersonId);

                // Create SelectList for AreaTypes
                var areaTypeItems = areaTypes.Select(a => new SelectListItem
                {
                    Value = a.AreaTypeId.ToString(),
                    Text = a.AreaTypeName
                }).ToList();

                // Create SelectList for Farms with display name
                var farmItems = farms.Select(f => new SelectListItem
                {
                    Value = f.FarmId.ToString(),
                    Text = !string.IsNullOrEmpty(f.LicenseNumber)
                        ? $"{f.LicenseNumber} ({(f.Organization != null ? f.Organization.OrganizationName : "N/A")})"
                        : $"Farm #{f.FarmId} ({(f.Organization != null ? f.Organization.OrganizationName : "N/A")})"
                }).ToList();

                // Create SelectList for Fields - Simple version (just FieldName)
                var fieldItems = fields.Select(f => new SelectListItem
                {
                    Value = f.FieldId.ToString(),
                    Text = f.FieldName ?? $"Field #{f.FieldId}" // Simple: Just FieldName
                }).ToList();

                // Create SelectList for TagRanges
                var tagRangeItems = tagRanges.Select(t => new SelectListItem
                {
                    Value = t.TagRangeId.ToString(),
                    Text = $"Tag Range #{t.TagRangeId}" // Adjust based on TagRange model properties
                }).ToList();

                // Create SelectList for Users with display name (FirstName LastName or UserName)
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
                    new SelectList(areaTypeItems, "Value", "Text"),
                    new SelectList(farmItems, "Value", "Text"),
                    new SelectList(fieldItems, "Value", "Text"), // Direct SelectList - no need for fieldsSelectList variable
                    new SelectList(tagRangeItems, "Value", "Text"),
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
