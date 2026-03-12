using Moq;
using Microsoft.AspNetCore.Http;
using AuthService.ApplicationApi.Application.Command.Auth;
using AuthService.Domain.Models;
using AuthService.Domain.Repository;
using AuthService.Domain.Services;

namespace AuthService.UnitTest
{
    public class ForgotPasswordHandlerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IAuditLogRepository> _auditLogRepoMock;
        private readonly ForgotPasswordHandler _handler;

        public ForgotPasswordHandlerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);
            _auditLogRepoMock = new Mock<IAuditLogRepository>();
            _handler = new ForgotPasswordHandler(
                _userServiceMock.Object,
                _httpContextAccessorMock.Object,
                _auditLogRepoMock.Object);
        }

        [Fact]
        public async Task Handle_Returns_Null_When_Email_Not_Found()
        {
            _userServiceMock
                .Setup(s => s.ForgotPasswordAsync(It.IsAny<string>()))
                .ReturnsAsync((ValueTuple<string, DateTime>?)null);

            var result = await _handler.Handle(
                new ForgotPasswordRequest { Email = "unknown@test.com" }, default);

            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_Returns_Response_With_Token_When_Email_Found()
        {
            var expiresAt = DateTime.UtcNow.AddHours(1);
            _userServiceMock
                .Setup(s => s.ForgotPasswordAsync("user@test.com"))
                .ReturnsAsync(("raw-reset-token", expiresAt));

            var result = await _handler.Handle(
                new ForgotPasswordRequest { Email = "user@test.com" }, default);

            Assert.NotNull(result);
            Assert.Equal("raw-reset-token", result.ResetToken);
            Assert.Equal(expiresAt, result.ExpiresAt);
        }

        [Fact]
        public async Task Handle_Logs_PasswordResetRequested_On_Success()
        {
            _userServiceMock
                .Setup(s => s.ForgotPasswordAsync("user@test.com"))
                .ReturnsAsync(("raw-reset-token", DateTime.UtcNow.AddHours(1)));

            await _handler.Handle(
                new ForgotPasswordRequest { Email = "user@test.com" }, default);

            _auditLogRepoMock.Verify(r => r.LogAsync(It.Is<AuditLogs>(l =>
                l.Action == AuditActions.PasswordResetRequested)), Times.Once);
        }

        [Fact]
        public async Task Handle_Does_Not_Log_When_Email_Not_Found()
        {
            _userServiceMock
                .Setup(s => s.ForgotPasswordAsync(It.IsAny<string>()))
                .ReturnsAsync((ValueTuple<string, DateTime>?)null);

            await _handler.Handle(
                new ForgotPasswordRequest { Email = "unknown@test.com" }, default);

            _auditLogRepoMock.Verify(r => r.LogAsync(It.IsAny<AuditLogs>()), Times.Never);
        }
    }
}
