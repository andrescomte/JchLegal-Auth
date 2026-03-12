using AuthService.Domain.Models;
using AuthService.Domain.SeedWork;

namespace AuthService.Domain.Repository
{
    public interface ITenantsRepository : IEntityRepository<Tenants>
    {
        Task<(IEnumerable<Tenants> Items, int TotalCount)> GetAllAsync(int page, int pageSize);
        Task<Tenants?> GetByIdAsync(int id);
        Task<bool> CodeExistsAsync(string code);
        Task<Tenants> CreateTenantAsync(Tenants tenant);
        Task<Tenants?> UpdateTenantAsync(int id, string name);
        Task<bool> DeactivateTenantAsync(int id);
    }
}
