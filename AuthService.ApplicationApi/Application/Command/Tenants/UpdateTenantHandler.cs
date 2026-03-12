using MediatR;
using AuthService.Domain.Repository;
using AuthService.ApplicationApi.Application.Query.TenantsQuery;

namespace AuthService.ApplicationApi.Application.Command.Tenant
{
    public class UpdateTenantHandler : IRequestHandler<UpdateTenantRequest, GetTenantsListResponse?>
    {
        private readonly ITenantsRepository _tenantsRepository;

        public UpdateTenantHandler(ITenantsRepository tenantsRepository)
        {
            _tenantsRepository = tenantsRepository;
        }

        public async Task<GetTenantsListResponse?> Handle(UpdateTenantRequest request, CancellationToken cancellationToken)
        {
            var tenant = await _tenantsRepository.UpdateTenantAsync(request.Id, request.Name);
            if (tenant == null) return null;

            return new GetTenantsListResponse
            {
                Id = tenant.Id,
                Code = tenant.Code,
                Name = tenant.Name,
                IsActive = tenant.IsActive,
                CreatedAt = tenant.CreatedAt
            };
        }
    }
}
