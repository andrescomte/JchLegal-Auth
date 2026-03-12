using MediatR;
using AuthService.Domain.SeedWork;

namespace AuthService.ApplicationApi.Application.Query.UsersQuery
{
    public class GetUsersListRequest : IRequest<PagedResponse<GetUsersListResponse>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
