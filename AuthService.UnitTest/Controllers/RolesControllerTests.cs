using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using AuthService.ApplicationApi.Application.Query.RolesQuery;
using AuthService.ApplicationApi.Application.Command.Role;
using AuthService.ApplicationApi.Controllers;
using AuthService.Domain.SeedWork;

namespace AuthService.UnitTest
{
    public class RolesControllerTests : TestBase
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly RolesController _controller;
        private readonly Mock<IAppLogger<RolesController>> _loggerMock;

        public RolesControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<IAppLogger<RolesController>>();
            _controller = new RolesController(_mediatorMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetAll_Returns_Ok_With_Roles()
        {
            var expected = new PagedResponse<RolesListResponse>
            {
                Data = new List<RolesListResponse> { new RolesListResponse { Id = 1, Code = "ADMIN", Name = "Administrador" } },
                Page = 1,
                PageSize = 20,
                TotalCount = 1
            };
            _mediatorMock.Setup(m => m.Send(It.IsAny<RolesListRequest>(), default))
                         .ReturnsAsync(expected);

            var result = await _controller.GetAll(new RolesListRequest());

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(expected, ok.Value);
        }

        [Fact]
        public async Task GetById_Returns_NotFound_When_Result_Is_Null()
        {
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetRoleByIdRequest>(), default))
                         .ReturnsAsync((GetRoleByIdResponse?)null);

            var result = await _controller.GetById(99);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetById_Returns_Ok_When_Found()
        {
            var expected = new GetRoleByIdResponse { Id = 1, Code = "ADMIN", Name = "Administrador" };
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetRoleByIdRequest>(), default))
                         .ReturnsAsync(expected);

            var result = await _controller.GetById(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(expected, ok.Value);
        }

        [Fact]
        public async Task CreateRole_Returns_Conflict_When_Result_Is_Null()
        {
            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateRoleRequest>(), default))
                         .ReturnsAsync((CreateRoleResponse?)null);

            var result = await _controller.CreateRole(new CreateRoleRequest { Code = "ADMIN", Name = "Admin" });

            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task CreateRole_Returns_Created_On_Success()
        {
            var expected = new CreateRoleResponse { Id = 4, Code = "MANAGER", Name = "Manager" };
            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateRoleRequest>(), default))
                         .ReturnsAsync(expected);

            var result = await _controller.CreateRole(new CreateRoleRequest { Code = "MANAGER", Name = "Manager" });

            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Same(expected, created.Value);
        }

        [Fact]
        public async Task UpdateRole_Returns_NotFound_When_Result_Is_Null()
        {
            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateRoleRequest>(), default))
                         .ReturnsAsync((UpdateRoleResponse?)null);

            var result = await _controller.UpdateRole(99, new UpdateRoleRequest { Name = "Nuevo Nombre" });

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateRole_Returns_Ok_With_Updated_Role()
        {
            var expected = new UpdateRoleResponse { Id = 1, Code = "ADMIN", Name = "Super Administrador" };
            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateRoleRequest>(), default))
                         .ReturnsAsync(expected);

            var result = await _controller.UpdateRole(1, new UpdateRoleRequest { Name = "Super Administrador" });

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(expected, ok.Value);
        }
    }
}
