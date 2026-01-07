using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using TpaSodManagement.Models.Db;

namespace TpaSodManagement.Services.Interfaces
{
    public interface ISeedingService
    {
        Task<ServiceResponse<List<Seeding>>> GetAllAsync();
        Task<ServiceResponse<List<Seeding>>> GetFilteredAsync(Dictionary<string, string> filters);
        Task<ServiceResponse<Seeding>> GetByIdAsync(long id);
        Task<ServiceResponse<Seeding>> CreateAsync(Seeding seeding);
        Task<ServiceResponse<Seeding>> UpdateAsync(Seeding seeding);
        Task<ServiceResponse<bool>> DeleteAsync(long id);
        Task<ServiceResponse<(SelectList AreaTypes, SelectList Farms, SelectList Fields, SelectList TagRanges, SelectList Users)>> GetDropdownDataAsync();
        Task<ServiceResponse<bool>> ExisTpasync(long id);
    }
}
