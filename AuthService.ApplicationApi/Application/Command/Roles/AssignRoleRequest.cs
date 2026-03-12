using MediatR;

namespace AuthService.ApplicationApi.Application.Command.Role
{
    public class AssignRoleRequest : IRequest<bool>
    {
        public long UserId { get; set; }
        public int RoleId { get; set; }
    }
}
