using MediatR;
using AuthService.Domain.Repository;

namespace AuthService.ApplicationApi.Application.Query.RolesQuery
{
    public class GetRoleByIdHandler : IRequestHandler<GetRoleByIdRequest, GetRoleByIdResponse?>
    {
        private readonly IRolesRepository _rolesRepository;

        public GetRoleByIdHandler(IRolesRepository rolesRepository)
        {
            _rolesRepository = rolesRepository;
        }

        public async Task<GetRoleByIdResponse?> Handle(GetRoleByIdRequest request, CancellationToken cancellationToken)
        {
            var role = await _rolesRepository.GetByIdAsync(request.Id);
            if (role == null)
                return null;

            return new GetRoleByIdResponse { Id = role.Id, Code = role.Code, Name = role.Name };
        }
    }
}
