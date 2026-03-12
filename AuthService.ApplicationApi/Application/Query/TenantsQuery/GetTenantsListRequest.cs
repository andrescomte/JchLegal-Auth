using MediatR;
using AuthService.Domain.SeedWork;

namespace AuthService.ApplicationApi.Application.Query.TenantsQuery
{
    public class GetTenantsListRequest : IRequest<PagedResponse<GetTenantsListResponse>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
