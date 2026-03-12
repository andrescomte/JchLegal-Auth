using AuthService.Domain.Models;
using AuthService.Domain.Repository;
using AuthService.Infrastructure.Context;

namespace AuthService.Infrastructure.Repository
{
    public class LoginAttemptsRepository : ILoginAttemptsRepository
    {
        private readonly AuthDbContext _context;

        public LoginAttemptsRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task RecordAsync(LoginAttempts attempt)
        {
            _context.LoginAttempts.Add(attempt);
            await _context.SaveChangesAsync();
        }
    }
}
