using MediatR;

namespace AuthService.ApplicationApi.Application.Query.TenantsQuery
{
    public class GetTenantByIdRequest : IRequest<GetTenantsListResponse?>
    {
        public int Id { get; set; }
    }
}
