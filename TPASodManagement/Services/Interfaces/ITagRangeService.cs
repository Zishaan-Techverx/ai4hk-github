using System.Collections.Generic;
using System.Threading.Tasks;
using TpaSodManagement.Database.Entities;

namespace TpaSodManagement.Services.Interfaces
{
    public interface ITagRangeService
    {
        Task<ServiceResponse<List<TagRange>>> GetAllAsync();
        Task<ServiceResponse<List<TagRange>>> GetFilteredAsync(Dictionary<string, string> filters);
        Task<ServiceResponse<TagRange>> GetByIdAsync(long id);
        Task<ServiceResponse<TagRange>> CreateAsync(TagRange tagRange);
        Task<ServiceResponse<TagRange>> UpdateAsync(TagRange tagRange);
        Task<ServiceResponse<bool>> DeleteAsync(long id, long? deletedByUserId);
        Task<ServiceResponse<bool>> ExisTpasync(long id);
    }
}
