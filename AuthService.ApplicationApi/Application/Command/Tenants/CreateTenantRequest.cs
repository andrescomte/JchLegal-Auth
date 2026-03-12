using MediatR;
using AuthService.ApplicationApi.Application.Query.TenantsQuery;

namespace AuthService.ApplicationApi.Application.Command.Tenant
{
    public class CreateTenantRequest : IRequest<GetTenantsListResponse?>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
