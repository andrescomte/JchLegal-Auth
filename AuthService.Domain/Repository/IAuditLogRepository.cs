using AuthService.Domain.Models;
using AuthService.Domain.SeedWork;

namespace AuthService.Domain.Repository
{
    public interface IAuditLogRepository : IRepository
    {
        Task LogAsync(AuditLogs log);
    }
}
