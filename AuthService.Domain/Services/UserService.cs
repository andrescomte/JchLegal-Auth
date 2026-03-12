using AuthService.Domain.Models;
using AuthService.Domain.Repository;
using AuthService.Domain.SeedWork;
using System.Net;

namespace AuthService.Domain.Services
{
    public interface IUserService
    {
        Task<AuthResult?> LoginAsync(string email, string password, IPAddress? ip, string? userAgent);
        Task<AuthResult?> RefreshTokenAsync(string refreshToken);
        Task<Users> RegisterAsync(string username, string email, string password, int roleId);
        Task<bool> LogoutAsync(string refreshToken);
        Task<bool> ChangePasswordAsync(long userId, string currentPassword, string newPassword);
        Task<(string rawToken, DateTime expiresAt)?> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string resetToken, string newPassword);
    }

    public class UserService : IUserService
    {
        private readonly IUsersRepository _usersRepository;
        private readonly ITokensRepository _tokensRepository;
        private readonly ILoginAttemptsRepository _loginAttemptsRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantContext _tenantContext;

        public UserService(
            IUsersRepository usersRepository,
            ITokensRepository tokensRepository,
            ILoginAttemptsRepository loginAttemptsRepository,
            IJwtTokenService jwtTokenService,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork,
            ITenantContext tenantContext)
        {
            _usersRepository = usersRepository;
            _tokensRepository = tokensRepository;
            _loginAttemptsRepository = loginAttemptsRepository;
            _jwtTokenService = jwtTokenService;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
            _tenantContext = tenantContext;
        }

        public async Task<AuthResult?> LoginAsync(string email, string password, IPAddress? ip, string? userAgent)
        {
            var user = await _usersRepository.GetByEmailAsync(email);
            var succeeded = false;

            try
            {
                if (user?.UserPasswords == null)
                    return null;

                var lastStatus = user.UserStatusHistory.FirstOrDefault();
                if (lastStatus?.Status.Code != "ACTIVE")
                    return null;

                if (!_passwordHasher.Verify(password, user.UserPasswords.PasswordHash))
                {
                    user.UserPasswords.FailedAttempts++;
                    if (user.UserPasswords.FailedAttempts >= 5)
                    {
                        var blockedStatusId = await _usersRepository.GetStatusIdByCodeAsync("BLOCKED");
                        await _usersRepository.AddStatusHistoryAsync(user.Id, blockedStatusId);
                    }
                    await _usersRepository.UpdatePasswordAsync(user.UserPasswords);
                    return null;
                }

                if (user.UserPasswords.FailedAttempts > 0)
                {
                    user.UserPasswords.FailedAttempts = 0;
                    await _usersRepository.UpdatePasswordAsync(user.UserPasswords);
                }

                succeeded = true;

                var roles = user.UserRoles.Select(ur => ur.Role.Code).ToList();
                var jwtToken = _jwtTokenService.GenerateAccessToken(user, roles, _tenantContext.TenantCode, _tenantContext.TenantId);

                var rawRefreshToken = _jwtTokenService.GenerateRefreshToken();
                var refreshHash = _jwtTokenService.HashToken(rawRefreshToken);

                await _tokensRepository.CreateTokenAsync(new Tokens
                {
                    UserId = user.Id,
                    TokenHash = refreshHash,
                    TokenType = "refresh",
                    IssuedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(7)
                });

                return new AuthResult { UserId = user.Id, JwtToken = jwtToken, RefreshToken = rawRefreshToken };
            }
            finally
            {
                await _loginAttemptsRepository.RecordAsync(new LoginAttempts
                {
                    UserId = user?.Id,
                    Email = email,
                    Succeeded = succeeded,
                    Ip = ip,
                    UserAgent = userAgent,
                    AttemptedAt = DateTime.UtcNow
                });
            }
        }

        public async Task<AuthResult?> RefreshTokenAsync(string refreshToken)
        {
            var hash = _jwtTokenService.HashToken(refreshToken);
            var storedToken = await _tokensRepository.GetByHashAsync(hash);

            if (storedToken == null || storedToken.Used || storedToken.RevokedAt != null || storedToken.ExpiresAt <= DateTime.UtcNow)
                return null;

            var user = storedToken.User;

            storedToken.Used = true;
            storedToken.RevokedAt = DateTime.UtcNow;
            await _tokensRepository.UpdateTokenAsync(storedToken);

            var roles = user.UserRoles.Select(ur => ur.Role.Code).ToList();
            var jwtToken = _jwtTokenService.GenerateAccessToken(user, roles, user.Tenant.Code, (int)user.TenantId);

            var rawRefreshToken = _jwtTokenService.GenerateRefreshToken();
            var newHash = _jwtTokenService.HashToken(rawRefreshToken);

            await _tokensRepository.CreateTokenAsync(new Tokens
            {
                UserId = user.Id,
                TokenHash = newHash,
                TokenType = "refresh",
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });

            return new AuthResult { UserId = user.Id, JwtToken = jwtToken, RefreshToken = rawRefreshToken };
        }

        public async Task<Users> RegisterAsync(string username, string email, string password, int roleId)
        {
            if (await _usersRepository.EmailExistsAsync(email))
                throw new InvalidOperationException("Email already registered.");

            PasswordPolicy.Validate(password);
            var passwordHash = _passwordHasher.Hash(password);
            var statusId = await _usersRepository.GetStatusIdByCodeAsync("ACTIVE");

            var user = new Users
            {
                TenantId = _tenantContext.TenantId,
                Username = username,
                Email = email,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            return await _unitOfWork.ExecuteInTransactionAsync(
                () => _usersRepository.CreateUserAsync(user, passwordHash, roleId, statusId));
        }

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            var hash = _jwtTokenService.HashToken(refreshToken);
            var storedToken = await _tokensRepository.GetByHashAsync(hash);

            if (storedToken == null || storedToken.Used || storedToken.RevokedAt != null)
                return false;

            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.Used = true;
            await _tokensRepository.UpdateTokenAsync(storedToken);

            return true;
        }

        public async Task<bool> ChangePasswordAsync(long userId, string currentPassword, string newPassword)
        {
            var user = await _usersRepository.GetByIdAsync(userId);
            if (user?.UserPasswords == null)
                return false;

            if (!_passwordHasher.Verify(currentPassword, user.UserPasswords.PasswordHash))
                return false;

            PasswordPolicy.Validate(newPassword);
            user.UserPasswords.PasswordHash = _passwordHasher.Hash(newPassword);
            user.UserPasswords.FailedAttempts = 0;
            await _usersRepository.UpdatePasswordAsync(user.UserPasswords);

            return true;
        }

        public async Task<(string rawToken, DateTime expiresAt)?> ForgotPasswordAsync(string email)
        {
            var user = await _usersRepository.GetByEmailAsync(email);
            if (user == null)
                return null;

            var rawToken = _jwtTokenService.GenerateRefreshToken();
            var tokenHash = _jwtTokenService.HashToken(rawToken);
            var expiresAt = DateTime.UtcNow.AddHours(1);

            await _tokensRepository.CreateTokenAsync(new Tokens
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                TokenType = "password_reset",
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt
            });

            return (rawToken, expiresAt);
        }

        public async Task<bool> ResetPasswordAsync(string resetToken, string newPassword)
        {
            var hash = _jwtTokenService.HashToken(resetToken);
            var stored = await _tokensRepository.GetByHashAsync(hash);

            if (stored == null
                || stored.TokenType != "password_reset"
                || stored.Used
                || stored.RevokedAt != null
                || stored.ExpiresAt <= DateTime.UtcNow)
                return false;

            var user = stored.User;
            if (user?.UserPasswords == null)
                return false;

            PasswordPolicy.Validate(newPassword);
            user.UserPasswords.PasswordHash = _passwordHasher.Hash(newPassword);
            user.UserPasswords.FailedAttempts = 0;
            await _usersRepository.UpdatePasswordAsync(user.UserPasswords);

            stored.Used = true;
            stored.RevokedAt = DateTime.UtcNow;
            await _tokensRepository.UpdateTokenAsync(stored);

            return true;
        }
    }
}
