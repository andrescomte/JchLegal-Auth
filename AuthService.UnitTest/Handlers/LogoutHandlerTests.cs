using Moq;
using Microsoft.AspNetCore.Http;
using AuthService.ApplicationApi.Application.Command.Auth;
using AuthService.Domain.Models;
using AuthService.Domain.Repository;
using AuthService.Domain.Services;

namespace AuthService.UnitTest
{
    public class LogoutHandlerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IAuditLogRepository> _auditLogRepoMock;
        private readonly LogoutHandler _handler;

        public LogoutHandlerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);
            _auditLogRepoMock = new Mock<IAuditLogRepository>();
            _handler = new LogoutHandler(
                _userServiceMock.Object,
                _httpContextAccessorMock.Object,
                _auditLogRepoMock.Object);
        }

        [Fact]
        public async Task Handle_Returns_True_When_Logout_Succeeds()
        {
            _userServiceMock.Setup(s => s.LogoutAsync("valid-token")).ReturnsAsync(true);

            var result = await _handler.Handle(
                new LogoutRequest { RefreshToken = "valid-token" }, default);

            Assert.True(result);
        }

        [Fact]
        public async Task Handle_Returns_False_When_Token_Is_Invalid()
        {
            _userServiceMock.Setup(s => s.LogoutAsync("invalid")).ReturnsAsync(false);

            var result = await _handler.Handle(
                new LogoutRequest { RefreshToken = "invalid" }, default);

            Assert.False(result);
        }

        [Fact]
        public async Task Handle_Logs_Logout_When_Token_Is_Valid()
        {
            _userServiceMock.Setup(s => s.LogoutAsync(It.IsAny<string>())).ReturnsAsync(true);

            await _handler.Handle(new LogoutRequest { RefreshToken = "valid-token" }, default);

            _auditLogRepoMock.Verify(r => r.LogAsync(It.Is<AuditLogs>(l =>
                l.Action == AuditActions.Logout)), Times.Once);
        }

        [Fact]
        public async Task Handle_Does_Not_Log_When_Token_Is_Invalid()
        {
            _userServiceMock.Setup(s => s.LogoutAsync(It.IsAny<string>())).ReturnsAsync(false);

            await _handler.Handle(new LogoutRequest { RefreshToken = "invalid" }, default);

            _auditLogRepoMock.Verify(r => r.LogAsync(It.IsAny<AuditLogs>()), Times.Never);
        }
    }
}
