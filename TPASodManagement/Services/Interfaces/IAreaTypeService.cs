using System.Collections.Generic;
using System.Threading.Tasks;
using TpaSodManagement.Models.Db;

namespace TpaSodManagement.Services.Interfaces
{
    public interface IAreaTypeService
    {
        Task<ServiceResponse<List<AreaType>>> GetAllAsync();
        Task<ServiceResponse<List<AreaType>>> GetFilteredAsync(Dictionary<string, string> filters);
        Task<ServiceResponse<AreaType>> GetByIdAsync(int id);
        Task<ServiceResponse<AreaType>> CreateAsync(AreaType areaType);
        Task<ServiceResponse<AreaType>> UpdateAsync(AreaType areaType);
        Task<ServiceResponse<bool>> DeleteAsync(int id);
        Task<ServiceResponse<bool>> ExisTpasync(int id);
    }
}
