using Moq;
using System.Net;
using AuthService.Domain.Models;
using AuthService.Domain.Repository;
using AuthService.Domain.SeedWork;
using AuthService.Domain.Services;

namespace AuthService.UnitTest
{
    public class UserServiceTests
    {
        private readonly Mock<IUsersRepository> _usersRepoMock;
        private readonly Mock<ITokensRepository> _tokensRepoMock;
        private readonly Mock<ILoginAttemptsRepository> _loginAttemptsRepoMock;
        private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _usersRepoMock = new Mock<IUsersRepository>();
            _tokensRepoMock = new Mock<ITokensRepository>();
            _loginAttemptsRepoMock = new Mock<ILoginAttemptsRepository>();
            _jwtTokenServiceMock = new Mock<IJwtTokenService>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _tenantContextMock = new Mock<ITenantContext>();
            _tenantContextMock.Setup(t => t.TenantId).Returns(1);
            _tenantContextMock.Setup(t => t.TenantCode).Returns("test-tenant");
            _unitOfWorkMock
                .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task<Users>>>(), It.IsAny<CancellationToken>()))
                .Returns<Func<Task<Users>>, CancellationToken>((fn, _) => fn());

            _service = new UserService(
                _usersRepoMock.Object,
                _tokensRepoMock.Object,
                _loginAttemptsRepoMock.Object,
                _jwtTokenServiceMock.Object,
                _passwordHasherMock.Object,
                _unitOfWorkMock.Object,
                _tenantContextMock.Object);
        }

        private static Users CreateActiveUser(int failedAttempts = 0) => new Users
        {
            Id = 1,
            TenantId = 1,
            Username = "testuser",
            Email = "test@example.com",
            IsActive = true,
            Tenant = new Tenants { Id = 1, Code = "test-tenant", Name = "Test Tenant" },
            UserPasswords = new UserPasswords
            {
                Id = 1,
                UserId = 1,
                PasswordHash = "hashed_password",
                FailedAttempts = failedAttempts
            },
            UserStatusHistory = new List<UserStatusHistory>
            {
                new UserStatusHistory { Status = new UserStatus { Code = "ACTIVE", Name = "Activo" } }
            },
            UserRoles = new List<UserRoles>
            {
                new UserRoles { Role = new Roles { Id = 2, Code = "USER", Name = "Usuario" } }
            }
        };

        // ─── LoginAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task LoginAsync_Returns_Null_When_User_Not_Found()
        {
            _usersRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((Users?)null);

            var result = await _service.LoginAsync("ghost@test.com", "pass", null, null);

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_Returns_Null_When_UserPasswords_Is_Null()
        {
            var user = CreateActiveUser();
            user.UserPasswords = null;
            _usersRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

            var result = await _service.LoginAsync("test@example.com", "pass", null, null);

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_Returns_Null_When_Status_Not_Active()
        {
            var user = CreateActiveUser();
            user.UserStatusHistory = new List<UserStatusHistory>
            {
                new UserStatusHistory { Status = new UserStatus { Code = "BLOCKED", Name = "Bloqueado" } }
            };
            _usersRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

            var result = await _service.LoginAsync("test@example.com", "pass", null, null);

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_Returns_Null_And_Increments_FailedAttempts_On_Wrong_Password()
        {
            var user = CreateActiveUser(failedAttempts: 0);
            _usersRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _passwordHasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
            _usersRepoMock.Setup(r => r.UpdatePasswordAsync(It.IsAny<UserPasswords>())).Returns(Task.CompletedTask);

            var result = await _service.LoginAsync("test@example.com", "wrong", null, null);

            Assert.Null(result);
            Assert.Equal(1, user.UserPasswords!.FailedAttempts);
            _usersRepoMock.Verify(r => r.UpdatePasswordAsync(user.UserPasswords), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_Blocks_User_After_Five_Failed_Attempts()
        {
            var user = CreateActiveUser(failedAttempts: 4);
            _usersRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _passwordHasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
            _usersRepoMock.Setup(r => r.GetStatusIdByCodeAsync("BLOCKED")).ReturnsAsync(3);
            _usersRepoMock.Setup(r => r.AddStatusHistoryAsync(It.IsAny<long>(), It.IsAny<int>())).Returns(Task.CompletedTask);
            _usersRepoMock.Setup(r => r.UpdatePasswordAsync(It.IsAny<UserPasswords>())).Returns(Task.CompletedTask);

            var result = await _service.LoginAsync("test@example.com", "wrong", null, null);

            Assert.Null(result);
            Assert.Equal(5, user.UserPasswords!.FailedAttempts);
            _usersRepoMock.Verify(r => r.GetStatusIdByCodeAsync("BLOCKED"), Times.Once);
            _usersRepoMock.Verify(r => r.AddStatusHistoryAsync(user.Id, 3), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_Returns_AuthResult_On_Successful_Login()
        {
            var user = CreateActiveUser();
            _usersRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _passwordHasherMock.Setup(h => h.Verify("correct", "hashed_password")).Returns(true);
            _jwtTokenServiceMock.Setup(j => j.GenerateAccessToken(user, It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<int>())).Returns("jwt-token");
            _jwtTokenServiceMock.Setup(j => j.GenerateRefreshToken()).Returns("raw-refresh");
            _jwtTokenServiceMock.Setup(j => j.HashToken("raw-refresh")).Returns("hashed-refresh");
            _tokensRepoMock.Setup(r => r.CreateTokenAsync(It.IsAny<Tokens>())).ReturnsAsync(new Tokens());

            var result = await _service.LoginAsync("test@example.com", "correct", null, null);

            Assert.NotNull(result);
            Assert.Equal("jwt-token", result.JwtToken);
            Assert.Equal("raw-refresh", result.RefreshToken);
        }

        [Fact]
        public async Task LoginAsync_Resets_FailedAttempts_On_Successful_Login()
        {
            var user = CreateActiveUser(failedAttempts: 2);
            _usersRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _passwordHasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _jwtTokenServiceMock.Setup(j => j.GenerateAccessToken(user, It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<int>())).Returns("jwt");
            _jwtTokenServiceMock.Setup(j => j.GenerateRefreshToken()).Returns("raw");
            _jwtTokenServiceMock.Setup(j => j.HashToken("raw")).Returns("hash");
            _tokensRepoMock.Setup(r => r.CreateTokenAsync(It.IsAny<Tokens>())).ReturnsAsync(new Tokens());
            _usersRepoMock.Setup(r => r.UpdatePasswordAsync(It.IsAny<UserPasswords>())).Returns(Task.CompletedTask);

            await _service.LoginAsync("test@example.com", "correct", null, null);

            Assert.Equal(0, user.UserPasswords!.FailedAttempts);
            _usersRepoMock.Verify(r => r.UpdatePasswordAsync(user.UserPasswords), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_Always_Records_LoginAttempt_Even_When_User_Not_Found()
        {
            _usersRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((Users?)null);

            await _service.LoginAsync("ghost@test.com", "pass", IPAddress.Loopback, "TestAgent/1.0");

            _loginAttemptsRepoMock.Verify(r => r.RecordAsync(It.Is<LoginAttempts>(a =>
                a.Email == "ghost@test.com" && !a.Succeeded)), Times.Once);
        }

        // ─── RefreshTokenAsync ────────────────────────────────────────────────

        [Fact]
        public async Task RefreshTokenAsync_Returns_Null_When_Token_Not_Found()
        {
            _jwtTokenServiceMock.Setup(j => j.HashToken(It.IsAny<string>())).Returns("hash");
            _tokensRepoMock.Setup(r => r.GetByHashAsync("hash")).ReturnsAsync((Tokens?)null);

            var result = await _service.RefreshTokenAsync("unknown-token");

            Assert.Null(result);
        }

        [Fact]
        public async Task RefreshTokenAsync_Returns_Null_When_Token_Already_Used()
        {
            var token = new Tokens { Used = true, RevokedAt = null, ExpiresAt = DateTime.UtcNow.AddDays(1), User = CreateActiveUser() };
            _jwtTokenServiceMock.Setup(j => j.HashToken(It.IsAny<string>())).Returns("hash");
            _tokensRepoMock.Setup(r => r.GetByHashAsync("hash")).ReturnsAsync(token);

            var result = await _service.RefreshTokenAsync("used-token");

            Assert.Null(result);
        }

        [Fact]
        public async Task RefreshTokenAsync_Returns_Null_When_Token_Expired()
        {
            var token = new Tokens { Used = false, RevokedAt = null, ExpiresAt = DateTime.UtcNow.AddDays(-1), User = CreateActiveUser() };
            _jwtTokenServiceMock.Setup(j => j.HashToken(It.IsAny<string>())).Returns("hash");
            _tokensRepoMock.Setup(r => r.GetByHashAsync("hash")).ReturnsAsync(token);

            var result = await _service.RefreshTokenAsync("expired-token");

            Assert.Null(result);
        }

        [Fact]
        public async Task RefreshTokenAsync_Returns_Null_When_Token_Revoked()
        {
            var token = new Tokens { Used = false, RevokedAt = DateTime.UtcNow.AddHours(-1), ExpiresAt = DateTime.UtcNow.AddDays(7), User = CreateActiveUser() };
            _jwtTokenServiceMock.Setup(j => j.HashToken(It.IsAny<string>())).Returns("hash");
            _tokensRepoMock.Setup(r => r.GetByHashAsync("hash")).ReturnsAsync(token);

            var result = await _service.RefreshTokenAsync("revoked-token");

            Assert.Null(result);
        }

        [Fact]
        public async Task RefreshTokenAsync_Returns_AuthResult_And_Rotates_Token()
        {
            var user = CreateActiveUser();
            var token = new Tokens { Used = false, RevokedAt = null, ExpiresAt = DateTime.UtcNow.AddDays(7), User = user };
            _jwtTokenServiceMock.Setup(j => j.HashToken("old-token")).Returns("old-hash");
            _jwtTokenServiceMock.Setup(j => j.HashToken("new-raw")).Returns("new-hash");
            _jwtTokenServiceMock.Setup(j => j.GenerateAccessToken(user, It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<int>())).Returns("new-jwt");
            _jwtTokenServiceMock.Setup(j => j.GenerateRefreshToken()).Returns("new-raw");
            _tokensRepoMock.Setup(r => r.GetByHashAsync("old-hash")).ReturnsAsync(token);
            _tokensRepoMock.Setup(r => r.UpdateTokenAsync(It.IsAny<Tokens>())).Returns(Task.CompletedTask);
            _tokensRepoMock.Setup(r => r.CreateTokenAsync(It.IsAny<Tokens>())).ReturnsAsync(new Tokens());

            var result = await _service.RefreshTokenAsync("old-token");

            Assert.NotNull(result);
            Assert.Equal("new-jwt", result.JwtToken);
            Assert.Equal("new-raw", result.RefreshToken);
            Assert.True(token.Used);
            Assert.NotNull(token.RevokedAt);
        }

        // ─── RegisterAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task RegisterAsync_Throws_When_Email_Already_Exists()
        {
            _usersRepoMock.Setup(r => r.EmailExistsAsync("existing@test.com")).ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.RegisterAsync("user", "existing@test.com", "pass", 2));
        }

        [Fact]
        public async Task RegisterAsync_Creates_User_Successfully()
        {
            var created = new Users { Id = 5, Username = "newuser", Email = "new@test.com" };
            _usersRepoMock.Setup(r => r.EmailExistsAsync("new@test.com")).ReturnsAsync(false);
            _passwordHasherMock.Setup(h => h.Hash("Password1")).Returns("hashed");
            _usersRepoMock.Setup(r => r.GetStatusIdByCodeAsync("ACTIVE")).ReturnsAsync(2);
            _usersRepoMock.Setup(r => r.CreateUserAsync(It.IsAny<Users>(), "hashed", 2, 2)).ReturnsAsync(created);

            var result = await _service.RegisterAsync("newuser", "new@test.com", "Password1", 2);

            Assert.NotNull(result);
            Assert.Equal(5, result.Id);
            Assert.Equal("newuser", result.Username);
        }

        // ─── ChangePasswordAsync ──────────────────────────────────────────────

        [Fact]
        public async Task ChangePasswordAsync_Returns_False_When_User_Not_Found()
        {
            _usersRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Users?)null);

            var result = await _service.ChangePasswordAsync(99, "any", "new");

            Assert.False(result);
        }

        [Fact]
        public async Task ChangePasswordAsync_Returns_False_When_UserPasswords_Is_Null()
        {
            var user = CreateActiveUser();
            user.UserPasswords = null;
            _usersRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

            var result = await _service.ChangePasswordAsync(1, "any", "new");

            Assert.False(result);
        }

        [Fact]
        public async Task ChangePasswordAsync_Returns_False_When_Current_Password_Is_Wrong()
        {
            var user = CreateActiveUser();
            _usersRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
            _passwordHasherMock.Setup(h => h.Verify("wrong", "hashed_password")).Returns(false);

            var result = await _service.ChangePasswordAsync(1, "wrong", "new");

            Assert.False(result);
            _usersRepoMock.Verify(r => r.UpdatePasswordAsync(It.IsAny<UserPasswords>()), Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_Returns_True_And_Updates_Password_On_Success()
        {
            var user = CreateActiveUser();
            _usersRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
            _passwordHasherMock.Setup(h => h.Verify("current", "hashed_password")).Returns(true);
            _passwordHasherMock.Setup(h => h.Hash("NewPass1")).Returns("new-hashed");
            _usersRepoMock.Setup(r => r.UpdatePasswordAsync(It.IsAny<UserPasswords>())).Returns(Task.CompletedTask);

            var result = await _service.ChangePasswordAsync(1, "current", "NewPass1");

            Assert.True(result);
            Assert.Equal("new-hashed", user.UserPasswords!.PasswordHash);
            Assert.Equal(0, user.UserPasswords.FailedAttempts);
            _usersRepoMock.Verify(r => r.UpdatePasswordAsync(user.UserPasswords), Times.Once);
        }

        // ─── LogoutAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task LogoutAsync_Returns_False_When_Token_Not_Found()
        {
            _jwtTokenServiceMock.Setup(j => j.HashToken(It.IsAny<string>())).Returns("hash");
            _tokensRepoMock.Setup(r => r.GetByHashAsync("hash")).ReturnsAsync((Tokens?)null);

            var result = await _service.LogoutAsync("unknown");

            Assert.False(result);
        }

        [Fact]
        public async Task LogoutAsync_Returns_False_When_Token_Already_Used()
        {
            var token = new Tokens { Used = true, RevokedAt = null };
            _jwtTokenServiceMock.Setup(j => j.HashToken(It.IsAny<string>())).Returns("hash");
            _tokensRepoMock.Setup(r => r.GetByHashAsync("hash")).ReturnsAsync(token);

            var result = await _service.LogoutAsync("used-token");

            Assert.False(result);
        }

        // ─── ForgotPasswordAsync ──────────────────────────────────────────────

        [Fact]
        public async Task ForgotPasswordAsync_Returns_Null_When_User_Not_Found()
        {
            _usersRepoMock.Setup(r => r.GetByEmailAsync("ghost@test.com")).ReturnsAsync((Users?)null);

            var result = await _service.ForgotPasswordAsync("ghost@test.com");

            Assert.Null(result);
            _tokensRepoMock.Verify(r => r.CreateTokenAsync(It.IsAny<Tokens>()), Times.Never);
        }

        [Fact]
        public async Task ForgotPasswordAsync_Returns_Token_When_User_Found()
        {
            var user = CreateActiveUser();
            _usersRepoMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
            _jwtTokenServiceMock.Setup(j => j.GenerateRefreshToken()).Returns("raw-reset-token");
            _jwtTokenServiceMock.Setup(j => j.HashToken("raw-reset-token")).Returns("hashed-reset-token");
            _tokensRepoMock.Setup(r => r.CreateTokenAsync(It.IsAny<Tokens>())).ReturnsAsync(new Tokens());

            var result = await _service.ForgotPasswordAsync(user.Email);

            Assert.NotNull(result);
            Assert.Equal("raw-reset-token", result.Value.rawToken);
            Assert.True(result.Value.expiresAt > DateTime.UtcNow);
        }

        [Fact]
        public async Task ForgotPasswordAsync_Creates_Token_With_Correct_Type_And_Expiry()
        {
            var user = CreateActiveUser();
            _usersRepoMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
            _jwtTokenServiceMock.Setup(j => j.GenerateRefreshToken()).Returns("raw-reset-token");
            _jwtTokenServiceMock.Setup(j => j.HashToken("raw-reset-token")).Returns("hashed-reset-token");

            Tokens? savedToken = null;
            _tokensRepoMock
                .Setup(r => r.CreateTokenAsync(It.IsAny<Tokens>()))
                .Callback<Tokens>(t => savedToken = t)
                .ReturnsAsync(new Tokens());

            await _service.ForgotPasswordAsync(user.Email);

            Assert.NotNull(savedToken);
            Assert.Equal("password_reset", savedToken!.TokenType);
            Assert.Equal("hashed-reset-token", savedToken.TokenHash);
            Assert.Equal(user.Id, savedToken.UserId);
            Assert.True(savedToken.ExpiresAt > DateTime.UtcNow.AddMinutes(59));
        }

        // ─── ResetPasswordAsync ───────────────────────────────────────────────

        [Fact]
        public async Task ResetPasswordAsync_Returns_False_When_Token_Not_Found()
        {
            _jwtTokenServiceMock.Setup(j => j.HashToken(It.IsAny<string>())).Returns("hash");
            _tokensRepoMock.Setup(r => r.GetByHashAsync("hash")).ReturnsAsync((Tokens?)null);

            var result = await _service.ResetPasswordAsync("invalid-token", "new-pass");

            Assert.False(result);
        }

        [Fact]
        public async Task ResetPasswordAsync_Returns_False_When_Token_Type_Is_Wrong()
        {
            var token = new Tokens { TokenType = "refresh", Used = false, RevokedAt = null, ExpiresAt = DateTime.UtcNow.AddHours(1), User = CreateActiveUser() };
            _jwtTokenServiceMock.Setup(j => j.HashToken(It.IsAny<string>())).Returns("hash");
            _tokensRepoMock.Setup(r => r.GetByHashAsync("hash")).ReturnsAsync(token);

            var result = await _service.ResetPasswordAsync("token", "new-pass");

            Assert.False(result);
        }

        [Fact]
        public async Task ResetPasswordAsync_Returns_False_When_Token_Already_Used()
        {
            var token = new Tokens { TokenType = "password_reset", Used = true, RevokedAt = null, ExpiresAt = DateTime.UtcNow.AddHours(1), User = CreateActiveUser() };
            _jwtTokenServiceMock.Setup(j => j.HashToken(It.IsAny<string>())).Returns("hash");
            _tokensRepoMock.Setup(r => r.GetByHashAsync("hash")).ReturnsAsync(token);

            var result = await _service.ResetPasswordAsync("token", "new-pass");

            Assert.False(result);
        }

        [Fact]
        public async Task ResetPasswordAsync_Returns_False_When_Token_Expired()
        {
            var token = new Tokens { TokenType = "password_reset", Used = false, RevokedAt = null, ExpiresAt = DateTime.UtcNow.AddHours(-1), User = CreateActiveUser() };
            _jwtTokenServiceMock.Setup(j => j.HashToken(It.IsAny<string>())).Returns("hash");
            _tokensRepoMock.Setup(r => r.GetByHashAsync("hash")).ReturnsAsync(token);

            var result = await _service.ResetPasswordAsync("token", "new-pass");

            Assert.False(result);
        }

        [Fact]
        public async Task ResetPasswordAsync_Returns_True_And_Updates_Password_On_Success()
        {
            var user = CreateActiveUser();
            var token = new Tokens { TokenType = "password_reset", Used = false, RevokedAt = null, ExpiresAt = DateTime.UtcNow.AddHours(1), User = user };
            _jwtTokenServiceMock.Setup(j => j.HashToken(It.IsAny<string>())).Returns("hash");
            _tokensRepoMock.Setup(r => r.GetByHashAsync("hash")).ReturnsAsync(token);
            _passwordHasherMock.Setup(h => h.Hash("NewPass1")).Returns("new-hashed");
            _usersRepoMock.Setup(r => r.UpdatePasswordAsync(It.IsAny<UserPasswords>())).Returns(Task.CompletedTask);
            _tokensRepoMock.Setup(r => r.UpdateTokenAsync(It.IsAny<Tokens>())).Returns(Task.CompletedTask);

            var result = await _service.ResetPasswordAsync("token", "NewPass1");

            Assert.True(result);
            Assert.Equal("new-hashed", user.UserPasswords!.PasswordHash);
            Assert.Equal(0, user.UserPasswords.FailedAttempts);
        }

        [Fact]
        public async Task ResetPasswordAsync_Marks_Token_As_Used_On_Success()
        {
            var user = CreateActiveUser();
            var token = new Tokens { TokenType = "password_reset", Used = false, RevokedAt = null, ExpiresAt = DateTime.UtcNow.AddHours(1), User = user };
            _jwtTokenServiceMock.Setup(j => j.HashToken(It.IsAny<string>())).Returns("hash");
            _tokensRepoMock.Setup(r => r.GetByHashAsync("hash")).ReturnsAsync(token);
            _passwordHasherMock.Setup(h => h.Hash(It.IsAny<string>())).Returns("new-hashed");
            _usersRepoMock.Setup(r => r.UpdatePasswordAsync(It.IsAny<UserPasswords>())).Returns(Task.CompletedTask);
            _tokensRepoMock.Setup(r => r.UpdateTokenAsync(It.IsAny<Tokens>())).Returns(Task.CompletedTask);

            await _service.ResetPasswordAsync("token", "NewPass1");

            Assert.True(token.Used);
            Assert.NotNull(token.RevokedAt);
            _tokensRepoMock.Verify(r => r.UpdateTokenAsync(token), Times.Once);
        }

        [Fact]
        public async Task LogoutAsync_Returns_True_And_Revokes_Token()
        {
            var token = new Tokens { Used = false, RevokedAt = null };
            _jwtTokenServiceMock.Setup(j => j.HashToken(It.IsAny<string>())).Returns("hash");
            _tokensRepoMock.Setup(r => r.GetByHashAsync("hash")).ReturnsAsync(token);
            _tokensRepoMock.Setup(r => r.UpdateTokenAsync(It.IsAny<Tokens>())).Returns(Task.CompletedTask);

            var result = await _service.LogoutAsync("valid-token");

            Assert.True(result);
            Assert.True(token.Used);
            Assert.NotNull(token.RevokedAt);
        }
    }
}
