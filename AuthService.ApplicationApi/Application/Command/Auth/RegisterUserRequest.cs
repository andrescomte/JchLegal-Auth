using MediatR;

namespace AuthService.ApplicationApi.Application.Command.Auth
{
    public class RegisterUserRequest : IRequest<RegisterUserResponse?>
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string RoleCode { get; set; } = "USER";
    }
}
