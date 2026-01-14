using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Database;
using TpaSodManagement.Database.Entities;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Services.Implementations
{
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CustomerService(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<ServiceResponse<List<Customer>>> GetAllAsync()
        {
            var response = new ServiceResponse<List<Customer>>();
            try
            {
                response.Data = await _context.Customers
                    .Include(c => c.Organization)
                    .Include(c => c.Person)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching customers: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<List<Customer>>> GetFilteredAsync(Dictionary<string, string> filters)
        {
            var response = new ServiceResponse<List<Customer>>();
            try
            {
                var query = _context.Customers
                    .Include(c => c.Organization)
                    .Include(c => c.Person)
                    .AsQueryable();

                // Apply filters
                if (filters != null && filters.Count > 0)
                {
                    if (filters.ContainsKey("CustomerType") && !string.IsNullOrWhiteSpace(filters["CustomerType"]))
                    {
                        var filterValue = filters["CustomerType"].Trim();
                        query = query.Where(c => c.CustomerType != null && c.CustomerType.Contains(filterValue));
                    }

                    if (filters.ContainsKey("CustomerCode") && !string.IsNullOrWhiteSpace(filters["CustomerCode"]))
                    {
                        var filterValue = filters["CustomerCode"].Trim();
                        query = query.Where(c => c.CustomerCode != null && c.CustomerCode.Contains(filterValue));
                    }

                    if (filters.ContainsKey("CreditLimit") && !string.IsNullOrWhiteSpace(filters["CreditLimit"]))
                    {
                        if (decimal.TryParse(filters["CreditLimit"], out decimal creditLimit))
                        {
                            query = query.Where(c => c.CreditLimit == creditLimit);
                        }
                    }

                    if (filters.ContainsKey("PaymentTermsDays") && !string.IsNullOrWhiteSpace(filters["PaymentTermsDays"]))
                    {
                        if (int.TryParse(filters["PaymentTermsDays"], out int paymentTerms))
                        {
                            query = query.Where(c => c.PaymentTermsDays == paymentTerms);
                        }
                    }

                    if (filters.ContainsKey("TaxExempt") && !string.IsNullOrWhiteSpace(filters["TaxExempt"]))
                    {
                        if (bool.TryParse(filters["TaxExempt"], out bool taxExempt))
                        {
                            query = query.Where(c => c.TaxExempt == taxExempt);
                        }
                    }

                    if (filters.ContainsKey("Notes") && !string.IsNullOrWhiteSpace(filters["Notes"]))
                    {
                        var filterValue = filters["Notes"].Trim();
                        query = query.Where(c => c.Notes != null && c.Notes.Contains(filterValue));
                    }

                    if (filters.ContainsKey("IsActive") && !string.IsNullOrWhiteSpace(filters["IsActive"]))
                    {
                        if (bool.TryParse(filters["IsActive"], out bool isActive))
                        {
                            query = query.Where(c => c.IsActive == isActive);
                        }
                        else if (filters["IsActive"].ToLower() == "true" || filters["IsActive"].ToLower() == "yes" || filters["IsActive"].ToLower() == "1")
                        {
                            query = query.Where(c => c.IsActive == true);
                        }
                        else if (filters["IsActive"].ToLower() == "false" || filters["IsActive"].ToLower() == "no" || filters["IsActive"].ToLower() == "0")
                        {
                            query = query.Where(c => c.IsActive == false);
                        }
                    }

                    if (filters.ContainsKey("Organization") && !string.IsNullOrWhiteSpace(filters["Organization"]))
                    {
                        var filterValue = filters["Organization"].Trim();
                        query = query.Where(c => c.Organization != null && c.Organization.OrganizationName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("Person") && !string.IsNullOrWhiteSpace(filters["Person"]))
                    {
                        var filterValue = filters["Person"].Trim();
                        query = query.Where(c => c.Person != null && 
                            (c.Person.FirstName.Contains(filterValue) || c.Person.LastName.Contains(filterValue)));
                    }

                    // Date range filters for CreatedDate
                    if (filters.ContainsKey("CreatedDate_From") && !string.IsNullOrWhiteSpace(filters["CreatedDate_From"]))
                    {
                        if (DateTimeOffset.TryParse(filters["CreatedDate_From"], out DateTimeOffset fromDate))
                        {
                            query = query.Where(c => c.CreatedDate >= fromDate);
                        }
                    }

                    if (filters.ContainsKey("CreatedDate_To") && !string.IsNullOrWhiteSpace(filters["CreatedDate_To"]))
                    {
                        if (DateTimeOffset.TryParse(filters["CreatedDate_To"], out DateTimeOffset toDate))
                        {
                            // Add one day to include the entire end date
                            toDate = toDate.AddDays(1).AddTicks(-1);
                            query = query.Where(c => c.CreatedDate <= toDate);
                        }
                    }

                    // Date range filters for UpdatedDate
                    if (filters.ContainsKey("UpdatedDate_From") && !string.IsNullOrWhiteSpace(filters["UpdatedDate_From"]))
                    {
                        if (DateTimeOffset.TryParse(filters["UpdatedDate_From"], out DateTimeOffset fromDate))
                        {
                            query = query.Where(c => c.UpdatedDate >= fromDate);
                        }
                    }

                    if (filters.ContainsKey("UpdatedDate_To") && !string.IsNullOrWhiteSpace(filters["UpdatedDate_To"]))
                    {
                        if (DateTimeOffset.TryParse(filters["UpdatedDate_To"], out DateTimeOffset toDate))
                        {
                            // Add one day to include the entire end date
                            toDate = toDate.AddDays(1).AddTicks(-1);
                            query = query.Where(c => c.UpdatedDate <= toDate);
                        }
                    }
                }

                response.Data = await query.OrderBy(c => c.CustomerCode).ToListAsync();
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error filtering customers: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<Customer>> GetByIdAsync(long id)
        {
            var response = new ServiceResponse<Customer>();
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.Organization)
                    .Include(c => c.Person)
                    .FirstOrDefaultAsync(c => c.CustomerId == id);

                if (customer == null)
                {
                    response.Success = false;
                    response.Message = "Customer not found";
                }
                else
                {
                    response.Data = customer;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching customer: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<Customer>> CreateAsync(Customer customer)
        {
            var response = new ServiceResponse<Customer>();
            try
            {
                var currentUserId = await _currentUserService.GetCurrentUserIdAsync();
                customer.CreatedDate = DateTimeOffset.UtcNow;
                customer.CreatedByUserId = currentUserId;
                _context.Add(customer);
                await _context.SaveChangesAsync();
                response.Data = customer;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error creating customer: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<Customer>> UpdateAsync(Customer customer)
        {
            var response = new ServiceResponse<Customer>();
            try
            {
                var exists = await _context.Customers.AnyAsync(c => c.CustomerId == customer.CustomerId);
                if (!exists)
                {
                    response.Success = false;
                    response.Message = "Customer not found";
                    return response;
                }

                var currentUserId = await _currentUserService.GetCurrentUserIdAsync();
                customer.UpdatedDate = DateTimeOffset.UtcNow;
                customer.UpdatedByUserId = currentUserId;
                _context.Update(customer);
                await _context.SaveChangesAsync();
                response.Data = customer;
            }
            catch (DbUpdateConcurrencyException)
            {
                response.Success = false;
                response.Message = "Concurrency error updating customer";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error updating customer: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteAsync(long id, long? deletedByUserId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.CustomerId == id && c.DeletedDate == null);
                if (customer == null)
                {
                    response.Success = false;
                    response.Message = "Customer not found";
                    response.Data = false;
                    return response;
                }

                // Soft delete: Set DeletedDate and DeletedByUserId
                var currentUserId = deletedByUserId ?? await _currentUserService.GetCurrentUserIdAsync();
                customer.DeletedDate = DateTimeOffset.UtcNow;
                customer.DeletedByUserId = currentUserId;
                
                await _context.SaveChangesAsync();
                response.Data = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error deleting customer: {ex.Message}";
                response.Data = false;
            }
            return response;
        }

        public async Task<ServiceResponse<(SelectList Organizations, SelectList People)>> GetCreateViewDataAsync()
        {
            var response = new ServiceResponse<(SelectList Organizations, SelectList People)>();
            try
            {
                var orgs = await _context.Organizations
                    .OrderBy(o => o.OrganizationName)
                    .ToListAsync();
            
                var people = await _context.People
                    .OrderBy(p => p.FirstName)
                    .ThenBy(p => p.LastName)
                    .ToListAsync();
            
                // Create SelectList for Organizations
                var orgItems = orgs.Select(o => new SelectListItem
                {
                    Value = o.OrganizationId.ToString(),
                    Text = o.OrganizationName
                }).ToList();
            
                // Create SelectList for People with display name (FirstName LastName)
                var peopleItems = people.Select(p =>
                {
                    var fullName = $"{p.FirstName} {p.LastName}".Trim();
                    var displayName = string.IsNullOrEmpty(fullName) 
                        ? $"Person #{p.PersonId}" 
                        : fullName;
                        
                    return new SelectListItem
                    {
                        Value = p.PersonId.ToString(), // Backend par PersonId jayega
                        Text = displayName // Dropdown mein name show hoga
                    };
                }).ToList();
            
                response.Data = (
                    new SelectList(orgItems, "Value", "Text"),
                    new SelectList(peopleItems, "Value", "Text")
                );
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
