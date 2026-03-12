using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using AuthService.ApplicationApi.Application.Command.Auth;
using AuthService.ApplicationApi.Controllers;
using AuthService.Domain.SeedWork;

namespace AuthService.UnitTest
{
    public class UserControllerTests : TestBase
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly UserController _controller;
        private readonly Mock<IAppLogger<UserController>> _loggerMock;

        public UserControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<IAppLogger<UserController>>();
            _controller = new UserController(_mediatorMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Authenticate_Returns_Unauthorized_When_Login_Fails()
        {
            _mediatorMock.Setup(m => m.Send(It.IsAny<AuthenticateRequest>(), default))
                         .ReturnsAsync((AuthenticateResponse?)null);

            var result = await _controller.Authenticate(new AuthenticateRequest());

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Authenticate_Returns_Ok_With_Token_On_Success()
        {
            var expected = new AuthenticateResponse { Token = "jwt", RefreshToken = "refresh" };
            _mediatorMock.Setup(m => m.Send(It.IsAny<AuthenticateRequest>(), default))
                         .ReturnsAsync(expected);

            var result = await _controller.Authenticate(new AuthenticateRequest());

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(expected, ok.Value);
        }

        [Fact]
        public async Task Register_Returns_Conflict_When_Handler_Returns_Null()
        {
            _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterUserRequest>(), default))
                         .ReturnsAsync((RegisterUserResponse?)null);

            var result = await _controller.Register(new RegisterUserRequest());

            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task Register_Returns_Created_On_Success()
        {
            var expected = new RegisterUserResponse { UserId = 1, Username = "john", Email = "john@test.com" };
            _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterUserRequest>(), default))
                         .ReturnsAsync(expected);

            var result = await _controller.Register(new RegisterUserRequest());

            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Same(expected, created.Value);
        }

        [Fact]
        public async Task Refresh_Returns_Unauthorized_When_Token_Invalid()
        {
            _mediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenRequest>(), default))
                         .ReturnsAsync((AuthenticateResponse?)null);

            var result = await _controller.Refresh(new RefreshTokenRequest());

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Refresh_Returns_Ok_On_Valid_Token()
        {
            var expected = new AuthenticateResponse { Token = "new-jwt", RefreshToken = "new-refresh" };
            _mediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenRequest>(), default))
                         .ReturnsAsync(expected);

            var result = await _controller.Refresh(new RefreshTokenRequest());

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(expected, ok.Value);
        }

        [Fact]
        public async Task Logout_Returns_BadRequest_When_Token_Invalid()
        {
            _mediatorMock.Setup(m => m.Send(It.IsAny<LogoutRequest>(), default))
                         .ReturnsAsync(false);

            var result = await _controller.Logout(new LogoutRequest());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Logout_Returns_Ok_When_Token_Valid()
        {
            _mediatorMock.Setup(m => m.Send(It.IsAny<LogoutRequest>(), default))
                         .ReturnsAsync(true);

            var result = await _controller.Logout(new LogoutRequest());

            Assert.IsType<OkResult>(result);
        }
    }
}
