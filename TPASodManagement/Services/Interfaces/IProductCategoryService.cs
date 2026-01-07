using System.Collections.Generic;
using System.Threading.Tasks;
using TpaSodManagement.Models.Db;

namespace TpaSodManagement.Services.Interfaces
{
    public interface IProductCategoryService
    {
        Task<ServiceResponse<List<ProductCategory>>> GetAllAsync();
        Task<ServiceResponse<List<ProductCategory>>> GetFilteredAsync(Dictionary<string, string> filters);
        Task<ServiceResponse<ProductCategory>> GetByIdAsync(int id);
        Task<ServiceResponse<ProductCategory>> CreateAsync(ProductCategory productCategory);
        Task<ServiceResponse<ProductCategory>> UpdateAsync(ProductCategory productCategory);
        Task<ServiceResponse<bool>> DeleteAsync(int id);
        Task<ServiceResponse<bool>> ExistsAsync(int id);
    }
}
