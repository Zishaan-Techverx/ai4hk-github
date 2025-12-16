using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TpaSodManagement.Areas.Identity.Data;
using TpaSodManagement.Models.Db;
using TpaSodManagement.Services.Interfaces;

namespace TpaSodManagement.Services.Implementations
{
    public class RegistrationService : IRegistrationService
    {
        private readonly UserManager<TpaSodManagementUser> _userManager;
        private readonly SodDbContext _context;
        private readonly ILogger<RegistrationService> _logger;

        public RegistrationService(
            UserManager<TpaSodManagementUser> userManager,
            SodDbContext context,
            ILogger<RegistrationService> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public async Task<List<Organization>> GetAllOrganizationsAsync()
        {
            return await _context.Organizations
                .OrderBy(o => o.OrganizationName)
                .ToListAsync();
        }

        public async Task<Organization?> GetOrganizationByNameAsync(string organizationName)
        {
            return await _context.Organizations
                .FirstOrDefaultAsync(o => o.OrganizationName.ToUpper() == organizationName.ToUpper());
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            return existingUser != null;
        }

        public async Task<string> GenerateUsernameAsync(string organizationName, string firstName)
        {
            string rawOrgName = organizationName.Replace(" ", "");
            const int OrgPrefixLength = 5;
            string orgPrefix = rawOrgName.Length >= OrgPrefixLength
                               ? rawOrgName.Substring(0, OrgPrefixLength)
                               : rawOrgName;

            orgPrefix = orgPrefix.ToUpper();

            string userInitials;
            string[] nameParts = firstName.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

            if (nameParts.Length >= 2)
            {
                userInitials = (nameParts[0][0].ToString() + nameParts[^1][0].ToString()).ToUpper();
            }
            else if (firstName.Length >= 2)
            {
                userInitials = firstName.Substring(0, 2).ToUpper();
            }
            else
            {
                userInitials = firstName.ToUpper();
            }

            string baseUsername = $"{orgPrefix}-{userInitials}";
            string finalUsername = baseUsername;
            int counter = 1;

            while (await _userManager.FindByNameAsync(finalUsername) != null)
            {
                finalUsername = $"{baseUsername}{counter++}";
            }

            return finalUsername;
        }

        public async Task<IdentityResult> CreateUserAsync(TpaSodManagementUser user, string password)
        {
            try
            {
                // Ensure all required Identity fields are explicitly set to avoid NULL insertion errors
                if (string.IsNullOrEmpty(user.PhoneNumber))
                {
                    user.PhoneNumber = string.Empty;
                }

                // Ensure boolean fields are explicitly set
                if (user.IsActive == default(bool))
                {
                    user.IsActive = true;
                }

                // Explicitly set all Identity required fields to prevent NULL insertion
                // Use direct assignment instead of || operator to ensure values are always set
                user.EmailConfirmed = false;
                user.PhoneNumberConfirmed = false;
                user.TwoFactorEnabled = false;
                user.LockoutEnabled = false;
                user.AccessFailedCount = 0; // Explicitly set to 0, don't check for default

                _logger.LogInformation("Creating user with AccessFailedCount: {Count}, EmailConfirmed: {EmailConfirmed}",
                    user.AccessFailedCount, user.EmailConfirmed);

                return await _userManager.CreateAsync(user, password);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user: {Email}. Inner exception: {InnerException}",
                    user.Email, ex.InnerException?.Message);
                throw;
            }
        }

        public async Task<Person> CreatePersonForUserAsync(TpaSodManagementUser user, string firstName, string lastName)
        {
            var person = new Person
            {
                FirstName = firstName ?? "",
                LastName = lastName ?? "",
                MiddleName = null,
                DateOfBirth = null,
                Gender = null,
                Title = null,
                Bio = null,
                IsPrimaryContact = false,
                CreatedDate = DateTimeOffset.UtcNow,
                UpdatedDate = DateTimeOffset.UtcNow
            };
            _context.People.Add(person);
            await _context.SaveChangesAsync();

            user.PersonId = person.PersonId;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Person record created successfully for user {Email}", user.Email);

            return person;
        }

