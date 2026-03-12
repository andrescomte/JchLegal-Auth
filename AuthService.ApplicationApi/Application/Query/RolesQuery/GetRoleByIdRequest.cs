using MediatR;

namespace AuthService.ApplicationApi.Application.Query.RolesQuery
{
    public class GetRoleByIdRequest : IRequest<GetRoleByIdResponse?>
    {
        public int Id { get; set; }
    }
}
