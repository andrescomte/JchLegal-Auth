using MediatR;

namespace AuthService.ApplicationApi.Application.Command.Role
{
    public class UpdateRoleRequest : IRequest<UpdateRoleResponse?>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
