using AuthService.Domain.Models;
using AuthService.Domain.SeedWork;

namespace AuthService.Domain.Repository
{
    public interface ILoginAttemptsRepository : IRepository
    {
        Task RecordAsync(LoginAttempts attempt);
    }
}
