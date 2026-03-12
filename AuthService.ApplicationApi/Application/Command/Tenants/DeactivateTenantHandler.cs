using MediatR;
using AuthService.Domain.Repository;

namespace AuthService.ApplicationApi.Application.Command.Tenant
{
    public class DeactivateTenantHandler : IRequestHandler<DeactivateTenantRequest, bool>
    {
        private readonly ITenantsRepository _tenantsRepository;

        public DeactivateTenantHandler(ITenantsRepository tenantsRepository)
        {
            _tenantsRepository = tenantsRepository;
        }

        public async Task<bool> Handle(DeactivateTenantRequest request, CancellationToken cancellationToken)
        {
            return await _tenantsRepository.DeactivateTenantAsync(request.Id);
        }
    }
}
