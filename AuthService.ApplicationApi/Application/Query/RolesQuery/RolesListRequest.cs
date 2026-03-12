using MediatR;
using AuthService.Domain.SeedWork;

namespace AuthService.ApplicationApi.Application.Query.RolesQuery
{
    public class RolesListRequest : IRequest<PagedResponse<RolesListResponse>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}