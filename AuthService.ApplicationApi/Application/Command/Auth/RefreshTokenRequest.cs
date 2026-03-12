using MediatR;

namespace AuthService.ApplicationApi.Application.Command.Auth
{
    public class RefreshTokenRequest : IRequest<AuthenticateResponse?>
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