        public async Task<Address> CreateAddressForUserAsync(
            TpaSodManagementUser user,
            string? addressLine1 = null,
            string? state = null,
            string? country = null,
            string? postalCode = null)
        {
            var defaultAddressType = await _context.AddressTypes
                .FirstOrDefaultAsync(at => at.IsActive);

            if (defaultAddressType == null)
            {
                defaultAddressType = await _context.AddressTypes.FirstOrDefaultAsync();
            }

            // Find or Create StateProvince ONLY if state parameter is provided
            // During registration, StateProvinceId will be null by default
            StateProvince? stateProvince = null;

            if (!string.IsNullOrWhiteSpace(state))
            {
                stateProvince = await _context.StateProvinces
                    .FirstOrDefaultAsync(sp => sp.StateName.ToUpper() == state.ToUpper() && sp.IsActive);

                if (stateProvince == null)
                {
                    stateProvince = await _context.StateProvinces
                        .FirstOrDefaultAsync(sp => sp.StateName.ToUpper() == state.ToUpper());
                }

                if (stateProvince == null)
                {
                    var defaultCountry = await _context.Countries
                        .FirstOrDefaultAsync(c => c.IsActive);

                    if (defaultCountry == null)
                    {
                        defaultCountry = await _context.Countries.FirstOrDefaultAsync();
                    }

                    // If country parameter is provided, try to find country by name
                    if (!string.IsNullOrWhiteSpace(country) && defaultCountry != null)
                    {
                        var countryByName = await _context.Countries
                            .FirstOrDefaultAsync(c => c.CountryName.ToUpper() == country.ToUpper());

                        if (countryByName != null)
                        {
                            defaultCountry = countryByName;
                        }
                    }

                    if (defaultCountry != null)
                    {
                        stateProvince = new StateProvince
                        {
                            CountryId = defaultCountry.CountryId,
                            StateCode = state.Length > 10 ? state.Substring(0, 10).ToUpper() : state.ToUpper(),
                            StateName = state,
                            IsActive = true,
                            CreatedDate = DateTimeOffset.UtcNow
                        };
                        _context.StateProvinces.Add(stateProvince);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            // Remove the default StateProvince fallback - keep StateProvinceId as null during registration
            // User can add it later during update

            var address = new Address
            {
                AddressTypeId = defaultAddressType?.AddressTypeId ?? 1,
                AddressLine1 = addressLine1 ?? "",
                City = "", // City separately
                StateProvinceId = stateProvince?.StateProvinceId, // Will be null if state parameter not provided
                PostalCode = postalCode ?? "",
                AddressLine2 = null,
                Latitude = null,
                Longitude = null,
                IsPrimary = false,
                IsVerified = false,
                IsActive = true,
                VerificationDate = null,
                CreatedDate = DateTimeOffset.UtcNow,
                UpdatedDate = DateTimeOffset.UtcNow
            };
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            user.AddressId = address.AddressId;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Address record created successfully for user {Email} with StateProvinceId: {StateProvinceId}",
                user.Email, address.StateProvinceId?.ToString() ?? "null");

            return address;
        }

        public async Task<Website> CreateWebsiteForUserAsync(TpaSodManagementUser user, string username)
        {
            var website = new Website
            {
                WebsiteUrl = "",
                WebsiteType = null,
                IsPrimary = false,
                IsActive = true,
                CreatedDate = DateTimeOffset.UtcNow,
                UpdatedDate = DateTimeOffset.UtcNow
            };
            _context.Websites.Add(website);
            await _context.SaveChangesAsync();


            user.WebsiteId = website.WebsiteId;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Empty Website record created successfully for user {Email}", user.Email);

            return website;
        }

        public async Task<Farm> CreateOrGetFarmForOrganizationAsync(long organizationId)
        {

            var farm = await _context.Farms
                .FirstOrDefaultAsync(f => f.OrganizationId == organizationId);

            if (farm == null)
            {

                farm = new Farm
                {
                    OrganizationId = organizationId,
                    TotalArea = null,
                    AreaTypeId = null,
                    OrganicCertified = false,
                    LicenseNumber = null,
                    CertificationDetails = null,
                    Latitude = null,
                    Longitude = null,
                    ElevationMeters = null,
                    SoilType = null,
                    IrrigationType = null,
                    ClimateZone = null
                };
                _context.Farms.Add(farm);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Empty Farm record created successfully for organization {OrganizationId}", organizationId);
            }

            return farm;
        }

        public async Task<TpaSodManagementUser?> GetUserWithAddressAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null && user.AddressId.HasValue)
            {

                var address = await _context.Addresses
                    .Include(a => a.AddressType)
                    .Include(a => a.StateProvince)
                    .FirstOrDefaultAsync(a => a.AddressId == user.AddressId.Value);

            }

            return user;
        }

        public async Task<Address?> GetUserAddressAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user?.AddressId.HasValue == true)
            {
                return await _context.Addresses
                    .Include(a => a.AddressType)
                    .Include(a => a.StateProvince)
                    .FirstOrDefaultAsync(a => a.AddressId == user.AddressId.Value);
            }

            return null;
        }

        public async Task<Person?> GetUserPersonAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user?.PersonId.HasValue == true)
            {
                return await _context.People
                    .FirstOrDefaultAsync(p => p.PersonId == user.PersonId.Value);
            }

            return null;
        }

        public async Task<Website?> GetUserWebsiteAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user?.WebsiteId.HasValue == true)
            {
                return await _context.Websites
                    .FirstOrDefaultAsync(w => w.WebsiteId == user.WebsiteId.Value);
            }

            return null;
        }

        public async Task<Farm?> GetUserFarmAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user?.FarmId.HasValue == true)
            {
                return await _context.Farms
                    .Include(f => f.Organization)
                    .Include(f => f.AreaType)
                    .FirstOrDefaultAsync(f => f.FarmId == user.FarmId.Value);
            }

            return null;
        }
    }
}

