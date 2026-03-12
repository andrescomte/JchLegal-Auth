using Moq;
using Microsoft.AspNetCore.Http;
using AuthService.ApplicationApi.Application.Command.Role;
using AuthService.Domain.Models;
using AuthService.Domain.Repository;
using RoleEntity = AuthService.Domain.Models.Roles;

namespace AuthService.UnitTest
{
    public class UpdateRoleHandlerTests
    {
        private readonly Mock<IRolesRepository> _rolesRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IAuditLogRepository> _auditLogRepoMock;
        private readonly UpdateRoleHandler _handler;

        public UpdateRoleHandlerTests()
        {
            _rolesRepoMock = new Mock<IRolesRepository>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);
            _auditLogRepoMock = new Mock<IAuditLogRepository>();
            _handler = new UpdateRoleHandler(
                _rolesRepoMock.Object,
                _httpContextAccessorMock.Object,
                _auditLogRepoMock.Object);
        }

        [Fact]
        public async Task Handle_Returns_Null_When_Role_Not_Found()
        {
            _rolesRepoMock.Setup(r => r.UpdateRoleAsync(99, It.IsAny<string>())).ReturnsAsync((RoleEntity?)null);

            var result = await _handler.Handle(
                new UpdateRoleRequest { Id = 99, Name = "Nuevo Nombre" }, default);

            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_Returns_Updated_Response_When_Role_Exists()
        {
            var updated = new RoleEntity { Id = 1, Code = "ADMIN", Name = "Super Administrador" };
            _rolesRepoMock.Setup(r => r.UpdateRoleAsync(1, "Super Administrador")).ReturnsAsync(updated);

            var result = await _handler.Handle(
                new UpdateRoleRequest { Id = 1, Name = "Super Administrador" }, default);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("ADMIN", result.Code);
            Assert.Equal("Super Administrador", result.Name);
        }

        [Fact]
        public async Task Handle_Logs_RoleUpdated_On_Success()
        {
            var updated = new RoleEntity { Id = 1, Code = "ADMIN", Name = "Super Administrador" };
            _rolesRepoMock.Setup(r => r.UpdateRoleAsync(1, It.IsAny<string>())).ReturnsAsync(updated);

            await _handler.Handle(new UpdateRoleRequest { Id = 1, Name = "Super Administrador" }, default);

            _auditLogRepoMock.Verify(r => r.LogAsync(It.Is<AuditLogs>(l =>
                l.Action == AuditActions.RoleUpdated)), Times.Once);
        }

        [Fact]
        public async Task Handle_Does_Not_Log_When_Role_Not_Found()
        {
            _rolesRepoMock.Setup(r => r.UpdateRoleAsync(99, It.IsAny<string>())).ReturnsAsync((RoleEntity?)null);

            await _handler.Handle(new UpdateRoleRequest { Id = 99, Name = "No existe" }, default);

            _auditLogRepoMock.Verify(r => r.LogAsync(It.IsAny<AuditLogs>()), Times.Never);
        }
    }
}
