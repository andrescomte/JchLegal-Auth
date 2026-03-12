using MediatR;

namespace AuthService.ApplicationApi.Application.Command.Auth
{
    public class AuthenticateRequest : IRequest<AuthenticateResponse?>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
