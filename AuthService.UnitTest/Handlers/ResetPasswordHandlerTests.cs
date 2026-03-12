using Moq;
using Microsoft.AspNetCore.Http;
using AuthService.ApplicationApi.Application.Command.Auth;
using AuthService.Domain.Models;
using AuthService.Domain.Repository;
using AuthService.Domain.Services;

namespace AuthService.UnitTest
{
    public class ResetPasswordHandlerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IAuditLogRepository> _auditLogRepoMock;
        private readonly ResetPasswordHandler _handler;

        public ResetPasswordHandlerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);
            _auditLogRepoMock = new Mock<IAuditLogRepository>();
            _handler = new ResetPasswordHandler(
                _userServiceMock.Object,
                _httpContextAccessorMock.Object,
                _auditLogRepoMock.Object);
        }

        [Fact]
        public async Task Handle_Returns_False_When_Token_Is_Invalid()
        {
            _userServiceMock
                .Setup(s => s.ResetPasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            var result = await _handler.Handle(
                new ResetPasswordRequest { ResetToken = "invalid-token", NewPassword = "NewPass1!" }, default);

            Assert.False(result);
        }

        [Fact]
        public async Task Handle_Returns_True_When_Password_Reset_Succeeds()
        {
            _userServiceMock
                .Setup(s => s.ResetPasswordAsync("valid-token", "NewPass1!"))
                .ReturnsAsync(true);

            var result = await _handler.Handle(
                new ResetPasswordRequest { ResetToken = "valid-token", NewPassword = "NewPass1!" }, default);

            Assert.True(result);
        }

        [Fact]
        public async Task Handle_Logs_PasswordResetCompleted_On_Success()
        {
            _userServiceMock
                .Setup(s => s.ResetPasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            await _handler.Handle(
                new ResetPasswordRequest { ResetToken = "valid-token", NewPassword = "NewPass1!" }, default);

            _auditLogRepoMock.Verify(r => r.LogAsync(It.Is<AuditLogs>(l =>
                l.Action == AuditActions.PasswordResetCompleted)), Times.Once);
        }

        [Fact]
        public async Task Handle_Does_Not_Log_When_Reset_Fails()
        {
            _userServiceMock
                .Setup(s => s.ResetPasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            await _handler.Handle(
                new ResetPasswordRequest { ResetToken = "expired-token", NewPassword = "NewPass1!" }, default);

            _auditLogRepoMock.Verify(r => r.LogAsync(It.IsAny<AuditLogs>()), Times.Never);
        }
    }
}
