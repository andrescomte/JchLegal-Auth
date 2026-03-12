using MediatR;

namespace AuthService.ApplicationApi.Application.Query.UsersQuery
{
    public class GetUserByIdRequest : IRequest<GetUsersListResponse?>
    {
        public long Id { get; set; }
    }
}
