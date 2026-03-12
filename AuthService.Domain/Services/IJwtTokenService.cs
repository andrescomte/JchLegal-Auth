using AuthService.Domain.Models;

namespace AuthService.Domain.Services
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(Users user, IEnumerable<string> roles, string tenantCode, int tenantId);
        string GenerateRefreshToken();
        string HashToken(string token);
    }
}
