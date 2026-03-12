using System.Diagnostics.CodeAnalysis;
using System.Net;
using AuthService.Domain.Models;
using AuthService.Domain.Services;

namespace AuthService.UnitTest.Mocks
{
    [ExcludeFromCodeCoverage]
    public class UserServiceFake : IUserService
    {
        public const string ValidEmail = "admin@test.com";
        public const string ValidPassword = "AdminPass1!";
        public const string ValidRefreshToken = "valid-refresh-token-for-tests";

        public async Task<AuthResult?> LoginAsync(string email, string password, IPAddress? ip, string? userAgent)
        {
            await Task.CompletedTask;
            if (email == ValidEmail && password == ValidPassword)
                return new AuthResult { JwtToken = "fake-jwt-token", RefreshToken = ValidRefreshToken };
            return null;
        }

        public async Task<AuthResult?> RefreshTokenAsync(string refreshToken)
        {
            await Task.CompletedTask;
            if (refreshToken == ValidRefreshToken)
                return new AuthResult { JwtToken = "fake-new-jwt", RefreshToken = "fake-new-refresh" };
            return null;
        }

        public async Task<Users> RegisterAsync(string username, string email, string password, int roleId)
        {
            await Task.CompletedTask;
            if (email == ValidEmail)
                throw new InvalidOperationException("Email already registered.");
            return new Users { Id = 100, Username = username, Email = email };
        }

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            await Task.CompletedTask;
            return refreshToken == ValidRefreshToken;
        }

        public async Task<bool> ChangePasswordAsync(long userId, string currentPassword, string newPassword)
        {
            await Task.CompletedTask;
            return currentPassword == ValidPassword;
        }

        public async Task<(string rawToken, DateTime expiresAt)?> ForgotPasswordAsync(string email)
        {
            await Task.CompletedTask;
            if (email == ValidEmail)
                return ("fake-reset-token", DateTime.UtcNow.AddHours(1));
            return null;
        }

        public async Task<bool> ResetPasswordAsync(string resetToken, string newPassword)
        {
            await Task.CompletedTask;
            return resetToken == "fake-reset-token";
        }
    }
}
