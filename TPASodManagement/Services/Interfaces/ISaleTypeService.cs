using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using TpaSodManagement.Database.Entities;

namespace TpaSodManagement.Services.Interfaces
{
    public interface ISaleTypeService
    {
        Task<ServiceResponse<List<SaleType>>> GetAllAsync();
        Task<ServiceResponse<List<SaleType>>> GetFilteredAsync(Dictionary<string, string> filters);
        Task<ServiceResponse<SaleType>> GetByIdAsync(int id);
        Task<ServiceResponse<SaleType>> CreateAsync(SaleType saleType);
        Task<ServiceResponse<SaleType>> UpdateAsync(SaleType saleType);
        Task<ServiceResponse<bool>> DeleteAsync(int id, long? deletedByUserId);
        Task<ServiceResponse<bool>> ExisTpasync(int id);
        Task<ServiceResponse<IEnumerable<SelectListItem>>> GetCertificateTypesForDropdownAsync();
    }
}
