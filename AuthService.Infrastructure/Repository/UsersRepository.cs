using Microsoft.EntityFrameworkCore;
using AuthService.Domain.Models;
using AuthService.Domain.Repository;
using AuthService.Domain.Services;
using AuthService.Infrastructure.Context;

namespace AuthService.Infrastructure.Repository
{
    public class UsersRepository : IUsersRepository
    {
        private readonly AuthDbContext _context;
        private readonly ITenantContext _tenantContext;

        public UsersRepository(AuthDbContext context, ITenantContext tenantContext)
        {
            _context = context;
            _tenantContext = tenantContext;
        }

        public Users Create(Users user)
        {
            _context.Users.Add(user);
            return user;
        }

        public async Task<Users?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.UserPasswords)
                .Include(u => u.UserStatusHistory
                    .OrderByDescending(h => h.StartedAt)
                    .Take(1))
                    .ThenInclude(h => h.Status)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email && u.TenantId == _tenantContext.TenantId);
        }

        public async Task<Users?> GetByIdAsync(long id)
        {
            return await _context.Users
                .Include(u => u.UserPasswords)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == _tenantContext.TenantId);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email && u.TenantId == _tenantContext.TenantId);
        }

        public async Task<int> GetStatusIdByCodeAsync(string code)
        {
            var status = await _context.UserStatus.FirstOrDefaultAsync(s => s.Code == code)
                ?? throw new InvalidOperationException($"User status '{code}' not found.");
            return status.Id;
        }

        public async Task<Users> CreateUserAsync(Users user, string passwordHash, int roleId, int statusId)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync(); // needed to generate user.Id

            _context.UserPasswords.Add(new UserPasswords
            {
                UserId = user.Id,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            });

            _context.UserRoles.Add(new UserRoles
            {
                UserId = user.Id,
                RoleId = roleId,
                IsPrimary = true
            });

            _context.UserStatusHistory.Add(new UserStatusHistory
            {
                UserId = user.Id,
                StatusId = statusId,
                StartedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return user;
        }

        public async Task AssignRoleAsync(long userId, int roleId)
        {
            var exists = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (!exists)
            {
                _context.UserRoles.Add(new UserRoles
                {
                    UserId = userId,
                    RoleId = roleId,
                    IsPrimary = false
                });
                await _context.SaveChangesAsync();
            }
        }

        public async Task RevokeRoleAsync(long userId, int roleId)
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (userRole != null)
            {
                _context.UserRoles.Remove(userRole);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdatePasswordAsync(UserPasswords userPasswords)
        {
            _context.UserPasswords.Update(userPasswords);
            await _context.SaveChangesAsync();
        }

        public async Task<(IEnumerable<Users> Items, int TotalCount)> GetAllAsync(int page, int pageSize)
        {
            var query = _context.Users
                .Where(u => u.TenantId == _tenantContext.TenantId);

            var totalCount = await query.CountAsync();

            var items = await query
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.UserStatusHistory
                    .OrderByDescending(h => h.StartedAt)
                    .Take(1))
                    .ThenInclude(h => h.Status)
                .OrderBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task UpdateUserAsync(Users user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeactivateUserAsync(long userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            user.IsActive = false;

            var inactiveStatusId = await GetStatusIdByCodeAsync("INACTIVE");
            await AddStatusHistoryAsync(userId, inactiveStatusId);

            await _context.SaveChangesAsync();
        }

        public async Task UnlockUserAsync(long userId)
        {
            var userPassword = await _context.UserPasswords.FirstOrDefaultAsync(p => p.UserId == userId);
            if (userPassword != null)
                userPassword.FailedAttempts = 0;

            var activeStatusId = await GetStatusIdByCodeAsync("ACTIVE");
            await AddStatusHistoryAsync(userId, activeStatusId);
        }

        public async Task AddStatusHistoryAsync(long userId, int statusId)
        {
            var previous = await _context.UserStatusHistory
                .Where(h => h.UserId == userId && h.EndedAt == null)
                .ToListAsync();

            foreach (var h in previous)
                h.EndedAt = DateTime.UtcNow;

            _context.UserStatusHistory.Add(new UserStatusHistory
            {
                UserId = userId,
                StatusId = statusId,
                StartedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }
    }
}
