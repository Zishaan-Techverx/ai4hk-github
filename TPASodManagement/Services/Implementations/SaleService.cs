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
    public class SaleService : ISaleService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<TpaSodManagementUser> _userManager;
        private readonly ICurrentUserService _currentUserService;

        public SaleService(ApplicationDbContext context, UserManager<TpaSodManagementUser> userManager, ICurrentUserService currentUserService)
        {
            _context = context;
            _userManager = userManager;
            _currentUserService = currentUserService;
        }

        public async Task<ServiceResponse<List<Sale>>> GetAllAsync()
        {
            var response = new ServiceResponse<List<Sale>>();
            try
            {
                // IgnoreQueryFilters so related entities (Farm, Currency, Customer, etc.) with soft-delete
                // don't exclude Sales via INNER JOIN. We manually filter Sale.DeletedDate.
                var query = _context.Sales
                    .IgnoreQueryFilters()
                    .Where(s => s.DeletedDate == null)
                    .Include(s => s.Currency)
                    .Include(s => s.Customer)
                        .ThenInclude(c => c.Person)
                    .Include(s => s.Customer)
                        .ThenInclude(c => c.Organization)
                    .Include(s => s.Farm)
                    .Include(s => s.Field)
                        .ThenInclude(f => f!.FieldType)
                    .Include(s => s.SaleType)
                    .Include(s => s.Status)
                    .Include(s => s.UpdatedByUser)
                    .Include(s => s.User)
                    .AsQueryable();

                if (!await _currentUserService.IsCurrentUserSuperAdminAsync())
                {
                    var farmId = await _currentUserService.GetCurrentUserFarmIdAsync();
                    if (farmId.HasValue)
                        query = query.Where(s => s.FarmId == farmId.Value);
                    else
                        query = query.Where(s => false);
                }

                response.Data = await query.ToListAsync();
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching sales: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<List<Sale>>> GetFilteredAsync(Dictionary<string, string> filters)
        {
            var response = new ServiceResponse<List<Sale>>();
            try
            {
                var query = _context.Sales
                    .IgnoreQueryFilters()
                    .Where(s => s.DeletedDate == null)
                    .Include(s => s.Currency)
                    .Include(s => s.Customer)
                        .ThenInclude(c => c.Person)
                    .Include(s => s.Customer)
                        .ThenInclude(c => c.Organization)
                    .Include(s => s.Farm)
                    .Include(s => s.Field)
                        .ThenInclude(f => f!.FieldType)
                    .Include(s => s.SaleType)
                    .Include(s => s.Status)
                    .Include(s => s.UpdatedByUser)
                    .Include(s => s.User)
                    .AsQueryable();

                if (!await _currentUserService.IsCurrentUserSuperAdminAsync())
                {
                    var farmId = await _currentUserService.GetCurrentUserFarmIdAsync();
                    if (farmId.HasValue)
                        query = query.Where(s => s.FarmId == farmId.Value);
                    else
                        query = query.Where(s => false);
                }

                // Apply filters
                if (filters != null && filters.Count > 0)
                {
                    if (filters.ContainsKey("SaleNumber") && !string.IsNullOrWhiteSpace(filters["SaleNumber"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["SaleNumber"]);
                        query = query.Where(s => s.SaleNumber != null && s.SaleNumber.Contains(filterValue));
                    }

                    if (filters.ContainsKey("InvoiceNumber") && !string.IsNullOrWhiteSpace(filters["InvoiceNumber"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["InvoiceNumber"]);
                        query = query.Where(s => s.InvoiceNumber != null && s.InvoiceNumber.Contains(filterValue));
                    }

                    if (filters.ContainsKey("PurchaseOrderNumber") && !string.IsNullOrWhiteSpace(filters["PurchaseOrderNumber"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["PurchaseOrderNumber"]);
                        query = query.Where(s => s.PurchaseOrderNumber != null && s.PurchaseOrderNumber.Contains(filterValue));
                    }

                    if (filters.ContainsKey("SubtotalAmount") && !string.IsNullOrWhiteSpace(filters["SubtotalAmount"]))
                    {
                        if (decimal.TryParse(filters["SubtotalAmount"], out decimal subtotal))
                        {
                            query = query.Where(s => s.SubtotalAmount == subtotal);
                        }
                    }

                    if (filters.ContainsKey("TaxAmount") && !string.IsNullOrWhiteSpace(filters["TaxAmount"]))
                    {
                        if (decimal.TryParse(filters["TaxAmount"], out decimal tax))
                        {
                            query = query.Where(s => s.TaxAmount == tax);
                        }
                    }

                    if (filters.ContainsKey("DiscountAmount") && !string.IsNullOrWhiteSpace(filters["DiscountAmount"]))
                    {
                        if (decimal.TryParse(filters["DiscountAmount"], out decimal discount))
                        {
                            query = query.Where(s => s.DiscountAmount == discount);
                        }
                    }

                    if (filters.ContainsKey("TotalAmount") && !string.IsNullOrWhiteSpace(filters["TotalAmount"]))
                    {
                        if (decimal.TryParse(filters["TotalAmount"], out decimal total))
                        {
                            query = query.Where(s => s.TotalAmount == total);
                        }
                    }

                    if (filters.ContainsKey("PaymentTermsDays") && !string.IsNullOrWhiteSpace(filters["PaymentTermsDays"]))
                    {
                        if (int.TryParse(filters["PaymentTermsDays"], out int paymentTerms))
                        {
                            query = query.Where(s => s.PaymentTermsDays == paymentTerms);
                        }
                    }

                    if (filters.ContainsKey("Notes") && !string.IsNullOrWhiteSpace(filters["Notes"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["Notes"]);
                        query = query.Where(s => s.Notes != null && s.Notes.Contains(filterValue));
                    }

                    if (filters.ContainsKey("CurrencyName") && !string.IsNullOrWhiteSpace(filters["CurrencyName"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["CurrencyName"]);
                        query = query.Where(s => s.Currency != null && s.Currency.CurrencyName != null && s.Currency.CurrencyName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("CustomerDisplay") && !string.IsNullOrWhiteSpace(filters["CustomerDisplay"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["CustomerDisplay"]);
                        query = query.Where(s => (s.Customer != null && s.Customer.Person != null && 
                            (s.Customer.Person.FirstName != null && s.Customer.Person.FirstName.Contains(filterValue) ||
                             s.Customer.Person.LastName != null && s.Customer.Person.LastName.Contains(filterValue))) ||
                            (s.Customer != null && s.Customer.Organization != null && s.Customer.Organization.OrganizationName != null && s.Customer.Organization.OrganizationName.Contains(filterValue)) ||
                            (s.Customer != null && s.Customer.CustomerCode != null && s.Customer.CustomerCode.Contains(filterValue)));
                    }

                    if (filters.ContainsKey("FarmName") && !string.IsNullOrWhiteSpace(filters["FarmName"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["FarmName"]);
                        query = query.Where(s => s.Farm != null && s.Farm.FarmName != null && s.Farm.FarmName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("SaleTypeName") && !string.IsNullOrWhiteSpace(filters["SaleTypeName"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["SaleTypeName"]);
                        query = query.Where(s => s.SaleType != null && s.SaleType.SaleTypeName != null && s.SaleType.SaleTypeName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("StatusName") && !string.IsNullOrWhiteSpace(filters["StatusName"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["StatusName"]);
                        query = query.Where(s => s.Status != null && s.Status.StatusName != null && s.Status.StatusName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("UpdatedByUserName") && !string.IsNullOrWhiteSpace(filters["UpdatedByUserName"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["UpdatedByUserName"]);
                        query = query.Where(s => s.UpdatedByUser != null && s.UpdatedByUser.UserName != null && s.UpdatedByUser.UserName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("UserName") && !string.IsNullOrWhiteSpace(filters["UserName"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["UserName"]);
                        query = query.Where(s => s.User != null && s.User.UserName != null && s.User.UserName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("IsActive") && !string.IsNullOrWhiteSpace(filters["IsActive"]))
                    {
                        if (bool.TryParse(filters["IsActive"], out bool isActiveValue))
                            query = query.Where(s => s.IsActive == isActiveValue);
                        else if (filters["IsActive"].ToLower() == "true" || filters["IsActive"].ToLower() == "yes" || filters["IsActive"].ToLower() == "1")
                            query = query.Where(s => s.IsActive == true);
                        else if (filters["IsActive"].ToLower() == "false" || filters["IsActive"].ToLower() == "no" || filters["IsActive"].ToLower() == "0")
                            query = query.Where(s => s.IsActive == false);
                    }

                    // Date range filters for SaleDate (DateOnly)
                    if (filters.ContainsKey("SaleDate_From") && !string.IsNullOrWhiteSpace(filters["SaleDate_From"]))
                    {
                        if (DateOnly.TryParse(filters["SaleDate_From"], out DateOnly fromDate))
                        {
                            query = query.Where(s => s.SaleDate >= fromDate);
                        }
                    }

                    if (filters.ContainsKey("SaleDate_To") && !string.IsNullOrWhiteSpace(filters["SaleDate_To"]))
                    {
                        if (DateOnly.TryParse(filters["SaleDate_To"], out DateOnly toDate))
                        {
                            query = query.Where(s => s.SaleDate <= toDate);
                        }
                    }

                    // Date range filters for DueDate (DateOnly?)
                    if (filters.ContainsKey("DueDate_From") && !string.IsNullOrWhiteSpace(filters["DueDate_From"]))
                    {
                        if (DateOnly.TryParse(filters["DueDate_From"], out DateOnly fromDate))
                        {
                            query = query.Where(s => s.DueDate.HasValue && s.DueDate.Value >= fromDate);
                        }
                    }

                    if (filters.ContainsKey("DueDate_To") && !string.IsNullOrWhiteSpace(filters["DueDate_To"]))
                    {
                        if (DateOnly.TryParse(filters["DueDate_To"], out DateOnly toDate))
                        {
                            query = query.Where(s => s.DueDate.HasValue && s.DueDate.Value <= toDate);
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

                    // Date range filters for UpdatedDate
                    if (filters.ContainsKey("UpdatedDate_From") && !string.IsNullOrWhiteSpace(filters["UpdatedDate_From"]))
                    {
                        if (DateTimeOffset.TryParse(filters["UpdatedDate_From"], out DateTimeOffset fromDate))
                        {
                            query = query.Where(s => s.UpdatedDate >= fromDate);
                        }
                    }

                    if (filters.ContainsKey("UpdatedDate_To") && !string.IsNullOrWhiteSpace(filters["UpdatedDate_To"]))
                    {
                        if (DateTimeOffset.TryParse(filters["UpdatedDate_To"], out DateTimeOffset toDate))
                        {
                            // Add one day to include the entire end date
                            toDate = toDate.AddDays(1).AddTicks(-1);
                            query = query.Where(s => s.UpdatedDate <= toDate);
                        }
                    }
                }

                response.Data = await query.OrderByDescending(s => s.CreatedDate).ToListAsync();
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error filtering sales: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<Sale>> GetByIdAsync(long id)
        {
            var response = new ServiceResponse<Sale>();
            try
            {
                var sale = await _context.Sales
                    .IgnoreQueryFilters()
                    .Where(s => s.DeletedDate == null)
                    .Include(s => s.Currency)
                    .Include(s => s.Customer)
                    .Include(s => s.Farm)
                    .Include(s => s.Field)
                        .ThenInclude(f => f!.FieldType)
                    .Include(s => s.SaleType)
                    .Include(s => s.Status)
                    .Include(s => s.UpdatedByUser)
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.SaleId == id);

                if (sale == null)
                {
                    response.Success = false;
                    response.Message = "Sale not found";
                }
                else
                {
                    response.Data = sale;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching sale: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<Sale>> GetByIdForCertificateAsync(long id)
        {
            var response = new ServiceResponse<Sale>();
            try
            {
                var sale = await _context.Sales
                    .IgnoreQueryFilters()
                    .Where(s => s.DeletedDate == null)
                    .Include(s => s.Currency)
                    .Include(s => s.Customer).ThenInclude(c => c!.Person)
                    .Include(s => s.Customer).ThenInclude(c => c!.Organization)
                    .Include(s => s.Customer).ThenInclude(c => c!.Address).ThenInclude(a => a!.StateProvince)
                    .Include(s => s.Farm).ThenInclude(f => f.Address).ThenInclude(a => a!.StateProvince)
                    .Include(s => s.Farm).ThenInclude(f => f.Organization)
                    .Include(s => s.Field).ThenInclude(f => f!.FieldType)
                    .Include(s => s.SaleType)
                    .Include(s => s.Status)
                    .FirstOrDefaultAsync(s => s.SaleId == id);

                if (sale == null)
                {
                    response.Success = false;
                    response.Message = "Sale not found";
                }
                else
                {
                    response.Data = sale;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching sale for certificate: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<Sale>> CreateAsync(Sale sale)
        {
            var response = new ServiceResponse<Sale>();
            try
            {
                var currentUserId = await _currentUserService.GetCurrentUserIdAsync();
                sale.CreatedDate = DateTimeOffset.UtcNow;
                sale.CreatedByUserId = currentUserId;
                _context.Sales.Add(sale);
                await _context.SaveChangesAsync();
                response.Data = sale;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error creating sale: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<Sale>> UpdateAsync(Sale sale)
        {
            var response = new ServiceResponse<Sale>();
            try
            {
                // Fetch existing entity from database to preserve CreatedByUserId and CreatedDate
                var existingSale = await _context.Sales
                    .FirstOrDefaultAsync(s => s.SaleId == sale.SaleId);
                
                if (existingSale == null)
                {
                    response.Success = false;
                    response.Message = "Sale not found";
                    return response;
                }

                // Update only the properties that should be updated
                // Preserve CreatedByUserId and CreatedDate
                existingSale.UserId = sale.UserId;
                existingSale.FarmId = sale.FarmId;
                existingSale.CustomerId = sale.CustomerId;
                existingSale.FieldId = sale.FieldId;
                existingSale.SaleTypeId = sale.SaleTypeId;
                existingSale.SaleNumber = sale.SaleNumber;
                existingSale.InvoiceNumber = sale.InvoiceNumber;
                existingSale.PurchaseOrderNumber = sale.PurchaseOrderNumber;
                existingSale.SaleDate = sale.SaleDate;
                existingSale.DueDate = sale.DueDate;
                existingSale.SubtotalAmount = sale.SubtotalAmount;
                existingSale.TaxAmount = sale.TaxAmount;
                existingSale.DiscountAmount = sale.DiscountAmount;
                existingSale.TotalAmount = sale.TotalAmount;
                existingSale.CurrencyId = sale.CurrencyId;
                existingSale.PaymentTermsDays = sale.PaymentTermsDays;
                existingSale.StatusId = sale.StatusId;
                existingSale.Notes = sale.Notes;
                existingSale.IsActive = sale.IsActive;
                
                // Set update audit fields
                var currentUserId = await _currentUserService.GetCurrentUserIdAsync();
                existingSale.UpdatedDate = DateTimeOffset.UtcNow;
                existingSale.UpdatedByUserId = currentUserId;
                
                // CreatedByUserId and CreatedDate are preserved from existingSale
                
                await _context.SaveChangesAsync();
                response.Data = existingSale;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error updating sale: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteAsync(long id, long? deletedByUserId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var sale = await _context.Sales
                    .FirstOrDefaultAsync(s => s.SaleId == id && s.DeletedDate == null);
                if (sale == null)
                {
                    response.Success = false;
                    response.Message = "Sale not found";
                    response.Data = false;
                    return response;
                }

                // Soft delete: Set DeletedDate and DeletedByUserId
                var currentUserId = deletedByUserId ?? await _currentUserService.GetCurrentUserIdAsync();
                sale.DeletedDate = DateTimeOffset.UtcNow;
                sale.DeletedByUserId = currentUserId;
                
                await _context.SaveChangesAsync();
                response.Data = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error deleting sale: {ex.Message}";
                response.Data = false;
            }
            return response;
        }

        public async Task<ServiceResponse<Dictionary<string, IEnumerable<SelectListItem>>>> GetDropdownDataAsync(long? selectedId = null)
        {
            var response = new ServiceResponse<Dictionary<string, IEnumerable<SelectListItem>>>();
            try
            {
                // TpaSodManagementUser se users fetch karein (filter by org for non-SuperAdmin)
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

                // Fetch all Person records for these users in one query (efficient batch loading)
                var userIds = users.Where(u => u.PersonId.HasValue).Select(u => u.PersonId!.Value).ToList();
                var people = await _context.People
                    .Where(p => userIds.Contains(p.PersonId))
                    .ToDictionaryAsync(p => p.PersonId);

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

                // Farms fetch karein with Organization (filter by user org for non-SuperAdmin)
                var farmsQuery = _context.Farms
                    .Include(f => f.Organization)
                    .OrderBy(f => f.FarmId)
                    .AsQueryable();
                if (!await _currentUserService.IsCurrentUserSuperAdminAsync())
                {
                    var farmId = await _currentUserService.GetCurrentUserFarmIdAsync();
                    if (farmId.HasValue)
                        farmsQuery = farmsQuery.Where(f => f.FarmId == farmId.Value);
                    else
                        farmsQuery = farmsQuery.Where(f => false);
                }
                var farms = await farmsQuery.ToListAsync();

                // Create SelectList for Farms with display name (Farm Name preferred)
                var farmItems = farms.Select(f =>
                {
                    var farmName = f.FarmName?.Trim();
                    var license = f.LicenseNumber?.Trim();
                    var orgName = f.Organization?.OrganizationName?.Trim();

                    var primaryLabel = !string.IsNullOrEmpty(farmName)
                        ? farmName
                        : !string.IsNullOrEmpty(license)
                            ? license
                            : $"Farm #{f.FarmId}";

                    var suffix = !string.IsNullOrEmpty(orgName) ? $" ({orgName})" : string.Empty;

                    return new SelectListItem
                    {
                        Value = f.FarmId.ToString(),
                        Text = primaryLabel + suffix
                    };
                }).ToList();

                // Currencies fetch karein
                var currencies = await _context.Currencies
                    .OrderBy(c => c.CurrencyName)
                    .ToListAsync();

                // Create SelectList for Currencies with display name
                var currencyItems = currencies.Select(c => new SelectListItem
                {
                    Value = c.CurrencyId.ToString(),
                    Text = c.CurrencyName
                }).ToList();

                // Customers fetch karein with Person and Organization for display name (filter by user org for non-SuperAdmin)
                var customersQuery = _context.Customers
                    .Include(c => c.Person)
                    .Include(c => c.Organization)
                    .Where(c => c.IsActive) // Only active customers
                    .OrderBy(c => c.CustomerId)
                    .AsQueryable();
                if (!await _currentUserService.IsCurrentUserSuperAdminAsync())
                {
                    var farmId = await _currentUserService.GetCurrentUserFarmIdAsync();
                    if (farmId.HasValue)
                        customersQuery = customersQuery.Where(c => c.FarmId == farmId.Value);
                    else
                        customersQuery = customersQuery.Where(c => false);
                }
                var customers = await customersQuery.ToListAsync();

                var fieldsQuery = _context.Fields
                    .Include(f => f.Farm)
                    .Where(f => f.IsActive)
                    .OrderBy(f => f.FieldName)
                    .AsQueryable();
                if (!await _currentUserService.IsCurrentUserSuperAdminAsync())
                {
                    var farmId = await _currentUserService.GetCurrentUserFarmIdAsync();
                    if (farmId.HasValue)
                        fieldsQuery = fieldsQuery.Where(f => f.FarmId == farmId.Value);
                    else
                        fieldsQuery = fieldsQuery.Where(f => false);
                }
                var fields = await fieldsQuery.ToListAsync();

                // Create SelectList for Customers with display name (use Person/Organization to decide)
                var customerItems = customers.Select(c =>
                {
                    string displayName;
                    if (c.Person != null)
                        {
                            var firstName = c.Person.FirstName?.Trim() ?? "";
                            var lastName = c.Person.LastName?.Trim() ?? "";
                            
                            if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
                            {
                                displayName = $"{firstName} {lastName}";
                            }
                            else if (!string.IsNullOrEmpty(firstName))
                            {
                                displayName = firstName;
                            }
                            else if (!string.IsNullOrEmpty(lastName))
                            {
                                displayName = lastName;
                            }
                            else
                            {
                                displayName = !string.IsNullOrEmpty(c.CustomerCode)
                                    ? c.CustomerCode
                                    : $"Customer #{c.CustomerId}";
                            }
                        }
                        else if (c.Organization != null)
                        {
                            var orgName = c.Organization.OrganizationName?.Trim() ?? "";
                            
                            if (!string.IsNullOrEmpty(orgName))
                            {
                                displayName = orgName;
                            }
                            else
                            {
                                displayName = !string.IsNullOrEmpty(c.CustomerCode)
                                    ? c.CustomerCode
                                    : $"Customer #{c.CustomerId}";
                            }
                        }
                        else
                        {
                            // Final fallback to CustomerCode or CustomerId
                            displayName = !string.IsNullOrEmpty(c.CustomerCode)
                                ? c.CustomerCode
                                : $"Customer #{c.CustomerId}";
                        }

                    return new SelectListItem
                    {
                        Value = c.CustomerId.ToString(),
                        Text = displayName
                    };
                }).ToList();

                response.Data = new Dictionary<string, IEnumerable<SelectListItem>>
                {
                    ["CurrencyId"] = currencyItems, 

                    ["CustomerId"] = customerItems, 

                    ["FarmId"] = farmItems,
                    ["FieldId"] = fields.Select(f => new SelectListItem
                    {
                        Value = f.FieldId.ToString(),
                        Text = string.IsNullOrWhiteSpace(f.FieldName) ? $"Field #{f.FieldId}" : f.FieldName
                    }).ToList(),

                    ["SaleTypeId"] = await _context.SaleTypes
                        .Where(x => x.IsActive) // Optional: only show active sale types
                        .Select(x => new SelectListItem { Value = x.SaleTypeId.ToString(), Text = x.SaleTypeName })
                        .ToListAsync(),

                    ["StatusId"] = await _context.Statuses
                        .Where(x => x.IsActive) // Optional: only show active statuses
                        .Select(x => new SelectListItem { Value = x.StatusId.ToString(), Text = x.StatusName })
                        .ToListAsync(),

                    ["UpdatedByUserId"] = userItems, 

                    ["UserId"] = userItems 
                };
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching dropdowns: {ex.Message}";
            }
            return response;
        }
    }
}
