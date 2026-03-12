using MediatR;
using AuthService.ApplicationApi.Application.Query.TenantsQuery;

namespace AuthService.ApplicationApi.Application.Command.Tenant
{
    public class UpdateTenantRequest : IRequest<GetTenantsListResponse?>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
