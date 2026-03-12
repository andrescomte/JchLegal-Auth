using Moq;
using Microsoft.AspNetCore.Http;
using AuthService.ApplicationApi.Application.Command.Auth;
using AuthService.Domain.Models;
using AuthService.Domain.Repository;
using AuthService.Domain.Services;

namespace AuthService.UnitTest
{
    public class RegisterUserHandlerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IRolesRepository> _rolesRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IAuditLogRepository> _auditLogRepoMock;
        private readonly RegisterUserHandler _handler;

        public RegisterUserHandlerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _rolesRepoMock = new Mock<IRolesRepository>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);
            _auditLogRepoMock = new Mock<IAuditLogRepository>();
            _handler = new RegisterUserHandler(
                _userServiceMock.Object,
                _rolesRepoMock.Object,
                _httpContextAccessorMock.Object,
                _auditLogRepoMock.Object);
        }

        [Fact]
        public async Task Handle_Returns_Null_When_Role_Not_Found()
        {
            _rolesRepoMock.Setup(r => r.GetByCodeAsync(It.IsAny<string>())).ReturnsAsync((Roles?)null);

            var result = await _handler.Handle(
                new RegisterUserRequest { RoleCode = "UNKNOWN" }, default);

            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_Returns_Null_When_Email_Already_Registered()
        {
            _rolesRepoMock.Setup(r => r.GetByCodeAsync(It.IsAny<string>()))
                          .ReturnsAsync(new Roles { Id = 2, Code = "USER" });
            _userServiceMock
                .Setup(s => s.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException("Email already registered."));

            var result = await _handler.Handle(
                new RegisterUserRequest { RoleCode = "USER", Email = "dup@test.com" }, default);

            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_Returns_Response_On_Successful_Registration()
        {
            var created = new Users { Id = 7, Username = "newuser", Email = "new@test.com" };
            _rolesRepoMock.Setup(r => r.GetByCodeAsync(It.IsAny<string>()))
                          .ReturnsAsync(new Roles { Id = 2, Code = "USER" });
            _userServiceMock
                .Setup(s => s.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(created);

            var result = await _handler.Handle(new RegisterUserRequest
            {
                Username = "newuser",
                Email = "new@test.com",
                Password = "pass",
                RoleCode = "USER"
            }, default);

            Assert.NotNull(result);
            Assert.Equal(7, result.UserId);
            Assert.Equal("newuser", result.Username);
            Assert.Equal("new@test.com", result.Email);
        }

        [Fact]
        public async Task Handle_Logs_UserRegistered_On_Success()
        {
            var created = new Users { Id = 7, Username = "newuser", Email = "new@test.com" };
            _rolesRepoMock.Setup(r => r.GetByCodeAsync(It.IsAny<string>()))
                          .ReturnsAsync(new Roles { Id = 2, Code = "USER" });
            _userServiceMock
                .Setup(s => s.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(created);

            await _handler.Handle(new RegisterUserRequest
            {
                Username = "newuser", Email = "new@test.com", Password = "pass", RoleCode = "USER"
            }, default);

            _auditLogRepoMock.Verify(r => r.LogAsync(It.Is<AuditLogs>(l =>
                l.Action == AuditActions.UserRegistered && l.UserId == 7)), Times.Once);
        }
    }
}
