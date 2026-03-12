using MediatR;
using AuthService.Domain.Repository;

namespace AuthService.ApplicationApi.Application.Command.User
{
    public class UpdateUserHandler : IRequestHandler<UpdateUserRequest, bool>
    {
        private readonly IUsersRepository _usersRepository;

        public UpdateUserHandler(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        public async Task<bool> Handle(UpdateUserRequest request, CancellationToken cancellationToken)
        {
            var user = await _usersRepository.GetByIdAsync(request.Id);
            if (user == null) return false;

            user.Username = request.Username;
            user.Email = request.Email;

            await _usersRepository.UpdateUserAsync(user);
            return true;
        }
    }
}
