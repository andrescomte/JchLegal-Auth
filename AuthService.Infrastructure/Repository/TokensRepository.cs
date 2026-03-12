using Microsoft.EntityFrameworkCore;
using AuthService.Domain.Models;
using AuthService.Domain.Repository;
using AuthService.Infrastructure.Context;

namespace AuthService.Infrastructure.Repository
{
    public class TokensRepository : ITokensRepository
    {
        private readonly AuthDbContext _context;

        public TokensRepository(AuthDbContext context)
        {
            _context = context;
        }

        public Tokens Create(Tokens token)
        {
            _context.Tokens.Add(token);
            return token;
        }

        public async Task<Tokens> CreateTokenAsync(Tokens token)
        {
            _context.Tokens.Add(token);
            await _context.SaveChangesAsync();
            return token;
        }

        public async Task<Tokens?> GetByHashAsync(string tokenHash)
        {
            return await _context.Tokens
                .Include(t => t.User)
                    .ThenInclude(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                .Include(t => t.User)
                    .ThenInclude(u => u.Tenant)
                .Include(t => t.User)
                    .ThenInclude(u => u.UserPasswords)
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
        }

        public async Task UpdateTokenAsync(Tokens token)
        {
            _context.Tokens.Update(token);
            await _context.SaveChangesAsync();
        }

        public async Task RevokeAllByUserIdAsync(long userId)
        {
            var activeTokens = await _context.Tokens
                .Where(t => t.UserId == userId && t.RevokedAt == null && !t.Used)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.Used = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}
