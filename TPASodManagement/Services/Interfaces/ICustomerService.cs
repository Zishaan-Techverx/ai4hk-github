using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;
using TpaSodManagement.Models.Db;

namespace TpaSodManagement.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<ServiceResponse<List<Customer>>> GetAllAsync();
        Task<ServiceResponse<List<Customer>>> GetFilteredAsync(Dictionary<string, string> filters);
        Task<ServiceResponse<Customer>> GetByIdAsync(long id);
        Task<ServiceResponse<Customer>> CreateAsync(Customer customer);
        Task<ServiceResponse<Customer>> UpdateAsync(Customer customer);
        Task<ServiceResponse<bool>> DeleteAsync(long id);
        Task<ServiceResponse<(SelectList Organizations, SelectList People)>> GetCreateViewDataAsync();
    }
}
