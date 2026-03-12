using System.Diagnostics.CodeAnalysis;
using AuthService.Domain.Models;
using AuthService.Domain.Repository;

namespace AuthService.UnitTest.Mocks
{
    [ExcludeFromCodeCoverage]
    public class TenantsRepositoryFake : ITenantsRepository
    {
        public static readonly int ExistingId = 1;
        public static readonly string ExistingCode = "default";

        private readonly List<Tenants> _tenants = new()
        {
            new Tenants
            {
                Id = ExistingId,
                Code = ExistingCode,
                Name = "Tenant por defecto",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        public Tenants Create(Tenants tenant) => tenant;

        public async Task<(IEnumerable<Tenants> Items, int TotalCount)> GetAllAsync(int page, int pageSize)
        {
            await Task.CompletedTask;
            var items = _tenants.Skip((page - 1) * pageSize).Take(pageSize);
            return (items, _tenants.Count);
        }

        public async Task<Tenants?> GetByIdAsync(int id)
        {
            await Task.CompletedTask;
            return _tenants.FirstOrDefault(t => t.Id == id);
        }

        public async Task<bool> CodeExistsAsync(string code)
        {
            await Task.CompletedTask;
            return _tenants.Any(t => t.Code == code);
        }

        public async Task<Tenants> CreateTenantAsync(Tenants tenant)
        {
            await Task.CompletedTask;
            tenant.Id = _tenants.Max(t => t.Id) + 1;
            _tenants.Add(tenant);
            return tenant;
        }

        public async Task<Tenants?> UpdateTenantAsync(int id, string name)
        {
            await Task.CompletedTask;
            var tenant = _tenants.FirstOrDefault(t => t.Id == id);
            if (tenant == null) return null;
            tenant.Name = name;
            return tenant;
        }

        public async Task<bool> DeactivateTenantAsync(int id)
        {
            await Task.CompletedTask;
            var tenant = _tenants.FirstOrDefault(t => t.Id == id);
            if (tenant == null) return false;
            tenant.IsActive = false;
            return true;
        }
    }
}
