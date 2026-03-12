using Moq;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using AuthService.ApplicationApi.Application.Command.Auth;
using AuthService.Domain.Models;
using AuthService.Domain.Repository;
using AuthService.Domain.Services;

namespace AuthService.UnitTest
{
    public class ChangePasswordHandlerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IAuditLogRepository> _auditLogRepoMock;
        private readonly ChangePasswordHandler _handler;

        public ChangePasswordHandlerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _auditLogRepoMock = new Mock<IAuditLogRepository>();
            _handler = new ChangePasswordHandler(
                _userServiceMock.Object,
                _httpContextAccessorMock.Object,
                _auditLogRepoMock.Object);
        }

        private void SetupHttpContext(string? userId)
        {
            var claims = userId != null
                ? new[] { new Claim(ClaimTypes.NameIdentifier, userId) }
                : Array.Empty<Claim>();

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };

            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);
        }

        [Fact]
        public async Task Handle_Returns_False_When_UserId_Claim_Missing()
        {
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);

            var result = await _handler.Handle(
                new ChangePasswordRequest { CurrentPassword = "old", NewPassword = "new" }, default);

            Assert.False(result);
            _userServiceMock.Verify(s => s.ChangePasswordAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Returns_False_When_Current_Password_Is_Wrong()
        {
            SetupHttpContext("1");
            _userServiceMock.Setup(s => s.ChangePasswordAsync(1, "wrong", "new")).ReturnsAsync(false);

            var result = await _handler.Handle(
                new ChangePasswordRequest { CurrentPassword = "wrong", NewPassword = "new" }, default);

            Assert.False(result);
        }

        [Fact]
        public async Task Handle_Returns_True_When_Password_Changed_Successfully()
        {
            SetupHttpContext("1");
            _userServiceMock.Setup(s => s.ChangePasswordAsync(1, "current", "new")).ReturnsAsync(true);

            var result = await _handler.Handle(
                new ChangePasswordRequest { CurrentPassword = "current", NewPassword = "new" }, default);

            Assert.True(result);
        }

        [Fact]
        public async Task Handle_Logs_PasswordChanged_On_Success()
        {
            SetupHttpContext("1");
            _userServiceMock.Setup(s => s.ChangePasswordAsync(1, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            await _handler.Handle(
                new ChangePasswordRequest { CurrentPassword = "current", NewPassword = "new" }, default);

            _auditLogRepoMock.Verify(r => r.LogAsync(It.Is<AuditLogs>(l =>
                l.Action == AuditActions.PasswordChanged && l.UserId == 1)), Times.Once);
        }

        [Fact]
        public async Task Handle_Does_Not_Log_When_Password_Change_Fails()
        {
            SetupHttpContext("1");
            _userServiceMock.Setup(s => s.ChangePasswordAsync(1, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            await _handler.Handle(
                new ChangePasswordRequest { CurrentPassword = "wrong", NewPassword = "new" }, default);

            _auditLogRepoMock.Verify(r => r.LogAsync(It.IsAny<AuditLogs>()), Times.Never);
        }
    }
}
