using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TpaSodManagement.Data;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Services.Implementations
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly SodDbContext _context;

        public ProductCategoryService(SodDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<List<ProductCategory>>> GetAllAsync()
        {
            var response = new ServiceResponse<List<ProductCategory>>();
            try
            {
                response.Data = await _context.ProductCategories
                    .OrderBy(pc => pc.CategoryName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching product categories: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<List<ProductCategory>>> GetFilteredAsync(Dictionary<string, string> filters)
        {
            var response = new ServiceResponse<List<ProductCategory>>();
            try
            {
                var query = _context.ProductCategories.AsQueryable();

                // Apply filters
                if (filters != null && filters.Count > 0)
                {
                    if (filters.ContainsKey("CategoryCode") && !string.IsNullOrWhiteSpace(filters["CategoryCode"]))
                    {
                        var filterValue = filters["CategoryCode"].Trim();
                        query = query.Where(pc => pc.CategoryCode != null && pc.CategoryCode.Contains(filterValue));
                    }

                    if (filters.ContainsKey("CategoryName") && !string.IsNullOrWhiteSpace(filters["CategoryName"]))
                    {
                        var filterValue = filters["CategoryName"].Trim();
                        query = query.Where(pc => pc.CategoryName != null && pc.CategoryName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("Description") && !string.IsNullOrWhiteSpace(filters["Description"]))
                    {
                        var filterValue = filters["Description"].Trim();
                        query = query.Where(pc => pc.Description != null && pc.Description.Contains(filterValue));
                    }

                    if (filters.ContainsKey("IsActive") && !string.IsNullOrWhiteSpace(filters["IsActive"]))
                    {
                        if (bool.TryParse(filters["IsActive"], out bool isActive))
                        {
                            query = query.Where(pc => pc.IsActive == isActive);
                        }
                    }
                }

                response.Data = await query.OrderBy(pc => pc.CategoryName).ToListAsync();
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error filtering product categories: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<ProductCategory>> GetByIdAsync(int id)
        {
            var response = new ServiceResponse<ProductCategory>();
            try
            {
                var productCategory = await _context.ProductCategories
                    .FirstOrDefaultAsync(pc => pc.ProductCategoryId == id);

                if (productCategory == null)
                {
                    response.Success = false;
                    response.Message = "Product category not found";
                }
                else
                {
                    response.Data = productCategory;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching product category: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<ProductCategory>> CreateAsync(ProductCategory productCategory)
        {
            var response = new ServiceResponse<ProductCategory>();
            try
            {
                // Check if CategoryCode already exists
                var exists = await _context.ProductCategories
                    .AnyAsync(pc => pc.CategoryCode == productCategory.CategoryCode);
                
                if (exists)
                {
                    response.Success = false;
                    response.Message = "Category code already exists";
                    return response;
                }

                productCategory.CreatedDate = DateTimeOffset.Now;
                _context.Add(productCategory);
                await _context.SaveChangesAsync();
                response.Data = productCategory;
            }
            catch (DbUpdateException ex)
            {
                response.Success = false;
                response.Message = $"Error creating product category: {ex.Message}";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error creating product category: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<ProductCategory>> UpdateAsync(ProductCategory productCategory)
        {
            var response = new ServiceResponse<ProductCategory>();
            try
            {
                var exists = await _context.ProductCategories
                    .AnyAsync(pc => pc.ProductCategoryId == productCategory.ProductCategoryId);
                
                if (!exists)
                {
                    response.Success = false;
                    response.Message = "Product category not found";
                    return response;
                }

                // Check if CategoryCode already exists for another category
                var codeExists = await _context.ProductCategories
                    .AnyAsync(pc => pc.CategoryCode == productCategory.CategoryCode && 
                                    pc.ProductCategoryId != productCategory.ProductCategoryId);
                
                if (codeExists)
                {
                    response.Success = false;
                    response.Message = "Category code already exists for another category";
                    return response;
                }

                _context.Update(productCategory);
                await _context.SaveChangesAsync();
                response.Data = productCategory;
            }
            catch (DbUpdateConcurrencyException)
            {
                response.Success = false;
                response.Message = "Concurrency error updating product category";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error updating product category: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteAsync(int id)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var productCategory = await _context.ProductCategories
                    .Include(pc => pc.Products)
                    .FirstOrDefaultAsync(pc => pc.ProductCategoryId == id);
                
                if (productCategory == null)
                {
                    response.Success = false;
                    response.Message = "Product category not found";
                    response.Data = false;
                    return response;
                }

                // Check if category has associated products
                if (productCategory.Products != null && productCategory.Products.Any())
                {
                    response.Success = false;
                    response.Message = "Cannot delete product category as it has associated products";
                    response.Data = false;
                    return response;
                }

                _context.ProductCategories.Remove(productCategory);
                await _context.SaveChangesAsync();
                response.Data = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error deleting product category: {ex.Message}";
                response.Data = false;
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> ExistsAsync(int id)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                response.Data = await _context.ProductCategories
                    .AnyAsync(pc => pc.ProductCategoryId == id);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error checking product category existence: {ex.Message}";
                response.Data = false;
            }
            return response;
        }
    }
}
