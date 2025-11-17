using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Services.Implementations
{
    public class SaleService : ISaleService
    {
        private readonly SodDbContext _context;

        public SaleService(SodDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<List<Sale>>> GetAllAsync()
        {
            var response = new ServiceResponse<List<Sale>>();
            try
            {
                response.Data = await _context.Sales
                    .Include(s => s.Currency)
                    .Include(s => s.Customer)
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

        public async Task<ServiceResponse<bool>> DeleteAsync(long id)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var sale = await _context.Sales.FindAsync(id);
                if (sale == null)
                {
                    response.Success = false;
                    response.Message = "Sale not found";
                    response.Data = false;
                    return response;
                }

                _context.Sales.Remove(sale);
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
                response.Data = new Dictionary<string, IEnumerable<SelectListItem>>
                {
                    ["CurrencyId"] = await _context.Currencies
                        .Select(x => new SelectListItem { Value = x.CurrencyId.ToString(), Text = x.CurrencyId.ToString() })
                        .ToListAsync(),

                    ["CustomerId"] = await _context.Customers
                        .Select(x => new SelectListItem { Value = x.CustomerId.ToString(), Text = x.CustomerId.ToString() })
                        .ToListAsync(),

                    ["FarmId"] = await _context.Farms
                        .Select(x => new SelectListItem { Value = x.FarmId.ToString(), Text = x.FarmId.ToString() })
                        .ToListAsync(),

                    ["SaleTypeId"] = await _context.SaleTypes
                        .Select(x => new SelectListItem { Value = x.SaleTypeId.ToString(), Text = x.SaleTypeId.ToString() })
                        .ToListAsync(),

                    ["StatusId"] = await _context.Statuses
                        .Select(x => new SelectListItem { Value = x.StatusId.ToString(), Text = x.StatusId.ToString() })
                        .ToListAsync(),

                    ["UpdatedByUserId"] = await _context.TpaUsers
                        .Select(x => new SelectListItem { Value = x.UserId.ToString(), Text = x.UserId.ToString() })
                        .ToListAsync(),

                    ["UserId"] = await _context.TpaUsers
                        .Select(x => new SelectListItem { Value = x.UserId.ToString(), Text = x.UserId.ToString() })
                        .ToListAsync()
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
