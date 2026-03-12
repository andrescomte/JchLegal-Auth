using MediatR;

namespace AuthService.ApplicationApi.Application.Command.Role
{
    public class CreateRoleRequest : IRequest<CreateRoleResponse?>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
