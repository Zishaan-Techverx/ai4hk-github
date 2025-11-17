using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using TpaSodManagement.Models.Db;

namespace TpaSodManagement.Services.Interfaces
{
    public interface IProductService
    {
        Task<ServiceResponse<List<Product>>> GetAllAsync();
        Task<ServiceResponse<Product>> GetByIdAsync(long id);
        Task<ServiceResponse<Product>> CreateAsync(Product product);
        Task<ServiceResponse<Product>> UpdateAsync(Product product);
        Task<ServiceResponse<bool>> DeleteAsync(long id);
        Task<ServiceResponse<(SelectList CertificateTypes, SelectList Users, SelectList Currencies, SelectList Categories)>> GetDropdownDataAsync();
        Task<ServiceResponse<bool>> ExisTpasync(long id);
    }
}
