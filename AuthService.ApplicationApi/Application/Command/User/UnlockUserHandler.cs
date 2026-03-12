using MediatR;
using AuthService.Domain.Repository;

namespace AuthService.ApplicationApi.Application.Command.User
{
    public class UnlockUserHandler : IRequestHandler<UnlockUserRequest, bool>
    {
        private readonly IUsersRepository _usersRepository;

        public UnlockUserHandler(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        public async Task<bool> Handle(UnlockUserRequest request, CancellationToken cancellationToken)
        {
            var user = await _usersRepository.GetByIdAsync(request.Id);
            if (user == null) return false;

            await _usersRepository.UnlockUserAsync(request.Id);
            return true;
        }
    }
}
