using System.Diagnostics.CodeAnalysis;
using AuthService.Domain.Models;
using AuthService.Domain.Repository;

namespace AuthService.UnitTest.Mocks
{
    [ExcludeFromCodeCoverage]
    public class AuditLogRepositoryFake : IAuditLogRepository
    {
        public List<AuditLogs> Logs { get; } = new();

        public async Task LogAsync(AuditLogs log)
        {
            await Task.CompletedTask;
            Logs.Add(log);
        }
    }
}
