using MediatR;

namespace AuthService.ApplicationApi.Application.Command.Auth
{
    public class LogoutRequest : IRequest<bool>
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
