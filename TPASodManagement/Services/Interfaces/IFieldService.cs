using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;
using TpaSodManagement.Models.Db;

namespace TpaSodManagement.Services.Interfaces
{
    public interface IFieldService
    {
        Task<ServiceResponse<List<Field>>> GetAllAsync();
        Task<ServiceResponse<Field>> GetByIdAsync(long id);
        Task<ServiceResponse<Field>> CreateAsync(Field field);
        Task<ServiceResponse<Field>> UpdateAsync(Field field);
        Task<ServiceResponse<bool>> DeleteAsync(long id);
        Task<ServiceResponse<(SelectList Farms, SelectList AreaTypes, SelectList Users)>> GetDropdownDataAsync();
        Task<ServiceResponse<bool>> ExisTpasync(long id);
    }
}

