using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TpaSodManagement.Data;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Services.Implementations
{
    public class CurrencyService : ICurrencyService
    {
        private readonly SodDbContext _context;

        public CurrencyService(SodDbContext context)
        {
            _context = context;
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
                        var filterValue = filters["CurrencyCode"].Trim();
                        query = query.Where(c => c.CurrencyCode.Contains(filterValue));
                    }

                    if (filters.ContainsKey("CurrencyName") && !string.IsNullOrWhiteSpace(filters["CurrencyName"]))
                    {
                        var filterValue = filters["CurrencyName"].Trim();
                        query = query.Where(c => c.CurrencyName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("CurrencySymbol") && !string.IsNullOrWhiteSpace(filters["CurrencySymbol"]))
                    {
                        var filterValue = filters["CurrencySymbol"].Trim();
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

                currency.CreatedDate = System.DateTimeOffset.UtcNow;
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
                var exists = await _context.Currencies.AnyAsync(c => c.CurrencyId == currency.CurrencyId);
                if (!exists)
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

                _context.Update(currency);
                await _context.SaveChangesAsync();
                response.Data = currency;
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

        public async Task<ServiceResponse<bool>> DeleteAsync(int id)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var currency = await _context.Currencies
                    .Include(c => c.Products)
                    .Include(c => c.Sales)
                    .Include(c => c.Wastes)
                    .FirstOrDefaultAsync(c => c.CurrencyId == id);

                if (currency == null)
                {
                    response.Success = false;
                    response.Message = "Currency not found";
                    response.Data = false;
                    return response;
                }

                // Check if currency is used in any products
                if (currency.Products != null && currency.Products.Any())
                {
                    response.Success = false;
                    response.Message = "Cannot delete currency as it is being used in products";
                    response.Data = false;
                    return response;
                }

                // Check if currency is used in any sales
                if (currency.Sales != null && currency.Sales.Any())
                {
                    response.Success = false;
                    response.Message = "Cannot delete currency as it is being used in sales";
                    response.Data = false;
                    return response;
                }

                _context.Currencies.Remove(currency);
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