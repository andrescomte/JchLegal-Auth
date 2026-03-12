using MediatR;
using AuthService.Domain.Repository;
using AuthService.ApplicationApi.Application.Query.TenantsQuery;
using AuthService.Domain.Models;

namespace AuthService.ApplicationApi.Application.Command.Tenant
{
    public class CreateTenantHandler : IRequestHandler<CreateTenantRequest, GetTenantsListResponse?>
    {
        private readonly ITenantsRepository _tenantsRepository;

        public CreateTenantHandler(ITenantsRepository tenantsRepository)
        {
            _tenantsRepository = tenantsRepository;
        }

        public async Task<GetTenantsListResponse?> Handle(CreateTenantRequest request, CancellationToken cancellationToken)
        {
            if (await _tenantsRepository.CodeExistsAsync(request.Code))
                return null;

            var tenant = Tenants.Create(request.Code, request.Name);

            var created = await _tenantsRepository.CreateTenantAsync(tenant);

            return new GetTenantsListResponse
            {
                Id = created.Id,
                Code = created.Code,
                Name = created.Name,
                IsActive = created.IsActive,
                CreatedAt = created.CreatedAt
            };
        }
    }
}
