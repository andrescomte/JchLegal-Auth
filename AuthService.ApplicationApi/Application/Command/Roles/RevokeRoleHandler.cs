using MediatR;
using AuthService.Domain.Repository;

namespace AuthService.ApplicationApi.Application.Command.Role
{
    public class RevokeRoleHandler : IRequestHandler<RevokeRoleRequest, bool>
    {
        private readonly IUsersRepository _usersRepository;

        public RevokeRoleHandler(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        public async Task<bool> Handle(RevokeRoleRequest request, CancellationToken cancellationToken)
        {
            await _usersRepository.RevokeRoleAsync(request.UserId, request.RoleId);
            return true;
        }
    }
}
