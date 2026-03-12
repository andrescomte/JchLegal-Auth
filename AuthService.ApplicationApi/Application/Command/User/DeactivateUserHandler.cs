using MediatR;
using AuthService.Domain.Repository;

namespace AuthService.ApplicationApi.Application.Command.User
{
    public class DeactivateUserHandler : IRequestHandler<DeactivateUserRequest, bool>
    {
        private readonly IUsersRepository _usersRepository;

        public DeactivateUserHandler(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        public async Task<bool> Handle(DeactivateUserRequest request, CancellationToken cancellationToken)
        {
            var user = await _usersRepository.GetByIdAsync(request.Id);
            if (user == null) return false;

            await _usersRepository.DeactivateUserAsync(request.Id);
            return true;
        }
    }
}
