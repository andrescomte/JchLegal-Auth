using MediatR;

namespace AuthService.ApplicationApi.Application.Command.Auth
{
    public class ForgotPasswordRequest : IRequest<ForgotPasswordResponse?>
    {
        public string Email { get; set; } = string.Empty;
    }
}
