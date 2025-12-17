using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.Home;

namespace TpaSodManagement.Services.Implementations
{
    public class HomeService : IHomeService
    {
        private readonly SodDbContext _context;

        public HomeService(SodDbContext context)
        {
            _context = context;
        }

        public async Task<HomeIndexViewModel> GetHomeIndexViewModelAsync(bool isAuthenticated)
        {
            var vm = new HomeIndexViewModel
            {
                Organizations = await _context.Organizations
                    .OrderBy(o => o.OrganizationName)
                    .ToListAsync(),
                IsAuthenticated = isAuthenticated
            };

            if (isAuthenticated)
            {
                vm.FarmCount = await _context.Farms.CountAsync();
                vm.CustomerCount = await _context.Customers.CountAsync();
                vm.ProductCount = await _context.Products.CountAsync();
                vm.SaleCount = await _context.Sales.CountAsync();
            }

            return vm;
        }
    }
}

