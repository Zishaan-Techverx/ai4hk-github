using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly SodDbContext _context;

        public ProductService(SodDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<List<Product>>> GetAllAsync()
        {
            var response = new ServiceResponse<List<Product>>();
            try
            {
                response.Data = await _context.Products
                    .Include(p => p.CertificateType)
                    .Include(p => p.CreatedByUser)
                    .Include(p => p.Currency)
                    .Include(p => p.ProductCategory)
                    .ToListAsync();
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching products: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<Product>> GetByIdAsync(long id)
        {
            var response = new ServiceResponse<Product>();
            try
            {
                var product = await _context.Products
                    .Include(p => p.CertificateType)
                    .Include(p => p.CreatedByUser)
                    .Include(p => p.Currency)
                    .Include(p => p.ProductCategory)
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (product == null)
                {
                    response.Success = false;
                    response.Message = "Product not found";
                }
                else
                {
                    response.Data = product;
                }
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching product: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<Product>> CreateAsync(Product product)
        {
            var response = new ServiceResponse<Product>();
            try
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                response.Data = product;
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error creating product: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<Product>> UpdateAsync(Product product)
        {
            var response = new ServiceResponse<Product>();
            try
            {
                var exists = await _context.Products.AnyAsync(p => p.ProductId == product.ProductId);
                if (!exists)
                {
                    response.Success = false;
                    response.Message = "Product not found";
                    return response;
                }

                _context.Update(product);
                await _context.SaveChangesAsync();
                response.Data = product;
            }
            catch (DbUpdateConcurrencyException)
            {
                response.Success = false;
                response.Message = "Concurrency error updating product";
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error updating product: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteAsync(long id)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    response.Success = false;
                    response.Message = "Product not found";
                    response.Data = false;
                    return response;
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                response.Data = true;
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error deleting product: {ex.Message}";
                response.Data = false;
            }
            return response;
        }

        public async Task<ServiceResponse<(SelectList CertificateTypes, SelectList Users, SelectList Currencies, SelectList Categories)>> GetDropdownDataAsync()
        {
            var response = new ServiceResponse<(SelectList, SelectList, SelectList, SelectList)>();
            try
            {
                var certs = await _context.CertificateTypes.ToListAsync();
                var users = await _context.TpaUsers.ToListAsync();
                var currencies = await _context.Currencies.ToListAsync();
                var categories = await _context.ProductCategories.ToListAsync();

                response.Data = (
                    new SelectList(certs, "CertificateTypeId", "CertificateTypeId"),
                    new SelectList(users, "UserId", "UserId"),
                    new SelectList(currencies, "CurrencyId", "CurrencyId"),
                    new SelectList(categories, "ProductCategoryId", "ProductCategoryId")
                );
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching dropdowns: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> ExisTpasync(long id)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                response.Data = await _context.Products.AnyAsync(p => p.ProductId == id);
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error checking product existence: {ex.Message}";
            }
            return response;
        }
    }
}
