using Microsoft.EntityFrameworkCore;
using AuthService.Domain.Models;
using AuthService.Domain.Repository;
using AuthService.Domain.SeedWork;
using AuthService.Infrastructure.Context;

namespace AuthService.Infrastructure.Repository
{
    public class RolesRepository : IRolesRepository
    {
        private readonly AuthDbContext _context;
        private readonly IAppLogger<RolesRepository> _logger;

        public RolesRepository(AuthDbContext context, IAppLogger<RolesRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Roles Create(Roles roleObject)
        {
            _logger.LogInformation("Creating new Roles entity");
            _context.Roles.Add(roleObject);
            return roleObject;
        }

        public async Task<(List<Roles> Items, int TotalCount)> ReadAll(int page, int pageSize)
        {
            _logger.LogInformation("Reading all Roles entities");
            var totalCount = await _context.Roles.CountAsync();
            var items = await _context.Roles
                .OrderBy(r => r.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (items, totalCount);
        }

        public async Task<Roles?> GetByIdAsync(int id)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Roles?> GetByCodeAsync(string code)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.Code == code);
        }

        public async Task<Roles> CreateRoleAsync(Roles role)
        {
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<Roles?> UpdateRoleAsync(int id, string name)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);
            if (role == null)
                return null;

            role.Name = name;
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<bool> ExistsAsync(string code)
        {
            return await _context.Roles.AnyAsync(r => r.Code == code);
        }
    }
}
