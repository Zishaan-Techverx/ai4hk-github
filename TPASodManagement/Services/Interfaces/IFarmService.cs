using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;
using TpaSodManagement.Database.Entities;

namespace TpaSodManagement.Services.Interfaces
{
    public interface IFarmService
    {
        Task<ServiceResponse<List<Farm>>> GetAllAsync();
        Task<ServiceResponse<List<Farm>>> GetFilteredAsync(Dictionary<string, string> filters);
        Task<ServiceResponse<Farm>> GetByIdAsync(long id);
        Task<ServiceResponse<Farm>> CreateAsync(Farm farm);
        Task<ServiceResponse<Farm>> UpdateAsync(Farm farm);
        Task<ServiceResponse<bool>> DeleteAsync(long id, long? deletedByUserId);
        Task<ServiceResponse<SelectList>> GetAreaTypeDropdownAsync(int? selectedAreaTypeId = null);
    }
}
