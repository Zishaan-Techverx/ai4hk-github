using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TpaSodManagement.Data;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Services.Implementations
{
    public class CustomerService : ICustomerService
    {
        private readonly SodDbContext _context;

        public CustomerService(SodDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<List<Customer>>> GetAllAsync()
        {
            var response = new ServiceResponse<List<Customer>>();
            try
            {
                response.Data = await _context.Customers
                    .Include(c => c.Organization)
                    .Include(c => c.Person)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching customers: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<Customer>> GetByIdAsync(long id)
        {
            var response = new ServiceResponse<Customer>();
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.Organization)
                    .Include(c => c.Person)
                    .FirstOrDefaultAsync(c => c.CustomerId == id);

                if (customer == null)
                {
                    response.Success = false;
                    response.Message = "Customer not found";
                }
                else
                {
                    response.Data = customer;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching customer: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<Customer>> CreateAsync(Customer customer)
        {
            var response = new ServiceResponse<Customer>();
            try
            {
                _context.Add(customer);
                await _context.SaveChangesAsync();
                response.Data = customer;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error creating customer: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<Customer>> UpdateAsync(Customer customer)
        {
            var response = new ServiceResponse<Customer>();
            try
            {
                var exists = await _context.Customers.AnyAsync(c => c.CustomerId == customer.CustomerId);
                if (!exists)
                {
                    response.Success = false;
                    response.Message = "Customer not found";
                    return response;
                }

                _context.Update(customer);
                await _context.SaveChangesAsync();
                response.Data = customer;
            }
            catch (DbUpdateConcurrencyException)
            {
                response.Success = false;
                response.Message = "Concurrency error updating customer";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error updating customer: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteAsync(long id)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                {
                    response.Success = false;
                    response.Message = "Customer not found";
                    response.Data = false;
                    return response;
                }

                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                response.Data = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error deleting customer: {ex.Message}";
                response.Data = false;
            }
            return response;
        }

        public async Task<ServiceResponse<(SelectList Organizations, SelectList People)>> GetCreateViewDataAsync()
        {
            var response = new ServiceResponse<(SelectList Organizations, SelectList People)>();
            try
            {
                var orgs = await _context.Organizations.ToListAsync();
                var people = await _context.People.ToListAsync();
                response.Data = (
                    new SelectList(orgs, "OrganizationId", "OrganizationId"),
                    new SelectList(people, "PersonId", "PersonId")
                );
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching dropdowns: {ex.Message}";
            }
            return response;
        }
    }
}
