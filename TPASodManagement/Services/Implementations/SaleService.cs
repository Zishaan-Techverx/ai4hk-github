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
    public class SaleService : ISaleService
    {
        private readonly SodDbContext _context;
        private readonly UserManager<TpaSodManagementUser> _userManager;

        public SaleService(SodDbContext context, UserManager<TpaSodManagementUser> userManager)
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
                // TpaSodManagementUser se users fetch karein
                var users = await _userManager.Users
                    .OrderBy(u => u.UserName)
                    .ToListAsync();

                // Create SelectList for Users with display name (FirstName LastName or UserName)
                var userItems = users.Select(u => new SelectListItem
                {
                    Value = u.Id, // TpaSodManagementUser ka Id string type hai
                    Text = !string.IsNullOrEmpty(u.FirstName) && !string.IsNullOrEmpty(u.LastName)
                        ? $"{u.FirstName} {u.LastName} ({u.UserName})"
                        : u.UserName ?? $"User #{u.Id}"
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

                response.Data = new Dictionary<string, IEnumerable<SelectListItem>>
                {
                    ["CurrencyId"] = currencyItems, // CurrencyName show hoga

                    ["CustomerId"] = await _context.Customers
                        .Select(x => new SelectListItem { Value = x.CustomerId.ToString(), Text = x.CustomerId.ToString() })
                        .ToListAsync(),

                    ["FarmId"] = farmItems, // LicenseNumber aur OrganizationName show hoga

                    ["SaleTypeId"] = await _context.SaleTypes
                        .Select(x => new SelectListItem { Value = x.SaleTypeId.ToString(), Text = x.SaleTypeId.ToString() })
                        .ToListAsync(),

                    ["StatusId"] = await _context.Statuses
                        .Select(x => new SelectListItem { Value = x.StatusId.ToString(), Text = x.StatusId.ToString() })
                        .ToListAsync(),

                    ["UpdatedByUserId"] = userItems, // TpaSodManagementUser se

                    ["UserId"] = userItems // TpaSodManagementUser se
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
