using AuthService.Domain.Models;
using AuthService.Domain.SeedWork;

namespace AuthService.Domain.Repository
{
    public interface IUsersRepository : IEntityRepository<Users>
    {
        Task<Users?> GetByEmailAsync(string email);
        Task<Users?> GetByIdAsync(long id);
        Task<bool> EmailExistsAsync(string email);
        Task<int> GetStatusIdByCodeAsync(string code);
        Task<Users> CreateUserAsync(Users user, string passwordHash, int roleId, int statusId);
        Task AssignRoleAsync(long userId, int roleId);
        Task RevokeRoleAsync(long userId, int roleId);
        Task UpdatePasswordAsync(UserPasswords userPasswords);
        Task AddStatusHistoryAsync(long userId, int statusId);
        Task<(IEnumerable<Users> Items, int TotalCount)> GetAllAsync(int page, int pageSize);
        Task UpdateUserAsync(Users user);
        Task DeactivateUserAsync(long userId);
        Task UnlockUserAsync(long userId);
    }
}
