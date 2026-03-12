using AuthService.Domain.Models;
using AuthService.Domain.SeedWork;

namespace AuthService.Domain.Repository
{
    public interface ITokensRepository : IEntityRepository<Tokens>
    {
        Task<Tokens> CreateTokenAsync(Tokens token);
        Task<Tokens?> GetByHashAsync(string tokenHash);
        Task UpdateTokenAsync(Tokens token);
        Task RevokeAllByUserIdAsync(long userId);
    }
}

