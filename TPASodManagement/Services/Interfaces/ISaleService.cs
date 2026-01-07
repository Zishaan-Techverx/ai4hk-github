using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;
using TpaSodManagement.Models.Db;

namespace TpaSodManagement.Services.Interfaces
{
    public interface ISaleService
    {
        Task<ServiceResponse<List<Sale>>> GetAllAsync();
        Task<ServiceResponse<List<Sale>>> GetFilteredAsync(Dictionary<string, string> filters);
        Task<ServiceResponse<Sale>> GetByIdAsync(long id);
        Task<ServiceResponse<Sale>> CreateAsync(Sale sale);
        Task<ServiceResponse<Sale>> UpdateAsync(Sale sale);
        Task<ServiceResponse<bool>> DeleteAsync(long id);
        Task<ServiceResponse<Dictionary<string, IEnumerable<SelectListItem>>>> GetDropdownDataAsync(long? selectedIds = null);
    }
}
