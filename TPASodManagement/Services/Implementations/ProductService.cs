using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TpaSodManagement.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Database.Entities;
using TpaSodManagement.Database;
using TpaSodManagement.Utilities;

namespace TpaSodManagement.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<TpaSodManagementUser> _userManager;
        private readonly ICurrentUserService _currentUserService;

        public ProductService(ApplicationDbContext context, UserManager<TpaSodManagementUser> userManager, ICurrentUserService currentUserService)
        {
            _context = context;
            _userManager = userManager;
            _currentUserService = currentUserService;
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
                    .Include(p => p.Currency)
                    .ToListAsync();
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching products: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<List<Product>>> GetFilteredAsync(Dictionary<string, string> filters)
        {
            var response = new ServiceResponse<List<Product>>();
            try
            {
                var query = _context.Products
                    .Include(p => p.CertificateType)
                    .Include(p => p.CreatedByUser)
                    .Include(p => p.ProductCategory)
                    .Include(p => p.Currency)
                    .AsQueryable();

                // Apply filters
                if (filters != null && filters.Count > 0)
                {
                    if (filters.ContainsKey("ProductCode") && !string.IsNullOrWhiteSpace(filters["ProductCode"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["ProductCode"]);
                        query = query.Where(p => p.ProductCode != null && p.ProductCode.Contains(filterValue));
                    }

                    if (filters.ContainsKey("ProductName") && !string.IsNullOrWhiteSpace(filters["ProductName"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["ProductName"]);
                        query = query.Where(p => p.ProductName != null && p.ProductName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("UnitOfMeasure") && !string.IsNullOrWhiteSpace(filters["UnitOfMeasure"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["UnitOfMeasure"]);
                        query = query.Where(p => p.UnitOfMeasure != null && p.UnitOfMeasure.Contains(filterValue));
                    }

                    if (filters.ContainsKey("StandardPrice") && !string.IsNullOrWhiteSpace(filters["StandardPrice"]))
                    {
                        if (decimal.TryParse(filters["StandardPrice"], out decimal standardPrice))
                        {
                            query = query.Where(p => p.StandardPrice == standardPrice);
                        }
                    }

                    if (filters.ContainsKey("RequiresCertificate") && !string.IsNullOrWhiteSpace(filters["RequiresCertificate"]))
                    {
                        if (bool.TryParse(filters["RequiresCertificate"], out bool requiresCertificate))
                        {
                            query = query.Where(p => p.RequiresCertificate == requiresCertificate);
                        }
                    }

                    if (filters.ContainsKey("Description") && !string.IsNullOrWhiteSpace(filters["Description"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["Description"]);
                        query = query.Where(p => p.Description != null && p.Description.Contains(filterValue));
                    }

                    if (filters.ContainsKey("IsActive") && !string.IsNullOrWhiteSpace(filters["IsActive"]))
                    {
                        if (bool.TryParse(filters["IsActive"], out bool isActive))
                        {
                            query = query.Where(p => p.IsActive == isActive);
                        }
                    }

                    if (filters.ContainsKey("CertificateTypeName") && !string.IsNullOrWhiteSpace(filters["CertificateTypeName"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["CertificateTypeName"]);
                        query = query.Where(p => p.CertificateType != null && p.CertificateType.CertificateTypeName != null && p.CertificateType.CertificateTypeName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("CreatedByUserName") && !string.IsNullOrWhiteSpace(filters["CreatedByUserName"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["CreatedByUserName"]);
                        query = query.Where(p => p.CreatedByUser != null && p.CreatedByUser.UserName != null && p.CreatedByUser.UserName.Contains(filterValue));
                    }

                    if (filters.ContainsKey("CurrencyCode") && !string.IsNullOrWhiteSpace(filters["CurrencyCode"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["CurrencyCode"]);
                        query = query.Where(p => p.Currency != null && p.Currency.CurrencyCode != null && p.Currency.CurrencyCode.Contains(filterValue));
                    }

                    if (filters.ContainsKey("ProductCategoryName") && !string.IsNullOrWhiteSpace(filters["ProductCategoryName"]))
                    {
                        var filterValue = FilterHelper.NormalizeSearchText(filters["ProductCategoryName"]);
                        query = query.Where(p => p.ProductCategory != null && p.ProductCategory.CategoryName != null && p.ProductCategory.CategoryName.Contains(filterValue));
                    }

                    // Date range filters for CreatedDate
                    if (filters.ContainsKey("CreatedDate_From") && !string.IsNullOrWhiteSpace(filters["CreatedDate_From"]))
                    {
                        if (DateTimeOffset.TryParse(filters["CreatedDate_From"], out DateTimeOffset fromDate))
                        {
                            query = query.Where(p => p.CreatedDate >= fromDate);
                        }
                    }

                    if (filters.ContainsKey("CreatedDate_To") && !string.IsNullOrWhiteSpace(filters["CreatedDate_To"]))
                    {
                        if (DateTimeOffset.TryParse(filters["CreatedDate_To"], out DateTimeOffset toDate))
                        {
                            // Add one day to include the entire end date
                            toDate = toDate.AddDays(1).AddTicks(-1);
                            query = query.Where(p => p.CreatedDate <= toDate);
                        }
                    }
                }

                response.Data = await query.OrderBy(p => p.ProductName).ToListAsync();
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"Error filtering products: {ex.Message}";
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
                var currentUserId = await _currentUserService.GetCurrentUserIdAsync();
                product.CreatedDate = DateTimeOffset.UtcNow;
                product.CreatedByUserId = currentUserId;
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
                // Fetch existing entity from database to preserve CreatedByUserId and CreatedDate
                var existingProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == product.ProductId);
                
                if (existingProduct == null)
                {
                    response.Success = false;
                    response.Message = "Product not found";
                    return response;
                }

                // Update only the properties that should be updated
                // Preserve CreatedByUserId and CreatedDate
                existingProduct.ProductCode = product.ProductCode;
                existingProduct.ProductName = product.ProductName;
                existingProduct.Description = product.Description;
                existingProduct.UnitOfMeasure = product.UnitOfMeasure;
                existingProduct.StandardPrice = product.StandardPrice;
                existingProduct.ProductCategoryId = product.ProductCategoryId;
                existingProduct.CurrencyId = product.CurrencyId;
                existingProduct.CertificateTypeId = product.CertificateTypeId;
                existingProduct.IsActive = product.IsActive;
                
                // Set update audit fields
                var currentUserId = await _currentUserService.GetCurrentUserIdAsync();
                existingProduct.UpdatedDate = DateTimeOffset.UtcNow;
                existingProduct.UpdatedByUserId = currentUserId;
                
                // CreatedByUserId and CreatedDate are preserved from existingProduct
                
                await _context.SaveChangesAsync();
                response.Data = existingProduct;
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

        public async Task<ServiceResponse<bool>> DeleteAsync(long id, long? deletedByUserId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == id && p.DeletedDate == null);
                if (product == null)
                {
                    response.Success = false;
                    response.Message = "Product not found";
                    response.Data = false;
                    return response;
                }

                // Soft delete: Set DeletedDate and DeletedByUserId
                var currentUserId = deletedByUserId ?? await _currentUserService.GetCurrentUserIdAsync();
                product.DeletedDate = DateTimeOffset.UtcNow;
                product.DeletedByUserId = currentUserId;
                
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
                    
                // Use UserManager instead of _context.TpaUsers
                var users = await _userManager.Users
                    .OrderBy(u => u.UserName)
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
                // Note: TpaSodManagementUser uses Id (string) and UserName, not UserId and Username
                var userItems = users.Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.UserName ?? $"User #{u.Id}"
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
