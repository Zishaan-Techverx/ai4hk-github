using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using TpaSodManagement.Data;
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
                var certs = await _context.CertificateTypes
                    .OrderBy(c => c.CertificateTypeId)
                    .ToListAsync();
                    
                var users = await _context.TpaUsers
                    .OrderBy(u => u.Username)
                    .ToListAsync();
                    
                var categories = await _context.ProductCategories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.CategoryName)
                    .ToListAsync();

                // Get currencies from database instead of CurrencyHelper
                var currencies = await _context.Currencies
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.CurrencyName)
                    .ToListAsync();

                // Create SelectList for CertificateTypes
                var certItems = certs.Select(c => new SelectListItem
                {
                    Value = c.CertificateTypeId.ToString(),
                    Text = c.CertificateTypeName ?? $"Certificate #{c.CertificateTypeId}"
                }).ToList();

                // Create SelectList for Users
                var userItems = users.Select(u => new SelectListItem
                {
                    Value = u.UserId.ToString(),
                    Text = u.Username ?? $"User #{u.UserId}"
                }).ToList();

                // Create SelectList for Categories
                var categoryItems = categories.Select(c => new SelectListItem
                {
                    Value = c.ProductCategoryId.ToString(),
                    Text = c.CategoryName
                }).ToList();

                // Create SelectList for Currencies
                var currencyItems = currencies.Select(c => new SelectListItem
                {
                    Value = c.CurrencyId.ToString(),
                    Text = c.CurrencyName
                }).ToList();

                response.Data = (
                    new SelectList(certItems, "Value", "Text"),
                    new SelectList(userItems, "Value", "Text"),
                    new SelectList(currencyItems, "Value", "Text"),
                    new SelectList(categoryItems, "Value", "Text")
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
