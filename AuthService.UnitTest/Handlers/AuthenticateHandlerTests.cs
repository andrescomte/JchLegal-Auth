using Moq;
using Microsoft.AspNetCore.Http;
using AuthService.ApplicationApi.Application.Command.Auth;
using AuthService.Domain.Models;
using AuthService.Domain.Repository;
using AuthService.Domain.Services;

namespace AuthService.UnitTest
{
    public class AuthenticateHandlerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IAuditLogRepository> _auditLogRepoMock;
        private readonly AuthenticateHandler _handler;

        public AuthenticateHandlerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);
            _auditLogRepoMock = new Mock<IAuditLogRepository>();
            _handler = new AuthenticateHandler(
                _userServiceMock.Object,
                _httpContextAccessorMock.Object,
                _auditLogRepoMock.Object);
        }

        [Fact]
        public async Task Handle_Returns_Null_When_Login_Fails()
        {
            _userServiceMock
                .Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), null, null))
                .ReturnsAsync((AuthResult?)null);

            var result = await _handler.Handle(
                new AuthenticateRequest { Email = "a@b.com", Password = "wrong" }, default);

            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_Returns_Response_When_Login_Succeeds()
        {
            _userServiceMock
                .Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), null, null))
                .ReturnsAsync(new AuthResult { UserId = 1, JwtToken = "jwt", RefreshToken = "refresh" });

            var result = await _handler.Handle(
                new AuthenticateRequest { Email = "a@b.com", Password = "correct" }, default);

            Assert.NotNull(result);
            Assert.Equal("jwt", result.Token);
            Assert.Equal("refresh", result.RefreshToken);
        }

        [Fact]
        public async Task Handle_Logs_LoginSuccess_When_Login_Succeeds()
        {
            _userServiceMock
                .Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), null, null))
                .ReturnsAsync(new AuthResult { UserId = 1, JwtToken = "jwt", RefreshToken = "refresh" });

            await _handler.Handle(
                new AuthenticateRequest { Email = "a@b.com", Password = "correct" }, default);

            _auditLogRepoMock.Verify(r => r.LogAsync(It.Is<AuditLogs>(l =>
                l.Action == AuditActions.LoginSuccess)), Times.Once);
        }

        [Fact]
        public async Task Handle_Logs_LoginFailed_When_Login_Fails()
        {
            _userServiceMock
                .Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), null, null))
                .ReturnsAsync((AuthResult?)null);

            await _handler.Handle(
                new AuthenticateRequest { Email = "a@b.com", Password = "wrong" }, default);

            _auditLogRepoMock.Verify(r => r.LogAsync(It.Is<AuditLogs>(l =>
                l.Action == AuditActions.LoginFailed)), Times.Once);
        }
    }
}
