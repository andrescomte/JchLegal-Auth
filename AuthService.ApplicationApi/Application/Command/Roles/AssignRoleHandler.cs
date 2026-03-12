using MediatR;
using AuthService.Domain.Repository;

namespace AuthService.ApplicationApi.Application.Command.Role
{
    public class AssignRoleHandler : IRequestHandler<AssignRoleRequest, bool>
    {
        private readonly IUsersRepository _usersRepository;

        public AssignRoleHandler(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        public async Task<bool> Handle(AssignRoleRequest request, CancellationToken cancellationToken)
        {
            await _usersRepository.AssignRoleAsync(request.UserId, request.RoleId);
            return true;
        }
    }
}
