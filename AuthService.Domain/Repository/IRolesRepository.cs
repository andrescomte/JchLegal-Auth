using AuthService.Domain.Models;
using AuthService.Domain.SeedWork;

namespace AuthService.Domain.Repository
{
    public interface IRolesRepository : IEntityRepository<Roles>
    {
        Task<(List<Roles> Items, int TotalCount)> ReadAll(int page, int pageSize);
        Task<Roles?> GetByIdAsync(int id);
        Task<Roles?> GetByCodeAsync(string code);
        Task<Roles> CreateRoleAsync(Roles role);
        Task<Roles?> UpdateRoleAsync(int id, string name);
        Task<bool> ExistsAsync(string code);
    }
}
