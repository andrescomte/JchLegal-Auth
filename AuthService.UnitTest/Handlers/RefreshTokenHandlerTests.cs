using Moq;
using AuthService.ApplicationApi.Application.Command.Auth;
using AuthService.Domain.Models;
using AuthService.Domain.Services;

namespace AuthService.UnitTest
{
    public class RefreshTokenHandlerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly RefreshTokenHandler _handler;

        public RefreshTokenHandlerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _handler = new RefreshTokenHandler(_userServiceMock.Object);
        }

        [Fact]
        public async Task Handle_Returns_Null_When_Refresh_Fails()
        {
            _userServiceMock.Setup(s => s.RefreshTokenAsync(It.IsAny<string>()))
                            .ReturnsAsync((AuthResult?)null);

            var result = await _handler.Handle(
                new RefreshTokenRequest { RefreshToken = "expired" }, default);

            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_Returns_Response_When_Refresh_Succeeds()
        {
            _userServiceMock.Setup(s => s.RefreshTokenAsync("valid"))
                            .ReturnsAsync(new AuthResult { JwtToken = "new-jwt", RefreshToken = "new-refresh" });

            var result = await _handler.Handle(
                new RefreshTokenRequest { RefreshToken = "valid" }, default);

            Assert.NotNull(result);
            Assert.Equal("new-jwt", result.Token);
            Assert.Equal("new-refresh", result.RefreshToken);
        }
    }
}
