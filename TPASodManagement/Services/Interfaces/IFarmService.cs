using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;
using TpaSodManagement.Models.Db;

namespace TpaSodManagement.Services.Interfaces
{
    public interface IFarmService
    {
        Task<ServiceResponse<List<Farm>>> GetAllAsync();
        Task<ServiceResponse<Farm>> GetByIdAsync(long id);
        Task<ServiceResponse<Farm>> CreateAsync(Farm farm);
        Task<ServiceResponse<Farm>> UpdateAsync(Farm farm);
        Task<ServiceResponse<bool>> DeleteAsync(long id);
        Task<ServiceResponse<(SelectList AreaTypes, SelectList Organizations)>> GetDropdownDataAsync(long? selectedOrganizationId = null, int? selectedAreaTypeId = null);
    }
}
