using MediatR;
using AuthService.Domain.Repository;

namespace AuthService.ApplicationApi.Application.Query.TenantsQuery
{
    public class GetTenantByIdHandler : IRequestHandler<GetTenantByIdRequest, GetTenantsListResponse?>
    {
        private readonly ITenantsRepository _tenantsRepository;

        public GetTenantByIdHandler(ITenantsRepository tenantsRepository)
        {
            _tenantsRepository = tenantsRepository;
        }

        public async Task<GetTenantsListResponse?> Handle(GetTenantByIdRequest request, CancellationToken cancellationToken)
        {
            var tenant = await _tenantsRepository.GetByIdAsync(request.Id);
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
