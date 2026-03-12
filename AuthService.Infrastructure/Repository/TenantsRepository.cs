using Microsoft.EntityFrameworkCore;
using AuthService.Domain.Models;
using AuthService.Domain.Repository;
using AuthService.Infrastructure.Context;

namespace AuthService.Infrastructure.Repository
{
    public class TenantsRepository : ITenantsRepository
    {
        private readonly AuthDbContext _context;

        public TenantsRepository(AuthDbContext context)
        {
            _context = context;
        }

        public Tenants Create(Tenants tenant)
        {
            _context.Tenants.Add(tenant);
            return tenant;
        }

        public async Task<(IEnumerable<Tenants> Items, int TotalCount)> GetAllAsync(int page, int pageSize)
        {
            var query = _context.Tenants;
            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(t => t.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (items, totalCount);
        }

        public async Task<Tenants?> GetByIdAsync(int id)
        {
            return await _context.Tenants.FindAsync(id);
        }

        public async Task<bool> CodeExistsAsync(string code)
        {
            return await _context.Tenants.AnyAsync(t => t.Code == code);
        }

        public async Task<Tenants> CreateTenantAsync(Tenants tenant)
        {
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();
            return tenant;
        }

        public async Task<Tenants?> UpdateTenantAsync(int id, string name)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null) return null;

            tenant.Name = name;
            await _context.SaveChangesAsync();
            return tenant;
        }

        public async Task<bool> DeactivateTenantAsync(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null) return false;

            tenant.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
