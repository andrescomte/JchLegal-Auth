using AuthService.Domain.Models;
using AuthService.Domain.Repository;
using AuthService.Infrastructure.Context;

namespace AuthService.Infrastructure.Repository
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly AuthDbContext _context;

        public AuditLogRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(AuditLogs log)
        {
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
