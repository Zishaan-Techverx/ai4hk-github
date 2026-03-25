using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;
using TpaSodManagement.Database.Entities;

namespace TpaSodManagement.Services.Interfaces
{
    public interface IFieldService
    {
        Task<ServiceResponse<List<Field>>> GetAllAsync();
        Task<ServiceResponse<List<Field>>> GetFilteredAsync(Dictionary<string, string> filters);
        Task<ServiceResponse<Field>> GetByIdAsync(long id);
        Task<ServiceResponse<Field>> CreateAsync(Field field);
        Task<ServiceResponse<Field>> UpdateAsync(Field field);
        Task<ServiceResponse<bool>> DeleteAsync(long id, long? deletedByUserId);
        Task<ServiceResponse<(SelectList Farms, SelectList AreaTypes, SelectList FieldTypes, SelectList Users)>> GetDropdownDataAsync();
        Task<ServiceResponse<bool>> ExisTpasync(long id);
    }
}

