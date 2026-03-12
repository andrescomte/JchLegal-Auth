using System.Diagnostics.CodeAnalysis;
using AuthService.Domain.Models;
using AuthService.Domain.Repository;

namespace AuthService.UnitTest.Mocks
{
    [ExcludeFromCodeCoverage]
    public class UsersRepositoryFake : IUsersRepository
    {
        private readonly List<Users> _users = new()
        {
            new Users
            {
                Id = 1,
                TenantId = 1,
                Username = "testuser",
                Email = UserServiceFake.ValidEmail,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Tenant = new Tenants { Id = 1, Code = "default", Name = "Tenant por defecto" },
                UserPasswords = new UserPasswords { Id = 1, UserId = 1, PasswordHash = "hashed", FailedAttempts = 0 },
                UserRoles = new List<UserRoles>
                {
                    new UserRoles { Role = new Roles { Id = 1, Code = "ADMIN", Name = "Administrador" } }
                },
                UserStatusHistory = new List<UserStatusHistory>
                {
                    new UserStatusHistory { Status = new UserStatus { Code = "ACTIVE", Name = "Activo" } }
                }
            }
        };

        public Users Create(Users user) => user;

        public async Task<(IEnumerable<Users> Items, int TotalCount)> GetAllAsync(int page, int pageSize)
        {
            await Task.CompletedTask;
            var items = _users.Skip((page - 1) * pageSize).Take(pageSize);
            return (items, _users.Count);
        }

        public async Task<Users?> GetByEmailAsync(string email)
        {
            await Task.CompletedTask;
            return _users.FirstOrDefault(u => u.Email == email);
        }

        public async Task<Users?> GetByIdAsync(long id)
        {
            await Task.CompletedTask;
            return _users.FirstOrDefault(u => u.Id == id);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            await Task.CompletedTask;
            return _users.Any(u => u.Email == email);
        }

        public async Task<int> GetStatusIdByCodeAsync(string code) { await Task.CompletedTask; return 1; }

        public async Task<Users> CreateUserAsync(Users user, string passwordHash, int roleId, int statusId)
        {
            await Task.CompletedTask;
            user.Id = _users.Max(u => u.Id) + 1;
            _users.Add(user);
            return user;
        }

        public async Task AssignRoleAsync(long userId, int roleId) { await Task.CompletedTask; }

        public async Task RevokeRoleAsync(long userId, int roleId) { await Task.CompletedTask; }

        public async Task UpdatePasswordAsync(UserPasswords userPasswords) { await Task.CompletedTask; }

        public async Task AddStatusHistoryAsync(long userId, int statusId) { await Task.CompletedTask; }

        public async Task UpdateUserAsync(Users user)
        {
            await Task.CompletedTask;
            var existing = _users.FirstOrDefault(u => u.Id == user.Id);
            if (existing == null) return;
            existing.Username = user.Username;
            existing.Email = user.Email;
        }

        public async Task DeactivateUserAsync(long userId)
        {
            await Task.CompletedTask;
            var user = _users.FirstOrDefault(u => u.Id == userId);
            if (user != null) user.IsActive = false;
        }

        public async Task UnlockUserAsync(long userId)
        {
            await Task.CompletedTask;
            var user = _users.FirstOrDefault(u => u.Id == userId);
            if (user?.UserPasswords != null)
            {
                user.UserPasswords.FailedAttempts = 0;
                user.UserStatusHistory = new List<UserStatusHistory>
                {
                    new UserStatusHistory { Status = new UserStatus { Code = "ACTIVE", Name = "Activo" } }
                };
            }
        }
    }
}
