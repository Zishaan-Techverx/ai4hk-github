using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Database;
using TpaSodManagement.Services.Interfaces;
using TpaSodManagement.ViewModels.Home;

namespace TpaSodManagement.Services.Implementations
{
    public class HomeService : IHomeService
    {
        private readonly ApplicationDbContext _context;

        public HomeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<HomeIndexViewModel> GetHomeIndexViewModelAsync(bool isAuthenticated)
        {
            var vm = new HomeIndexViewModel
            {
                Farms = await _context.Farms
                    .Where(f => f.DeletedDate == null && f.IsActive)
                    .OrderBy(f => f.FarmName)
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

