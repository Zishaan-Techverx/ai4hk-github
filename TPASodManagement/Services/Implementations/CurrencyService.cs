using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Database;
using TpaSodManagement.Utilities;
using TpaSodManagement.Database.Entities;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Services.Implementations
{
    public class CurrencyService : ICurrencyService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CurrencyService(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<ServiceResponse<List<Currency>>> GetAllAsync()
        {
            var response = new ServiceResponse<List<Currency>>();
            try
            {
                response.Data = await _context.Currencies
                    .OrderBy(c => c.CurrencyName)
                    .ToListAsync();
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching currencies: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<List<Currency>>> GetFilteredAsync(Dictionary<string, string> filters)
        {
            var response = new ServiceResponse<List<Currency>>();
            try
            {
                var query = _context.Currencies.AsQueryable();

                // Apply filters
                if (filters != null && filters.Count > 0)
                {
                    if (filters.ContainsKey("CurrencyCode") && !string.IsNullOrWhiteSpace(filters["CurrencyCode"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["CurrencyCode"]);
                        query = query.Where(c => c.CurrencyCode.Contains(filterValue));
                    }

                    if (filters.ContainsKey("CurrencyName") && !string.IsNullOrWhiteSpace(filters["CurrencyName"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["CurrencyName"]);
                        query = query.Where(c => c.CurrencyName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("CurrencySymbol") && !string.IsNullOrWhiteSpace(filters["CurrencySymbol"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["CurrencySymbol"]);
                        query = query.Where(c => c.CurrencySymbol != null && c.CurrencySymbol.Contains(filterValue));
                    }

                    if (filters.ContainsKey("DecimalPlaces") && !string.IsNullOrWhiteSpace(filters["DecimalPlaces"]))
                    {
                        if (byte.TryParse(filters["DecimalPlaces"], out byte decimalPlaces))
                        {
                            query = query.Where(c => c.DecimalPlaces == decimalPlaces);
                        }
                    }

                    if (filters.ContainsKey("IsActive") && !string.IsNullOrWhiteSpace(filters["IsActive"]))
                    {
                        if (bool.TryParse(filters["IsActive"], out bool isActive))
                        {
                            query = query.Where(c => c.IsActive == isActive);
                        }
                    }
                }

                response.Data = await query.OrderBy(c => c.CurrencyName).ToListAsync();
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error filtering currencies: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<Currency>> GetByIdAsync(int id)
        {
            var response = new ServiceResponse<Currency>();
            try
            {
                var currency = await _context.Currencies.FindAsync(id);

                if (currency == null)
                {
                    response.Success = false;
                    response.Message = "Currency not found";
                }
                else
                {
                    response.Data = currency;
                }
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching currency: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<Currency>> CreateAsync(Currency currency)
        {
            var response = new ServiceResponse<Currency>();
            try
            {
                // Check if currency code already exists
                var exists = await _context.Currencies
                    .AnyAsync(c => c.CurrencyCode.ToUpper() == currency.CurrencyCode.ToUpper());

                if (exists)
                {
                    response.Success = false;
                    response.Message = "Currency code already exists";
                    return response;
                }

                var currentUserId = await _currentUserService.GetCurrentUserIdAsync();
                currency.CreatedDate = DateTimeOffset.UtcNow;
                currency.CreatedByUserId = currentUserId;
                _context.Add(currency);
                await _context.SaveChangesAsync();
                response.Data = currency;
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error creating currency: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<Currency>> UpdateAsync(Currency currency)
        {
            var response = new ServiceResponse<Currency>();
            try
            {
                // Fetch existing entity from database to preserve CreatedByUserId and CreatedDate
                var existingCurrency = await _context.Currencies
                    .FirstOrDefaultAsync(c => c.CurrencyId == currency.CurrencyId);
                
                if (existingCurrency == null)
                {
                    response.Success = false;
                    response.Message = "Currency not found";
                    return response;
                }

                // Check if currency code already exists (excluding current currency)
                var codeExists = await _context.Currencies
                    .AnyAsync(c => c.CurrencyCode.ToUpper() == currency.CurrencyCode.ToUpper() 
                        && c.CurrencyId != currency.CurrencyId);

                if (codeExists)
                {
                    response.Success = false;
                    response.Message = "Currency code already exists";
                    return response;
                }

                // Update only the properties that should be updated
                // Preserve CreatedByUserId and CreatedDate
                existingCurrency.CurrencyCode = currency.CurrencyCode;
                existingCurrency.CurrencyName = currency.CurrencyName;
                existingCurrency.CurrencySymbol = currency.CurrencySymbol;
                existingCurrency.IsActive = currency.IsActive;
                
                // Set update audit fields
                var currentUserId = await _currentUserService.GetCurrentUserIdAsync();
                existingCurrency.UpdatedDate = DateTimeOffset.UtcNow;
                existingCurrency.UpdatedByUserId = currentUserId;
                
                // CreatedByUserId and CreatedDate are preserved from existingCurrency
                
                await _context.SaveChangesAsync();
                response.Data = existingCurrency;
            }
            catch (DbUpdateConcurrencyException)
            {
                response.Success = false;
                response.Message = "Concurrency error updating currency";
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error updating currency: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteAsync(int id, long? deletedByUserId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var currency = await _context.Currencies
                    .FirstOrDefaultAsync(c => c.CurrencyId == id && c.DeletedDate == null);

                if (currency == null)
                {
                    response.Success = false;
                    response.Message = "Currency not found";
                    response.Data = false;
                    return response;
                }

                // Soft delete: Set DeletedDate and DeletedByUserId
                var currentUserId = deletedByUserId ?? await _currentUserService.GetCurrentUserIdAsync();
                currency.DeletedDate = DateTimeOffset.UtcNow;
                currency.DeletedByUserId = currentUserId;
                
                await _context.SaveChangesAsync();
                response.Data = true;
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error deleting currency: {ex.Message}";
                response.Data = false;
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> ExisTpasync(int id)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                response.Data = await _context.Currencies.AnyAsync(c => c.CurrencyId == id);
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error checking currency existence: {ex.Message}";
            }
            return response;
        }
    }
}