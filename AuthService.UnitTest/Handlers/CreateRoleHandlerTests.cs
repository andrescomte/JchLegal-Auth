using Moq;
using AuthService.ApplicationApi.Application.Command.Role;
using AuthService.Domain.Repository;
using RoleEntity = AuthService.Domain.Models.Roles;

namespace AuthService.UnitTest
{
    public class CreateRoleHandlerTests
    {
        private readonly Mock<IRolesRepository> _rolesRepoMock;
        private readonly CreateRoleHandler _handler;

        public CreateRoleHandlerTests()
        {
            _rolesRepoMock = new Mock<IRolesRepository>();
            _handler = new CreateRoleHandler(_rolesRepoMock.Object);
        }

        [Fact]
        public async Task Handle_Returns_Null_When_Role_Code_Already_Exists()
        {
            _rolesRepoMock.Setup(r => r.ExistsAsync("ADMIN")).ReturnsAsync(true);

            var result = await _handler.Handle(
                new CreateRoleRequest { Code = "admin", Name = "Admin" }, default);

            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_Returns_Response_When_Role_Is_Created()
        {
            var created = new RoleEntity { Id = 4, Code = "MANAGER", Name = "Manager" };
            _rolesRepoMock.Setup(r => r.ExistsAsync("MANAGER")).ReturnsAsync(false);
            _rolesRepoMock.Setup(r => r.CreateRoleAsync(It.IsAny<RoleEntity>())).ReturnsAsync(created);

            var result = await _handler.Handle(
                new CreateRoleRequest { Code = "manager", Name = "Manager" }, default);

            Assert.NotNull(result);
            Assert.Equal(4, result.Id);
            Assert.Equal("MANAGER", result.Code);
            Assert.Equal("Manager", result.Name);
        }
    }
}
