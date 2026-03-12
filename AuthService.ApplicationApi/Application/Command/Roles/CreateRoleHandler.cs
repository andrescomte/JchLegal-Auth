using MediatR;
using AuthService.Domain.Models;
using AuthService.Domain.Repository;

namespace AuthService.ApplicationApi.Application.Command.Role
{
    public class CreateRoleHandler : IRequestHandler<CreateRoleRequest, CreateRoleResponse?>
    {
        private readonly IRolesRepository _rolesRepository;

        public CreateRoleHandler(IRolesRepository rolesRepository)
        {
            _rolesRepository = rolesRepository;
        }

        public async Task<CreateRoleResponse?> Handle(CreateRoleRequest request, CancellationToken cancellationToken)
        {
            if (await _rolesRepository.ExistsAsync(request.Code.Trim().ToUpperInvariant()))
                return null;

            var role = Roles.Create(request.Code, request.Name);
            var created = await _rolesRepository.CreateRoleAsync(role);

            return new CreateRoleResponse { Id = created.Id, Code = created.Code, Name = created.Name };
        }
    }
}
