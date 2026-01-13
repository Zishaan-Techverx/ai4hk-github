using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Database.Entities;
using TpaSodManagement.Database;

namespace TpaSodManagement.Services.Implementations
{
    public class SaleService : ISaleService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<TpaSodManagementUser> _userManager;

        public SaleService(ApplicationDbContext context, UserManager<TpaSodManagementUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ServiceResponse<List<Sale>>> GetAllAsync()
        {
            var response = new ServiceResponse<List<Sale>>();
            try
            {
                response.Data = await _context.Sales
                    .Include(s => s.Currency)
                    .Include(s => s.Customer)
                        .ThenInclude(c => c.Person)
                    .Include(s => s.Customer)
                        .ThenInclude(c => c.Organization)
                    .Include(s => s.Farm)
                    .Include(s => s.SaleType)
                    .Include(s => s.Status)
                    .Include(s => s.UpdatedByUser)
                    .Include(s => s.User)
                    .ToListAsync();
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
                    .Include(s => s.Currency)
                    .Include(s => s.Customer)
                        .ThenInclude(c => c.Person)
                    .Include(s => s.Customer)
                        .ThenInclude(c => c.Organization)
                    .Include(s => s.Farm)
                    .Include(s => s.SaleType)
                    .Include(s => s.Status)
                    .Include(s => s.UpdatedByUser)
                    .Include(s => s.User)
                    .AsQueryable();

                // Apply filters
                if (filters != null && filters.Count > 0)
                {
                    if (filters.ContainsKey("SaleNumber") && !string.IsNullOrWhiteSpace(filters["SaleNumber"]))
                    {
                        var filterValue = filters["SaleNumber"].Trim();
                        query = query.Where(s => s.SaleNumber != null && s.SaleNumber.Contains(filterValue));
                    }

                    if (filters.ContainsKey("InvoiceNumber") && !string.IsNullOrWhiteSpace(filters["InvoiceNumber"]))
                    {
                        var filterValue = filters["InvoiceNumber"].Trim();
                        query = query.Where(s => s.InvoiceNumber != null && s.InvoiceNumber.Contains(filterValue));
                    }

                    if (filters.ContainsKey("PurchaseOrderNumber") && !string.IsNullOrWhiteSpace(filters["PurchaseOrderNumber"]))
                    {
                        var filterValue = filters["PurchaseOrderNumber"].Trim();
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
                        var filterValue = filters["Notes"].Trim();
                        query = query.Where(s => s.Notes != null && s.Notes.Contains(filterValue));
                    }

                    if (filters.ContainsKey("CurrencyName") && !string.IsNullOrWhiteSpace(filters["CurrencyName"]))
                    {
                        var filterValue = filters["CurrencyName"].Trim();
                        query = query.Where(s => s.Currency != null && s.Currency.CurrencyName != null && s.Currency.CurrencyName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("CustomerDisplay") && !string.IsNullOrWhiteSpace(filters["CustomerDisplay"]))
                    {
                        var filterValue = filters["CustomerDisplay"].Trim();
                        query = query.Where(s => (s.Customer != null && s.Customer.Person != null && 
                            (s.Customer.Person.FirstName != null && s.Customer.Person.FirstName.Contains(filterValue) ||
                             s.Customer.Person.LastName != null && s.Customer.Person.LastName.Contains(filterValue))) ||
                            (s.Customer != null && s.Customer.Organization != null && s.Customer.Organization.OrganizationName != null && s.Customer.Organization.OrganizationName.Contains(filterValue)) ||
                            (s.Customer != null && s.Customer.CustomerCode != null && s.Customer.CustomerCode.Contains(filterValue)));
                    }

                    if (filters.ContainsKey("FarmLicenseNumber") && !string.IsNullOrWhiteSpace(filters["FarmLicenseNumber"]))
                    {
                        var filterValue = filters["FarmLicenseNumber"].Trim();
                        query = query.Where(s => s.Farm != null && s.Farm.LicenseNumber != null && s.Farm.LicenseNumber.Contains(filterValue));
                    }

                    if (filters.ContainsKey("SaleTypeName") && !string.IsNullOrWhiteSpace(filters["SaleTypeName"]))
                    {
                        var filterValue = filters["SaleTypeName"].Trim();
                        query = query.Where(s => s.SaleType != null && s.SaleType.SaleTypeName != null && s.SaleType.SaleTypeName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("StatusName") && !string.IsNullOrWhiteSpace(filters["StatusName"]))
                    {
                        var filterValue = filters["StatusName"].Trim();
                        query = query.Where(s => s.Status != null && s.Status.StatusName != null && s.Status.StatusName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("UpdatedByUserName") && !string.IsNullOrWhiteSpace(filters["UpdatedByUserName"]))
                    {
                        var filterValue = filters["UpdatedByUserName"].Trim();
                        query = query.Where(s => s.UpdatedByUser != null && s.UpdatedByUser.UserName != null && s.UpdatedByUser.UserName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("UserName") && !string.IsNullOrWhiteSpace(filters["UserName"]))
                    {
                        var filterValue = filters["UserName"].Trim();
                        query = query.Where(s => s.User != null && s.User.UserName != null && s.User.UserName.Contains(filterValue));
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
                    .Include(s => s.Currency)
                    .Include(s => s.Customer)
                    .Include(s => s.Farm)
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

        public async Task<ServiceResponse<Sale>> CreateAsync(Sale sale)
        {
            var response = new ServiceResponse<Sale>();
            try
            {
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
                var exists = await _context.Sales.AnyAsync(s => s.SaleId == sale.SaleId);
                if (!exists)
                {
                    response.Success = false;
                    response.Message = "Sale not found";
                    return response;
                }

                _context.Sales.Update(sale);
                await _context.SaveChangesAsync();
                response.Data = sale;
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
                sale.DeletedDate = DateTimeOffset.UtcNow;
                sale.DeletedByUserId = deletedByUserId;
                
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
                // TpaSodManagementUser se users fetch karein
                var users = await _userManager.Users
                    .OrderBy(u => u.UserName)
                    .ToListAsync();

                // Fetch all Person records for these users in one query (efficient batch loading)
                var userIds = users.Where(u => u.PersonId.HasValue).Select(u => u.PersonId.Value).ToList();
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

                // Farms fetch karein with Organization
                var farms = await _context.Farms
                    .Include(f => f.Organization)
                    .OrderBy(f => f.FarmId)
                    .ToListAsync();

                // Create SelectList for Farms with display name (LicenseNumber or Farm ID with Organization)
                var farmItems = farms.Select(f => new SelectListItem
                {
                    Value = f.FarmId.ToString(),
                    Text = !string.IsNullOrEmpty(f.LicenseNumber)
                        ? $"{f.LicenseNumber} ({(f.Organization != null ? f.Organization.OrganizationName : "N/A")})"
                        : $"Farm #{f.FarmId} ({(f.Organization != null ? f.Organization.OrganizationName : "N/A")})"
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

                // Customers fetch karein with Person and Organization for display name
                var customers = await _context.Customers
                    .Include(c => c.Person)
                    .Include(c => c.Organization)
                    .Where(c => c.IsActive) // Only active customers
                    .OrderBy(c => c.CustomerId)
                    .ToListAsync();

                // Create SelectList for Customers with display name
                var customerItems = customers.Select(c =>
                {
                    string displayName;
                    
                    // Case-insensitive CustomerType check
                    var customerType = c.CustomerType?.ToUpper() ?? "";
                    
                    if ((customerType == "PERSON" || customerType == "P") && c.Person != null)
                    {
                        // Person customer - show FirstName LastName
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
                            // If name not available, use CustomerCode or CustomerId
                            displayName = !string.IsNullOrEmpty(c.CustomerCode)
                                ? c.CustomerCode
                                : $"Customer #{c.CustomerId}";
                        }
                    }
                    else if ((customerType == "ORG" || customerType == "ORGANIZATION" || customerType == "O") && c.Organization != null)
                    {
                        // Organization customer - show OrganizationName
                        var orgName = c.Organization.OrganizationName?.Trim() ?? "";
                        
                        if (!string.IsNullOrEmpty(orgName))
                        {
                            displayName = orgName;
                        }
                        else
                        {
                            // If name not available, use CustomerCode or CustomerId
                            displayName = !string.IsNullOrEmpty(c.CustomerCode)
                                ? c.CustomerCode
                                : $"Customer #{c.CustomerId}";
                        }
                    }
                    else
                    {
                        // Try to get name from Person or Organization even if CustomerType doesn't match
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
