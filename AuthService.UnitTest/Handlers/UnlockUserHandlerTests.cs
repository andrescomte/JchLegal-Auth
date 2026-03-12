using Moq;
using AuthService.ApplicationApi.Application.Command.User;
using AuthService.Domain.Models;
using AuthService.Domain.Repository;

namespace AuthService.UnitTest.Handlers
{
    public class UnlockUserHandlerTests
    {
        private readonly Mock<IUsersRepository> _repoMock = new();
        private readonly UnlockUserHandler _handler;

        public UnlockUserHandlerTests()
        {
            _handler = new UnlockUserHandler(_repoMock.Object);
        }

        [Fact]
        public async Task Handle_Returns_False_When_User_Not_Found()
        {
            _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Users?)null);

            var result = await _handler.Handle(new UnlockUserRequest { Id = 99 }, default);

            Assert.False(result);
            _repoMock.Verify(r => r.UnlockUserAsync(It.IsAny<long>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Returns_True_And_Unlocks_When_User_Exists()
        {
            var user = new Users { Id = 1, Username = "blocked-user" };
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
            _repoMock.Setup(r => r.UnlockUserAsync(1)).Returns(Task.CompletedTask);

            var result = await _handler.Handle(new UnlockUserRequest { Id = 1 }, default);

            Assert.True(result);
            _repoMock.Verify(r => r.UnlockUserAsync(1), Times.Once);
        }
    }
}
