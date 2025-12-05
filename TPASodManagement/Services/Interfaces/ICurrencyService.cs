using System.Collections.Generic;
using System.Threading.Tasks;
using TpaSodManagement.Models.Db;

namespace TpaSodManagement.Services.Interfaces
{
    public interface ICurrencyService
    {
        Task<ServiceResponse<List<Currency>>> GetAllAsync();
        Task<ServiceResponse<Currency>> GetByIdAsync(int id);
        Task<ServiceResponse<Currency>> CreateAsync(Currency currency);
        Task<ServiceResponse<Currency>> UpdateAsync(Currency currency);
        Task<ServiceResponse<bool>> DeleteAsync(int id);
        Task<ServiceResponse<bool>> ExisTpasync(int id);
    }
}